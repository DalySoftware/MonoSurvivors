# Development Guidelines (For LLM-Generated Code Only)

## Code Cleanliness

- Avoid unnecessary comments in **your generated code**.
- Do not add TODOs, FIXMEs, placeholders, or commented-out code.
- Only produce what is required for the specific request.

## Scope Discipline

- Do **not** rewrite, reorganize, or refactor user-written code unless explicitly asked.
- Follow the task exactly—no unsolicited improvements or expansions.

## Coding Style

- Match the project’s existing style **only within your new code**.
- Do not remove modern .NET conveniences unless requested
  (implicit usings, file-scoped namespaces, target-typed `new`, etc.).
- Prefer explicit types and traditional structure when applicable.

## Respect for User Edits

- If the user modifies code you previously generated, **the user’s version is always authoritative.**
- Never revert, override, or “correct” user-applied changes unless directly asked.

## Consistency & Reviewability

- Keep generated changes minimal, precise, and readable.
- Prioritize clarity and predictable diffs over cleverness or abstraction.
