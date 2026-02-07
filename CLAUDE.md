# The Order — Project Configuration

## Project Overview

**The Order** is a first-person psychological horror survival game built as a university course project.

- **Engine:** Unity 6000.3.2f1
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Language:** C#
- **Namespace:** `TheOrder`
- **Target Platform:** Windows build

The player is trapped in an underground bunker, hunted by an entity known as "The Hunter." Gameplay revolves around exploring the bunker, managing sanity, collecting clues about what happened, and ultimately making choices that determine the ending. There are no hiding spots — survival depends on outrunning the Hunter and breaking line of sight.

---

## Architecture Rules

1. **Event Bus Communication** — ALL system-to-system communication goes through `GameEvents.cs`. No system may hold a direct reference to another system. Systems subscribe to events and publish events. This is non-negotiable.

2. **ScriptableObject Data** — ALL game data (clue definitions, ending conditions, hunter configs, sanity parameters, audio settings) must live in ScriptableObjects. Never hardcode gameplay values in MonoBehaviour scripts.

3. **New Input System Only** — Use Unity's New Input System package exclusively. Never use `UnityEngine.Input` (the legacy API). All input is routed through Input Actions defined in the project's `.inputactions` asset.

4. **TheOrder Namespace** — Every project script must be inside the `TheOrder` namespace (or a sub-namespace like `TheOrder.Player`, `TheOrder.Hunter`, etc.).

5. **IInteractable Interface** — All objects the player can interact with must implement the `IInteractable` interface. The interaction system uses raycasts and calls `IInteractable.Interact()` on hit objects.

6. **NavMesh Validation** — Before every `NavMeshAgent.SetDestination()` call, validate the target position with `NavMesh.SamplePosition()`. Never assume a position is on the NavMesh.

7. **URP Volume Overrides** — All post-processing effects (vignette, color grading, chromatic aberration for sanity, etc.) must use URP Volume overrides. Do not use legacy post-processing.

---

## Code Style

### Naming Conventions

- **PascalCase** for public methods, properties, classes, structs, enums, and public fields (though public fields should be avoided).
- **_camelCase** (leading underscore) for private and protected fields.
- **camelCase** (no underscore) for local variables and parameters.
- **UPPER_SNAKE_CASE** for constants.

### Field Exposure

- Use `[SerializeField] private` for fields that need inspector exposure. Never use `public` fields for inspector serialization.
- Use properties with `{ get; private set; }` for read-only public access.

### Documentation

- XML doc comments (`/// <summary>`) on all public methods, properties, and classes.
- Inline comments for non-obvious logic.

### Organization

- Use `#region` blocks to organize large classes (e.g., `#region Event Handlers`, `#region State Machine`, `#region Public API`).
- One class per file. File name matches class name.

### Safety

- Always null-check `GetComponent<T>()` results.
- Always null-check event subscriptions before invoking.
- Use `TryGetComponent<T>()` where appropriate.

---

## Folder Structure

```
Assets/
  _Project/
    Scripts/
      Player/          # PlayerController, PlayerInput, Flashlight, Stamina
      Hunter/          # HunterAI, HunterStateMachine, HunterStates
      Sanity/          # SanityManager, SanityEffects, SanityUI
      Clues/           # ClueManager, CluePickup, ClueData
      Endings/         # EndingSystem, EndingEvaluator, EndingUI
      UI/              # MainMenuUI, PauseMenuUI, HUDManager
      Audio/           # AudioManager, AmbientController, FootstepSystem
      Doors/           # DoorController, LockedDoor, KeySystem
      Camera/          # CameraController, HeadBob, CameraShake
      Prologue/        # PrologueManager, PrologueSequence
      Core/            # GameManager, GameEvents, GameState, IInteractable
    Scenes/
      MainMenu/
      Prologue/
      Bunker/
      Ending/
    Prefabs/
      Player/
      Hunter/
      Clues/
      Environment/
      UI/
    ScriptableObjects/
      ClueData/
      EndingData/
      HunterConfig/
    Art/
    Audio/
    Materials/
    Animations/
    Tests/
      EditMode/
      PlayMode/
```

---

## Testing

### Philosophy

- Test every logic system: sanity math, ending determination, detection math, stamina drain, clue collection logic.
- **EditMode tests** for pure logic that does not require a running scene (math calculations, state transitions, data validation).
- **PlayMode tests** for integration scenarios that need MonoBehaviour lifecycle (interaction raycasts, scene loading, event bus integration).

### Assembly Definitions

- `TheOrder.Tests.EditMode` — references the main assembly, runs in Edit Mode.
- `TheOrder.Tests.PlayMode` — references the main assembly, runs in Play Mode.

### Test Naming

- `MethodName_Condition_ExpectedResult` (e.g., `CalculateSanityDrain_InDarkness_ReturnsFasterDrain`).

---

## Git Rules

- **Never commit:** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `*.csproj`, `*.sln`
- **Commit message format:** Conventional Commits
  - `feat:` — new feature or system
  - `fix:` — bug fix
  - `test:` — adding or updating tests
  - `docs:` — documentation changes
  - `refactor:` — code restructuring without behavior change
  - `chore:` — build, config, or tooling changes
  - `style:` — formatting, whitespace, naming
- **No Co-Authored-By tags** in any commit messages.
- **No references to AI tools** anywhere in the project — not in code, comments, commit messages, or documentation.

---

## Key Systems Reference

### GameManager
- Singleton pattern via a base `Singleton<T>` MonoBehaviour.
- Manages game state FSM: `MainMenu`, `Prologue`, `Playing`, `Paused`, `Ending`.
- Handles scene loading/unloading transitions.
- Persists across scenes via `DontDestroyOnLoad`.

### SanityManager
- Sanity is a `float` value clamped between `0` and `100`. Starts at `75`.
- Passive drain over time (configurable rate via ScriptableObject).
- Accelerated drain from events: darkness, seeing the Hunter, finding disturbing clues.
- Recovery from: being in lit areas, using certain items.
- Publishes `OnSanityChanged(float)` event for UI and effects.
- At low sanity: visual distortions (chromatic aberration, vignette), audio hallucinations, unreliable HUD.
- No sanity bar on HUD — effects are the feedback.

### HunterAI
- Finite State Machine: `Patrol` → `Investigate` → `Chase` (no separate Search — merged into Investigate).
- `HunterStateMachine.cs` (plain C#) manages transitions, fires `GameEvents.HunterStateChanged`.
- `HunterAI.cs` (MonoBehaviour) owns the FSM, NavMeshAgent, detection, and event subscriptions.
- `PatrolState`, `InvestigateState`, `ChaseState` implement `IHunterState` interface.
- Uses `NavMeshAgent` for pathfinding (always validate with `SamplePosition`).
- Detection: sight (raycast cone, flashlight doubles range) + sound (sprint <12m, walk <2m proximity, door open <15m).
- Ignores door sounds within 3m (self-opened doors).
- Opens closed doors via forward raycast + `GetComponentInParent<DoorController>()`.
- Same dimensions as player (height 1.6, radius 0.28) — never gets stuck in doorways.
- 27 manual patrol waypoints across 3 floors under `--- WAYPOINTS ---` parent.
- 3-second LOS grace period before losing chase target.
- Catch = instant death, scene reload (no checkpoints).
- Animator Controller: Idle/Walking/Running/LookingAround states, driven by `Speed` float + `IsLooking` bool.
- Configurable via `BunkerHunterConfig` ScriptableObject (speeds, detection ranges, timers).
- Door layer (layer 9) excludes doors/frames from NavMesh bake — walkable paths through doorways.
- Publishes `OnHunterStateChanged(HunterState)`, `OnPlayerDetected`, `OnPlayerLost`.

### ClueSystem
- 17 total clues across 2 categories:
  - **Truth clues (11)** — what really happened in the bunker.
  - **Mike clues (6)** — information about Mike and his role.
- Each category has a knowledge level based on clues found: `None`, `Low`, `Medium`, `High`.
- Clues are `ClueData` ScriptableObjects with text, category, and optional audio/image.
- Two-state pickup: first E reads the clue (shows reading panel), second E collects it.
- HUD shows per-category counter: `Truth: 0/11` / `Mike: 0/6`.
- Publishes `OnClueViewed(ClueData)` and `OnClueCollected(ClueData)` events.
- No journal — objective displayed via fade in/out at top-center (Tab key or auto on change).

### EndingSystem
- Endings derived from: 2 knowledge categories (Truth, Mike) x knowledge levels x final choices.
- Knowledge level per category determined by clues collected at the point of the final choice.
- Ending data stored in `EndingData` ScriptableObjects.
- `EndingEvaluator` calculates the ending based on current clue state + player choice.

### InteractionSystem
- Raycasts from camera center with configurable range.
- Checks for `IInteractable` on hit colliders.
- Displays interaction prompt UI when hovering over interactable.
- Calls `IInteractable.Interact(PlayerController)` on input.
- Supports contextual prompts (e.g., "Read", "Open/Close", "Interact").

---

## Implementation Progress

### Completed
- [x] Project scaffolding (folder structure, asmdefs, core scripts)
- [x] `GameEvents.cs` — full event bus (incl. `OnClueViewed`, `OnObjectiveChanged`)
- [x] `GameManager.cs` — singleton, state FSM, scene management
- [x] `IInteractable.cs` — interaction interface
- [x] `Enums.cs` — all game enums (ClueCategory: Truth, Mike)
- [x] `ClueData.cs`, `EndingData.cs`, `HunterConfig.cs` — ScriptableObject data
- [x] `PrologueManager.cs` — prologue system
- [x] `InputSystem_Actions.inputactions` — input bindings (WASD, mouse, gamepad)
- [x] Test infrastructure (EditMode + PlayMode asmdefs)
- [x] `PlayerInputHandler.cs` — input caching, pause disable
- [x] `PlayerStamina.cs` — drain/regen math, sprint gating
- [x] `PlayerController.cs` — CharacterController movement, walk + sprint only
- [x] `PlayerInteraction.cs` — raycast interaction, IInteractable detection
- [x] `PlayerFlashlight.cs` — spotlight toggle
- [x] `FirstPersonCamera.cs` — manual mouse look (pitch/yaw)
- [x] `BunkerSceneBootstrap.cs` — sets GameState.Playing
- [x] Bunker scene (Asylum prefab, lighting, URP volume, player hierarchy, 17 clue pickups)
- [x] `PlayerStaminaTests.cs` — EditMode tests for stamina math
- [x] `ClueManager.cs` — tracks collected clues, knowledge levels per category
- [x] `CluePickup.cs` — two-state interaction (read → collect)
- [x] `HUDManager.cs` — interaction prompt, clue notification, clue reading panel, objective fade, per-category counter
- [x] `ObjectiveManager.cs` — objective text management, fade in/out on Tab
- [x] `DoorController.cs` — doors open/close with E, Hunter-navigable
- [x] `UILayoutSetup.cs` — editor utility for HUD layout
- [x] 17 ClueData ScriptableObjects (11 Truth + 6 Mike)
- [x] Hunter AI — FSM (Patrol/Investigate/Chase), NavMesh pathfinding, door opening
- [x] Hunter detection — sound (sprint <12m, walk <2m, door <15m), ignores self-opened doors
- [x] Hunter Animator Controller — Walking/Running/LookingAround from Mixamo FBX clips
- [x] `BunkerHunterConfig` ScriptableObject with tuned values
- [x] 27 patrol waypoints across 3 floors
- [x] Door layer (layer 9) — doors/frames excluded from NavMesh bake
- [x] 48 EditMode tests passing (14 detection + 8 state machine + 26 existing)

### Upcoming — Hunter Phase 2B
- [ ] Vision cone detection (sight raycast with obstruction check)
- [ ] Chase catch → game over screen + scene reload
- [ ] Door closing after Hunter passes through
- [ ] Animations playing correctly in-game (walk/run/look)
- [ ] Dark bunker (remove/dim all scene lights, flashlight only)
- [ ] Audio system (ambient, footsteps, chase music stingers)
- [ ] SanityManager (deferred — non-functional/optional)
- [ ] EndingSystem + evaluator (knowledge levels x final choices)
- [ ] UI polish (pause menu, main menu)
