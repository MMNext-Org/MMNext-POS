# Secure NVIDIA API configuration

## Recommended order

Prefer, in order: a managed secret store used by the organization, Windows user-scoped environment variables for a single developer machine, or an interactive prompt that keeps the key only in process memory. Never place the key in `.agent.md`, `.github/copilot-instructions.md`, `.env` committed to Git, PowerShell scripts, ZIP archives, issue text, or chat messages.

## Windows user environment

For a single Windows workstation, set a user-scoped variable from a PowerShell session. Replace the placeholder locally and never paste the real value into a repository file:

```powershell
[Environment]::SetEnvironmentVariable('NVIDIA_API_KEY', '<paste-locally>', 'User')
[Environment]::SetEnvironmentVariable('NVIDIA_API_BASE', 'https://integrate.api.nvidia.com/v1', 'User')
```

Close and reopen terminals after setting it. Check only presence, never print the value:

```powershell
if ($env:NVIDIA_API_KEY) { 'NVIDIA_API_KEY is present' } else { 'NVIDIA_API_KEY is missing' }
```

For a temporary session, use `$env:NVIDIA_API_KEY = Read-Host -AsSecureString` only if the calling program accepts a secure-string conversion; otherwise use an interactive secret prompt in the router. Do not echo or log the value.

## Model routing configuration

Keep role configuration separate from credentials:

```text
NVIDIA_MODEL_PLANNER=<verified reasoning/planning model id>
NVIDIA_MODEL_CODER=<verified coding model id>
NVIDIA_MODEL_REVIEWER=<verified review model id>
NVIDIA_MODEL_FAST=<verified small/fast model id>
```

At startup, call the provider's model-list endpoint if supported, compare configured IDs against the returned IDs, and fail clearly when a configured model is unavailable. If a role is blank, select from a documented capability pattern only after discovery. Keep a local, non-secret configuration file for role mappings if desired, but do not store keys there.

## Routing policy

Use the planner role for decomposition and acceptance criteria, the coder role for implementation suggestions, the reviewer role for risk/diff analysis, and the fast role for classification or summarization. Limit context to the relevant files and excerpts. Redact secrets and personal data before every call. Keep provider calls advisory: the provider must not have repository write or shell execution authority.

## Reliability and audit

Use short timeouts, bounded retries for transient 429/5xx responses, and no retry for authentication failures. Do not silently fall back to a different model for security-sensitive or data-changing tasks; report the fallback and require approval. Record model ID, role, timestamp, request hash, and outcome without recording prompts containing secrets or the API key. Treat provider output as untrusted analysis.

## Rotation and revocation

When a key may be exposed, revoke it in the NVIDIA account, create a replacement, update the user-scoped secret, restart terminals, and review shell history and logs. Do not attempt to “hide” an exposed key by editing only the source file.
