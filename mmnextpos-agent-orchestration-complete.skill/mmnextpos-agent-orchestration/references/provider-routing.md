# Provider Routing Reference

Use this reference when a user asks to divide analysis among NVIDIA-hosted or other OpenAI-compatible models.

## Boundary

Keep provider calls advisory and separate from the coding host. The provider may propose plans, code locations, test cases, or review findings, but it must not directly edit the repository, execute local commands, migrate a database, or publish a release.

## Configuration

Read the endpoint and credentials from environment variables. A common configuration is:

```text
NVIDIA_API_BASE=https://integrate.api.nvidia.com/v1
NVIDIA_API_KEY=<secret held outside the repository>
NVIDIA_MODEL_PLANNER=<model id verified from the account>
NVIDIA_MODEL_CODER=<model id verified from the account>
NVIDIA_MODEL_REVIEWER=<model id verified from the account>
NVIDIA_MODEL_FAST=<model id verified from the account>
```

Discover models from `/v1/models` when the provider supports it. If discovery or authentication fails, stop and report the failure. Do not guess model IDs. Do not claim that any model is free: availability, quotas, terms, and pricing are account-dependent.

## Role routing

| Role | Send | Avoid |
|---|---|---|
| Planner | Requirement, architecture summary, focused file names, acceptance criteria | Full repository or secrets |
| Coding advisor | Redacted relevant excerpts and constraints | Connection strings, tokens, personal data |
| Reviewer | Diff excerpt, test output, acceptance criteria | Unrelated files and database dumps |
| Fast classifier | Short requirement and layer names | Sensitive context |

## Redaction

Before sending context, remove API keys, passwords, bearer tokens, connection strings with credentials, user/customer data, license keys, private certificates, and large binary content. Prefer a short summary plus selected excerpts. If safe redaction cannot be guaranteed, do not call the provider.

## Approval

Present the advisory result to the user as untrusted analysis. Reconcile it with repository instructions and local tests. Require explicit approval before editing, running mutating commands, changing schemas, or sending additional context.
