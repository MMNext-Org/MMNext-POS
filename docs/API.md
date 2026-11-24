# BREWFLOW POS — API Reference: Loyalty & Privacy Workflows

This document describes the server-side contracts for loyalty enrollment/disenrollment/redemption and customer privacy export/erasure. All operations are tenant-scoped and authorized through Supabase RLS + RPC authorization checks.

---

## 1. Loyalty Workflow RPCs

All loyalty RPCs accept `p_user_id` (calling user UUID) and `p_org_id` (organization UUID) for authorization. Organization scope is enforced server-side; cross-tenant calls are denied.

### `rpc_loyalty_enroll`

Enrolls a customer in the loyalty program and creates a loyalty account.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `p_user_id` | uuid | Calling user (must belong to `p_org_id`) |
| `p_customer_id` | uuid | Customer to enroll |
| `p_org_id` | uuid | Organization scope |
| `p_consent_version` | text | Consent version recorded at enrollment |

**Behavior:**
- Idempotent: duplicate enrollment for the same customer/org returns the existing account without duplicate ledger entries.
- Creates a loyalty account with zero available points and "Standard" tier.
- Writes an opening-balance ledger entry.
- Fails if the customer does not exist or the user is not a member of the organization.

**Returns:** loyalty account row (`loyalty_account_id`, `points_balance`, `enrolled_at`, `status`).

### `rpc_loyalty_disenroll`

Closes a loyalty account and forfeits the available balance.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `p_user_id` | uuid | Calling user |
| `p_customer_id` | uuid | Customer to disenroll |
| `p_org_id` | uuid | Organization scope |

**Behavior:**
- Rejected while a redemption reservation is active for the account.
- Writes a forfeiture ledger entry reducing available points to zero; `forfeited_points` is recorded.
- Account status becomes `disenclosed`; historical ledger rows are preserved.
- Re-enrollment starts at zero points; forfeited points are never restored.
- Inactive/disenrolled accounts cannot earn, reserve, or redeem.

**Returns:** success boolean.

### `rpc_loyalty_redeem_reserve`

Reserves points for a reward (phase 1 of two-phase redemption).

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `p_user_id` | uuid | Calling user |
| `p_customer_id` | uuid | Customer redeeming points |
| `p_org_id` | uuid | Organization scope |
| `p_points` | integer | Points to reserve (positive) |
| `p_reward_id` | uuid | Reward being redeemed |
| `p_branch_id` | uuid | Branch scope (nullable) |
| `p_idempotency_key` | uuid | Unique key preventing duplicate reservation |

**Behavior:**
- Locks the account row during mutation to prevent concurrent double-spend.
- Rejects if available balance < points (no overdraft).
- Rejects if account is not `active`.
- Creates a `reserved` reservation row; points are held but not yet deducted from available balance.
- Idempotent: same `p_idempotency_key` returns the existing reservation without creating a duplicate.

**Returns:** reservation row (`reservation_id`, `status`, `points`, `reserved_at`).

### `rpc_loyalty_redeem_commit`

Commits a reserved redemption (phase 2 of two-phase redemption).

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `p_reservation_id` | uuid | Reservation to commit |
| `p_user_id` | uuid | Calling user |

**Behavior:**
- Only reservations in `reserved` state can be committed.
- Writes a redeem ledger transaction deducting points from the account.
- Reservation state transitions to `committed`.
- Idempotent: re-committing an already-committed reservation is a no-op.

**Returns:** success boolean.

### `rpc_loyalty_redeem_release`

Releases a reserved redemption back to the available balance.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `p_reservation_id` | uuid | Reservation to release |
| `p_user_id` | uuid | Calling user |

**Behavior:**
- Reservation must be in `reserved` state.
- Points are returned to the available balance; reservation state becomes `released`.
- Idempotent: releasing an already-released/expired reservation is a no-op.

### `rpc_loyalty_redeem_expire`

Expires a reserved reservation after a timeout.

**Parameters:** same as `rpc_loyalty_redeem_release`.

**Behavior:** transitions `reserved` → `expired` and returns points to available balance.

### `rpc_loyalty_redeem_quote`

Computes the dollar value of a points redemption without mutating state.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `p_points` | integer | Points to quote |
| `p_org_id` | uuid | Organization scope |

**Returns:** `{ points, discountAmount }` where `discountAmount = points / 100` (100 points = $1.00).

---

## 2. Reservation State Machine

```
quoted  →  reserved  →  committed
                    ↓
                 released
                    ↓
                 expired
```

- **quoted**: estimate only, no points held (client-side calculation).
- **reserved**: points held, unavailable for other redemptions; can be committed, released, or expired.
- **committed**: points deducted; redemption is final. Refunds reverse via a reversal ledger entry.
- **released**: points returned to available balance; reservation closed.
- **expired**: points returned after timeout; reservation closed.

Only `reserved` reservations can be committed, released, or expired. `committed`, `released`, and `expired` are terminal states.

---

## 3. Idempotency

All mutation RPCs accept an `idempotency_key` (or derive one from the reservation ID). The server:
1. Checks for an existing record with the same idempotency key within the organization.
2. If found, returns the existing result without creating a duplicate.
3. If not found, performs the mutation and records the key.

Retries with the same key are safe and produce exactly one effect.

---

## 4. Privacy Export — Versioned JSON

### Endpoint / Flow

1. User navigates to **Privacy → Data Export**.
2. Client calls `handleExport()`, which assembles the export document from local Supabase query results (customers, loyalty accounts, transactions, consent history).
3. Client renders the JSON and offers `downloadExport()` to save it as a file.

### JSON Schema (version `1.0`)

```json
{
  "version": "1.0",
  "exportedAt": "2026-01-15T10:30:00.000Z",
  "source": "BREWFLOW POS - Privacy Data Export",
  "organizationId": "org-uuid",
  "exportedBy": "user-uuid",
  "customers": [
    {
      "profile": {
        "id": "customer-uuid",
        "name": "Jane Doe",
        "email": "jane@example.com",
        "phone": "+1-555-0100",
        "consent": false,
        "createdAt": "2026-01-01T00:00:00.000Z",
        "updatedAt": "2026-01-01T00:00:00.000Z"
      },
      "loyaltyAccount": {
        "id": "account-uuid",
        "pointsBalance": 250,
        "tier": "Standard",
        "createdAt": "2026-01-01T00:00:00.000Z",
        "updatedAt": "2026-01-01T00:00:00.000Z"
      },
      "transactions": [
        {
          "id": "txn-uuid",
          "type": "earn",
          "points": 100,
          "balanceAfter": 100,
          "reason": "Coffee purchase",
          "referenceId": "order-uuid",
          "createdAt": "2026-01-05T00:00:00.000Z",
          "updatedAt": "2026-01-05T00:00:00.000Z"
        }
      ],
      "consentHistory": [
        {
          "id": "consent-uuid",
          "consentType": "loyalty_enrollment",
          "consented": true,
          "consentVersion": "1.0",
          "consentedAt": "2026-01-01T00:00:00.000Z",
          "createdAt": "2026-01-01T00:00:00.000Z"
        }
      ]
    }
  ],
  "summary": {
    "customerCount": 1,
    "accountCount": 1,
    "transactionCount": 1,
    "consentRecordCount": 1,
    "totalPoints": 250
  }
}
```

### Export Rules

- **UTF-8** encoded, pretty-printed JSON (`2`-space indent).
- Empty histories are represented as empty arrays (`[]`), not omitted.
- A customer with no loyalty account has `"loyaltyAccount": null`.
- Download filename: `brewflow-privacy-export-v1.0-<YYYY-MM-DD>.json`.
- No credentials, secret configuration, internal authorization data, or other-tenant/customer records are included.
- Export is scoped to the calling user's organization only.

---

## 5. Privacy Consent & Retention RPCs

### `rpc_record_consent`

Records a consent decision for a customer.

| Parameter | Type | Description |
|-----------|------|-------------|
| `p_user_id` | uuid | Calling user |
| `p_customer_id` | uuid | Customer |
| `p_org_id` | uuid | Organization scope |
| `p_consent_type` | text | `loyalty_enverollment` \| `marketing` \| `data_processing` \| `terms_of_service` |
| `p_consented` | boolean | Consent given or withdrawn |
| `p_consent_version` | text | Policy/consent version |

### `rpc_set_retention_policy`

Sets the retention period for a data type.

| Parameter | Type | Description |
|-----------|------|-------------|
| `p_user_id` | uuid | Calling user |
| `p_org_id` | uuid | Organization scope |
| `p_data_type` | text | Data type key |
| `p_retention_period_days` | integer | Retention period in days |
| `p_auto_delete` | boolean | Whether to auto-delete after retention |

### `rpc_apply_retention`

Applies retention policies: identifies expired records for deletion/anonymization.

| Parameter | Type | Description |
|-----------|------|-------------|
| `p_user_id` | uuid | Calling user |
| `p_org_id` | uuid | Organization scope |
| `p_data_type` | text | Optional: single data type, or null for all |

**Returns:** list of expired records (`data_type`, `record_id`, `days_past_retention`, `action`).

---

## 6. Order Type (Loyalty Fields)

The `Order` type includes optional loyalty fields:

| Field | Type | Description |
|-------|------|-------------|
| `customerId` | string \| undefined | Customer associated with the order |
| `discountAmount` | number \| undefined | Loyalty discount applied (dollars) |
| `redemptionReference` | string \| undefined | Order ID used as redemption reference |
| `subtotal` | number | Order subtotal before discount |

---

## 7. Points-to-Dollar Conversion

```
100 points = $1.00 discount
discountAmount = points / 100
```

All amounts are integer points and integer currency minor units throughout; rounding is deterministic.

---

## 8. Error Handling

- RPC errors propagate to the client as Supabase error objects.
- Client surfaces errors via inline error messages (`setError`).
- Redemption failures abort order creation ("Order not created").
- Offline orders are stored to `localStorage` and synced when connectivity returns.