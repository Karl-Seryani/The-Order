Pre-commit validation checklist.

Steps:
1. Check Unity console for errors — must be zero
2. Run EditMode tests — all must pass
3. Verify no hardcoded values (should be in ScriptableObjects)
4. Verify TheOrder namespace on all project scripts
5. Verify no UnityEngine.Input usage (must use New Input System)
6. Check for any TODO/FIXME comments that should be resolved
