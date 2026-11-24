/**
 * Domain logic for loyalty point calculations.
 * All math is integer-based with deterministic round-half-up rounding to
 * avoid float drift across service boundaries.
 */

/**
 * Round a non-negative integer division result half-up.
 * Returns floor((numerator / denominator) + 0.5) using integer-only math.
 */
export function roundHalfUpDivide(numerator: number, denominator: number): number {
  if (denominator <= 0) return 0;
  return Math.floor((numerator * 2 + denominator) / (2 * denominator));
}

/**
 * Calculate points earned from a payment amount using an earn rule.
 *
 * points = floor(amount_minor_units * points_per_currency_unit / currency_minor_unit)
 *
 * Applies min_spend_minor_units threshold and max_points_per_transaction cap.
 */
export function calculatePointsEarned(
  amountMinorUnits: number,
  pointsPerCurrencyUnit: number,
  currencyMinorUnit: number,
  opts?: {
    minSpendMinorUnits?: number;
    maxPointsPerTransaction?: number;
  }
): number {
  if (amountMinorUnits <= 0 || pointsPerCurrencyUnit <= 0 || currencyMinorUnit <= 0) {
    return 0;
  }

  if (opts?.minSpendMinorUnits !== undefined && amountMinorUnits < opts.minSpendMinorUnits) {
    return 0;
  }

  // Integer math: (amount * rate) / currency_unit, floor
  const points = Math.floor(
    (amountMinorUnits * pointsPerCurrencyUnit) / currencyMinorUnit
  );

  if (opts?.maxPointsPerTransaction !== undefined) {
    return Math.min(points, opts.maxPointsPerTransaction);
  }

  return points;
}

/**
 * Prorate points for a partial refund using integer-safe round-half-up.
 *
 * restored = round_half_up(total_points * refund_amount / original_amount)
 */
export function proratePoints(
  totalPoints: number,
  refundMinorUnits: number,
  originalMinorUnits: number
): number {
  if (totalPoints <= 0 || refundMinorUnits <= 0 || originalMinorUnits <= 0) {
    return 0;
  }
  const refundRatio = Math.min(refundMinorUnits, originalMinorUnits);
  return roundHalfUpDivide(totalPoints * refundRatio, originalMinorUnits);
}

/**
 * Check whether a reward is active at a given point in time.
 */
export function isRewardActive(
  activeFrom: Date | string | null,
  activeUntil: Date | string | null,
  at: Date | string = new Date()
): boolean {
  const t = new Date(at).getTime();
  const from = activeFrom ? new Date(activeFrom).getTime() : null;
  const until = activeUntil ? new Date(activeUntil).getTime() : null;

  if (from !== null && t < from) return false;
  if (until !== null && t >= until) return false;
  return true;
}

/**
 * Check whether a customer tier is eligible for earning/redeeming.
 * null/undefined eligible_tiers means all tiers are eligible.
 */
export function isTierEligible(
  customerTier: string | null | undefined,
  eligibleTiers: string[] | null | undefined
): boolean {
  if (!eligibleTiers || eligibleTiers.length === 0) return true;
  return eligibleTiers.includes(customerTier ?? '');
}
