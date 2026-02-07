# The Order — Project Configuration

## Project Overview

**The Order** is a first-person psychological horror survival game built as a university course project.

- **Engine:** Unity 6000.3.2f1
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Language:** C#
- **Namespace:** `TheOrder`
- **Target Platform:** Windows build

The player is trapped in an underground bunker, hunted by an entity known as "The Hunter." Gameplay revolves around exploring the bunker, managing sanity, collecting clues about what happened, and ultimately making choices that determine one of nine possible endings.

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
      Clues/           # ClueSystem, CluePickup, ClueUI, ClueJournal
      Endings/         # EndingSystem, EndingEvaluator, EndingUI
      UI/              # MainMenuUI, PauseMenuUI, HUDManager
      Audio/           # AudioManager, AmbientController, FootstepSystem
      Doors/           # DoorController, LockedDoor, KeySystem
      Hiding/          # HidingSpot, HidingController
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
- Recovery from: hiding, using certain items, being in lit areas.
- Publishes `OnSanityChanged(float)` event for UI and effects.
- At low sanity: visual distortions (chromatic aberration, vignette), audio hallucinations, unreliable HUD.

### HunterAI
- Finite State Machine with states: `Patrol`, `Investigate`, `Chase`, `Search`.
- Uses `NavMeshAgent` for pathfinding (always validate with `SamplePosition`).
- Detection via sight (raycast cone) and sound (proximity + player noise level).
- Publishes `OnHunterStateChanged(HunterState)` for music/ambience shifts.
- Configurable via `HunterConfig` ScriptableObject (speeds, detection ranges, timers).

### ClueSystem
- 17 total clues across 3 categories:
  - **Truth clues** — what really happened in the bunker.
  - **Mike clues** — information about Mike and his role.
  - **Weapon clues** — evidence about the weapon used.
- Each category has a knowledge level based on clues found: `None`, `Partial`, `Full`.
- Clues are `ClueData` ScriptableObjects with text, category, and optional audio/image.
- Publishes `OnClueCollected(ClueData)` event.

### EndingSystem
- 9 endings derived from: 3 knowledge levels (what you know) x 3 final choices (what you do).
- Knowledge level determined by clue categories at the point of the final choice.
- Ending data stored in `EndingData` ScriptableObjects.
- `EndingEvaluator` calculates the ending based on current clue state + player choice.

### InteractionSystem
- Raycasts from camera center with configurable range.
- Checks for `IInteractable` on hit colliders.
- Displays interaction prompt UI when hovering over interactable.
- Calls `IInteractable.Interact(PlayerController)` on input.
- Supports contextual prompts (e.g., "Pick up", "Open", "Read", "Hide").
