# Debug Unity — Structured Unity Debugging

This skill provides a structured workflow for debugging Unity-specific issues (NavMesh, physics, animations, AI, rendering).

## Instructions

When the user runs /debug-unity, follow this process:

### Step 1: Read the Environment
1. Read the Unity console output (if MCP available)
2. Read ALL scripts that touch the reported system
3. Check relevant ScriptableObject configs for incorrect values
4. Ask: "What do you see in the Scene/Game view? Is the issue in Edit mode, Play mode, or both?"

### Step 2: Categorize the Issue
Identify which Unity subsystem is involved:
- **NavMesh/Pathfinding**: Check NavMeshAgent settings (baseOffset, speed, acceleration, areaMask), NavMesh bake settings, obstacle layers
- **Physics/Colliders**: Check Rigidbody settings, collider dimensions, layer collision matrix, trigger vs collider confusion
- **Animation**: Check Animator Controller transitions, root motion settings, avatar configuration, FBX import settings (In Place checkbox)
- **Rendering/Lighting**: Check URP settings, light bake, shader compatibility, volume overrides
- **UI**: Check Canvas render mode, EventSystem module type (must be InputSystemUIInputModule), sorting order

### Step 3: Diagnose
Apply the /diagnose workflow (Steps 1-4 from the diagnose skill).

### Step 4: Unity-Specific Checks
Before proposing any fix, verify these common Unity gotchas:
- [ ] Are serialized field defaults being overridden by saved scene values?
- [ ] Is the correct layer assigned? (Player=8, Door=9)
- [ ] Is NavMesh.SamplePosition being called before SetDestination?
- [ ] Are animations imported as Humanoid (not Generic) if retargeting?
- [ ] Is keepOriginalPositionY stripped from FBX clips?
- [ ] Are MonoBehaviours using _isPaused flag instead of enabled=false?

### Step 5: Fix and Validate
- Apply ONE change at a time
- After each change, check console output
- Ask user to confirm in the editor
- If 2 fixes fail, full re-diagnosis from Step 1