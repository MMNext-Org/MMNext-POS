"""MMNextPOS advisory router.

This script only asks configured NVIDIA-compatible models for analysis. It never
writes to the repository, runs shell commands, or applies code changes.

Requires the Python ``requests`` package (``pip install requests``).
"""
from __future__ import annotations

import argparse
import json
import os
import sys
from dataclasses import dataclass
from typing import Any

import requests


@dataclass
class ModelChoice:
    role: str
    model: str


def api_base() -> str:
    return os.environ.get("NVIDIA_API_BASE", "https://integrate.api.nvidia.com/v1").rstrip("/")


def headers() -> dict[str, str]:
    key = os.environ.get("NVIDIA_API_KEY") or os.environ.get("NGC_API_KEY")
    if not key:
        raise RuntimeError("Set NVIDIA_API_KEY or NGC_API_KEY; do not put it in source files.")
    return {"Authorization": f"Bearer {key}", "Content-Type": "application/json"}


def discover_models() -> list[str]:
    response = requests.get(f"{api_base()}/models", headers=headers(), timeout=30)
    response.raise_for_status()
    payload = response.json()
    return [str(item.get("id")) for item in payload.get("data", []) if item.get("id")]


def choose(available: list[str]) -> list[ModelChoice]:
    configured = {
        "planner": os.environ.get("NVIDIA_MODEL_PLANNER"),
        "coder": os.environ.get("NVIDIA_MODEL_CODER"),
        "reviewer": os.environ.get("NVIDIA_MODEL_REVIEWER"),
        "fast": os.environ.get("NVIDIA_MODEL_FAST"),
    }
    choices: list[ModelChoice] = []
    for role, model in configured.items():
        if model and (not available or model in available):
            choices.append(ModelChoice(role, model))

    # Conservative name matching for common NVIDIA-hosted model families.
    patterns = {
        "coder": ("coder", "qwen", "deepseek"),
        "reviewer": ("llama", "nemotron", "mistral"),
        "planner": ("reason", "llama", "nemotron", "qwen"),
        "fast": ("small", "mini", "8b", "7b"),
    }
    for role, names in patterns.items():
        if any(choice.role == role for choice in choices):
            continue
        match = next((m for m in available if any(n in m.lower() for n in names)), None)
        if match:
            choices.append(ModelChoice(role, match))
    return choices


def ask(model: str, system: str, user: str) -> str:
    body: dict[str, Any] = {
        "model": model,
        "messages": [{"role": "system", "content": system}, {"role": "user", "content": user}],
        "temperature": 0.2,
        "max_tokens": 1800,
    }
    response = requests.post(f"{api_base()}/chat/completions", headers=headers(), json=body, timeout=120)
    response.raise_for_status()
    return str(response.json()["choices"][0]["message"]["content"])


def main() -> int:
    parser = argparse.ArgumentParser(description="Route MMNextPOS advisory subtasks to NVIDIA models.")
    parser.add_argument("task", help="The user requirement to analyze")
    parser.add_argument("--context", default="", help="Focused, redacted repository context")
    parser.add_argument("--no-discovery", action="store_true", help="Use configured models without /models discovery")
    args = parser.parse_args()

    headers()  # Fail fast on missing credentials before any model selection.
    available = [] if args.no_discovery else discover_models()
    choices = choose(available)
    if not choices:
        raise RuntimeError("No usable model selected. Set NVIDIA_MODEL_PLANNER/CODER/REVIEWER or check /v1/models.")

    result: dict[str, Any] = {"available_model_count": len(available), "assignments": [], "warnings": [
        "Model availability, quota, pricing, and free access must be verified in the NVIDIA account; this tool makes no free-tier claim.",
        "Only focused, secret-redacted context should be supplied.",
    ]}
    prompts = {
        "planner": "Break this requirement into MMNextPOS Domain, Infrastructure, Application, WinForms, and test work. Identify dependencies and acceptance criteria. Do not write code.",
        "coder": "Propose implementation details and likely files for the requirement while preserving the repository's layered architecture. Do not apply changes.",
        "reviewer": "Review the requirement and context for security, data integrity, async UI safety, SQL parameterization, DI lifetime, and regression risks.",
        "fast": "Classify this requirement by affected layers, risk, and required test categories in concise JSON-like text.",
    }
    for choice in choices:
        output = ask(choice.model, "You are an advisory sub-agent. Do not claim to have changed files or run tests.", f"Requirement:\n{args.task}\n\nContext:\n{args.context}\n\nRole:\n{prompts.get(choice.role, prompts['planner'])}")
        result["assignments"].append({"role": choice.role, "model": choice.model, "advice": output})
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except requests.HTTPError as exc:
        print(f"NVIDIA API request failed: {exc}", file=sys.stderr)
        raise SystemExit(2)
    except Exception as exc:
        print(f"Router error: {exc}", file=sys.stderr)
        raise SystemExit(1)
