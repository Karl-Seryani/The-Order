# Handoff — Session Handoff Document

Generate a structured handoff document summarizing this session's progress for seamless continuation in the next session.

## Instructions

When the user runs /handoff, generate a markdown document and save it to `.claude/handoffs/YYYY-MM-DD.md` (create the directory if needed). Use today's date.

### Document Structure

```markdown
# Session Handoff — [DATE]

## What We Accomplished
- [Bullet list of completed work with specific file paths]

## Current State
- **Game is playable**: Yes/No
- **Tests passing**: [count] EditMode, [count] PlayMode
- **Last successful commit**: [hash + message, or "uncommitted changes"]
- **Build status**: Compiles / Has errors

## What's Still Broken
- [Issue]: [exact symptoms observed] — [file(s) involved]

## Approaches That FAILED (Do Not Repeat)
- [What was tried] — [Why it didn't work]

## Uncommitted Changes
- [List modified/new files from git status]

## Recommended Next Steps
1. [Most important next task]
2. [Second priority]
3. [Third priority]

## Key Context for Next Session
- [Any non-obvious state, workarounds in place, or gotchas to be aware of]
```

### Process
1. Run `git status` and `git log --oneline -5` to check current state
2. Review the conversation history for what was accomplished and what failed
3. Check for any console errors or test failures mentioned
4. Write the handoff document
5. Also update the memory file at `.claude/projects/-Users-karlseryani-The-Order/memory/MEMORY.md` if significant progress was made

### Rules
- Be honest about what's broken — don't sugarcoat
- Include FAILED approaches so they aren't retried
- Always include file paths so the next session can jump straight in
- Keep it concise — this is a reference doc, not a narrative