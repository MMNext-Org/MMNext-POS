---
name: mmnextpos-agent-orchestration
description: Reusable workflow for setting up or improving approval-based AI coding agents for .NET desktop projects, especially MMNextPOS-style layered WinForms systems. Use when inspecting a repository, creating Copilot custom agents, routing advisory subtasks to an OpenAI-compatible provider such as NVIDIA, adding reviewer handoffs, or packaging a Windows installer/configuration.
---

# MMNextPOS Agent Orchestration

Use this skill to create a safe, reusable coding-agent setup for a local .NET project. Separate planning, implementation, review, and user approval. Include public project conventions and generic agent patterns, but never copy private platform instructions, hidden prompts, credentials, or proprietary skill files.

## Workflow

1. **Confirm access and scope.** Determine whether the repository is attached, uploaded, or available through Git. If local access is unavailable, request a source archive or repository URL. Do not claim to have inspected paths that cannot be read.
2. **Inspect read-only first.** Identify solution/project files, target frameworks, architecture, test projects, build/test commands, existing `AGENTS.md` or Copilot instructions, CI workflows, secret/configuration patterns, and dangerous operations. Preserve existing instructions rather than replacing them.
3. **Define the orchestration contract.** Use four roles: orchestrator, planner, implementer, and reviewer. The orchestrator decomposes a request; the planner is read-only; the implementer edits only after approval; the reviewer checks the diff and evidence. Use handoffs where the host supports them.
4. **Create host-compatible custom agents.** Prefer repository-scoped `.github/agents/*.agent.md` profiles for Copilot/VS Code-compatible workflows. Include YAML frontmatter with a descriptive name, description, tools, and optional handoffs. Keep prompts concise and project-specific.
5. **Add the approval gate.** Before edits, builds, tests with side effects, migrations, publishing, or external API calls, show affected files, task breakdown, commands, risks, rollback notes, and acceptance criteria. Stop and ask for explicit approval. Do not treat prior approval as permanent.
6. **Add advisory model routing only when needed.** If an external OpenAI-compatible provider is requested, keep it separate from the coding host. The bundled router requires the Python `requests` package (`pip install requests`). Read credentials from a managed secret store or OS user-scoped environment variables; never place keys in agent files, `.env` files committed to Git, archives, prompts, logs, or chat. Discover available models at runtime where possible; allow role-based environment-variable overrides; use bounded retries and clear authentication/quota errors; never claim a model is free without checking the user’s account and current terms. Send only focused, redacted context. The router must not edit the repository or run commands.
7. **Preserve project safety.** For layered .NET systems, keep domain logic framework-independent, keep SQL parameterized, preserve async UI safety, respect DI lifetimes and nullable annotations, and add tests in the appropriate layer. Never expose keys, passwords, connection strings, dumps, or personal data.
8. **Validate.** Run syntax checks for generated scripts, validate agent frontmatter, test safe failure paths without credentials, and use the project’s own build/test commands only when the user approves and the environment supports them. Do not call external APIs merely to validate a configuration.
9. **Package and deliver.** Provide a ZIP or skill package containing agent profiles, router/launcher, secret-free environment example, installer, and concise instructions. State exactly what was inspected, changed, and not run.

## Default role behavior

### Orchestrator

Require sections for **Understanding**, **Affected areas**, **Task breakdown**, **Advisory routing**, **Proposed changes**, **Verification plan**, and **Approval required**. Decompose work by domain, persistence, application services, UI, tests, configuration, and documentation. Never silently edit or execute.

### Planner

Operate read-only. Identify files, dependencies, acceptance criteria, risks, test categories, and rollback considerations. Ask clarifying questions when a request changes data or has ambiguous behavior.

### Implementer

Edit the smallest coherent set of files only after approval. Preserve existing repository instructions. Do not rewrite unrelated code, commit secrets, run destructive commands, or alter a database without separate confirmation.

### Reviewer

Operate read-only. Check the diff against requirements, architecture direction, SQL parameterization, connection lifetime, UI-thread safety, async behavior, nullability, DI lifetime, test adequacy, and secret exposure. Classify findings by severity and report exact test evidence.

## NVIDIA/OpenAI-compatible advisory router

Use [provider-routing.md](references/provider-routing.md) for API boundaries and [secure-nvidia-config.md](references/secure-nvidia-config.md) for key storage, role mappings, redaction, retries, audit, and rotation. Keep provider settings separate from credentials: `NVIDIA_API_BASE`, `NVIDIA_MODEL_PLANNER`, `NVIDIA_MODEL_CODER`, `NVIDIA_MODEL_REVIEWER`, and `NVIDIA_MODEL_FAST` may be non-secret configuration; `NVIDIA_API_KEY` must be a secret. Verify actual model availability from the provider’s model endpoint when supported. If discovery fails, stop with an actionable error rather than guessing. Treat quota and “free” status as user-account conditions.

Use [copilot-installation.md](references/copilot-installation.md) when explaining how to use this skill’s outputs in VS Code/GitHub Copilot. A `.skill` archive is not automatically consumed by Copilot: copy repository-scoped `.agent.md` files into `.github/agents/`, keep project-wide rules in `AGENTS.md` or `.github/copilot-instructions.md`, and run the NVIDIA router as a separate local advisory component. Use [agent-profile-template.md](templates/agent-profile-template.md) when creating a new host-compatible profile. Do not add README or changelog files inside this skill; the skill itself is the reusable instruction package.
