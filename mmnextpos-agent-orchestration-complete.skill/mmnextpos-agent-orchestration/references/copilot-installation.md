# GitHub Copilot and VS Code installation

## Understand the two layers

The skill package is an instruction package for a coding agent. GitHub Copilot does not automatically read a `.skill` archive. To use the workflow in Copilot, copy the generated `.agent.md` profiles into the repository's `.github/agents/` directory, and keep project-wide rules in `AGENTS.md` or `.github/copilot-instructions.md`. The NVIDIA router is a separate local script that the agent may invoke only when the user approves and the host supports terminal tools.

## Repository-scoped installation

1. Back up or commit current changes in `J:\Project 1\MMNext POS`.
2. Extract the setup ZIP to a temporary folder.
3. Copy `.github\agents\*.agent.md` into `J:\Project 1\MMNext POS\.github\agents\`. Do not overwrite existing profiles without reviewing the diff.
4. Copy the router and launcher into a project tools directory such as `J:\Project 1\MMNext POS\tools\mmnextpos-agent\`.
5. Copy only the secret-free environment example. Do not copy a real key.
6. Review the files, then commit them on a dedicated branch if the repository is under Git.
7. Open the project root in VS Code, ensure the GitHub Copilot extension is signed in and enabled, and reload the window.
8. Open Chat/Agent mode and select `mmnextpos-orchestrator`. Use the planner and reviewer handoffs when offered.

## GitHub-hosted Copilot

For Copilot cloud-agent use, the repository must contain and commit the profiles under `.github/agents/`. On GitHub, open the Copilot agents area, select the repository, refresh after the profile is merged into the default branch, and select the custom agent from the agent dropdown. Availability depends on the Copilot product, repository settings, and preview status.

## User-level VS Code installation

If the same profile is needed across workspaces, use VS Code's custom-agent editor or its documented user-level custom-agent directory. Prefer repository scope for MMNextPOS so the instructions travel with the project and are reviewed with source control.

## First-run test

Use a harmless read-only prompt such as:

```text
Inspect the solution structure and propose a plan to add a small unit-tested validation rule. Do not edit files, run commands, call external APIs, or access secrets. Stop at the approval gate.
```

Confirm that the agent reports affected layers and asks for approval. Only then test a small approved change. Run the NVIDIA router separately with redacted context and verify that missing credentials or unavailable models produce a clear error. Never use the key as a prompt or commit it.
