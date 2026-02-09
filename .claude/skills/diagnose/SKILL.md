# Diagnose — Root Cause Analysis Before Fixes

This skill enforces a structured diagnosis workflow. Use it before attempting any bug fix.

## Instructions

When the user runs /diagnose, follow this process exactly:

### Step 1: Gather Evidence
- Read ALL scripts related to the reported issue
- If Unity MCP is available, read the Unity console output
- Ask the user: "What exactly do you see happening in the editor? (visual behavior, not just error messages)"

### Step 2: Root Cause Hypothesis
List the **top 3 possible root causes**, ranked by likelihood. For each:
- State the hypothesis in one sentence
- Cite the specific code or configuration that supports it
- Rate confidence: High / Medium / Low

Present these to the user and ask: "Which of these matches what you're seeing, or is it something else?"

### Step 3: Propose Minimal Fix
Only after the user confirms the likely root cause:
- Propose the **smallest possible code change** that addresses it
- Explain exactly what it changes and why
- State what should NOT change as a result

### Step 4: Verify
After applying the fix:
- If MCP is available, read console output again
- Ask the user to confirm the behavior is fixed
- If the fix FAILS: **do NOT iterate on the same approach** — go back to Step 2 and re-analyze from scratch

## Rules
- NEVER skip to writing code before completing Steps 1-2
- NEVER change more than one thing at a time
- After 2 failed fix attempts, stop and explicitly reassess the entire diagnosis
- Do not refactor, clean up, or "improve" surrounding code — fix only the bug