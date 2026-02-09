# Working Rules

## Task Execution
- Never do more than ONE task at a time. Complete it, report back, then move on.
- After each task, report:
  1. What was done and what was found
  2. How it was tested (unit test, manual verification, etc.)
  3. How to test it in-game (exact steps the user should take in Unity)
- Wait for confirmation before starting the next task.

## Documentation
- After each important phase, update ALL relevant files:
  - CLAUDE.md (architecture, systems reference, implementation progress)
  - Memory files (session progress, lessons learned)
- Keep docs in sync with actual code state. No stale documentation.

## Planning
- Default to planning and discussing before writing code.
- Present approach, get approval, then implement.
- When suggesting features or changes, be honest and critical — no sugarcoating.

## Session Discipline
- **Commit every 30-60 minutes.** A half-working committed feature beats a fully-planned uncommitted one.
- **One clear target per session.** Don't try to fix AI + doors + animations + death cam all at once.
- **End every session with /handoff.** Write what worked, what failed, and what to try next.

## Debugging Rules
- **2-attempt rule is HARD.** After 2 failed fixes for the same bug, STOP. Do not try a third.
- Instead: re-diagnose from scratch. Ask "what do you actually see in the editor?"
- **Explore before fixing.** Spawn an Explore agent to map all related scripts before touching code. Most regressions came from jumping into fixes without understanding the full picture.
- **Change ONE thing at a time.** Never batch fixes.

## Accountability (Both Ways)
- If the user is trying to cram too many goals into one session — call it out.
- If the user hasn't committed in over an hour — remind them.
- If I'm on my second fix attempt and it's not working — the user should say "stop, what do you think is actually happening?" Enforce this.
- If I start refactoring or "improving" code that isn't broken — the user should kill it.
- We hold each other accountable. This is a team.
