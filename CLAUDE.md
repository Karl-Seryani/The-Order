# The Order — Project Configuration

## Project Overview

**The Order** is a first-person psychological horror survival game built as a university course project.

- **Engine:** Unity 6000.3.2f1
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Language:** C#
- **Namespace:** `TheOrder`
- **Target Platform:** Windows build

The player (John) is trapped in an underground bunker, hunted by his brother Mike ("The Hunter") — a mute operative whose tongue was surgically removed by The Order. Gameplay revolves around exploring the bunker, collecting clues about what happened, and ultimately making choices that determine the ending. There are no hiding spots — survival depends on outrunning the Hunter and breaking line of sight.

---

## Working Style

Do NOT write or scaffold code unless explicitly asked. Default to planning, discussing, and explaining first. Wait for an explicit go-ahead before generating any implementation.

When fixing bugs: STOP after 2 failed fix attempts and re-analyze the root cause from scratch instead of iterating on symptoms. Ask what is actually being observed in the editor before trying another code-level fix. Prefer simple, minimal changes over refactors. Change only ONE thing at a time.

When teaching or learning (crypto, Django, PortSwigger labs, etc.): guide through concepts and let the user write the code. Do not implement solutions. Ask what the next step should be before showing it.

---

## Architecture Rules

1. **Event Bus Communication** — ALL system-to-system communication goes through `GameEvents.cs`. No system may hold a direct reference to another system. Systems subscribe to events and publish events. This is non-negotiable.

2. **ScriptableObject Data** — ALL game data (clue definitions, ending conditions, hunter configs, sanity parameters, audio settings) must live in ScriptableObjects. Never hardcode gameplay values in MonoBehaviour scripts.

3. **New Input System Only** — Use Unity's New Input System package exclusively. Never use `UnityEngine.Input` (the legacy API). All input is routed through Input Actions defined in the project's `.inputactions` asset.

4. **TheOrder Namespace** — Every project script must be inside the `TheOrder` namespace (or a sub-namespace like `TheOrder.Player`, `TheOrder.Hunter`, etc.).

5. **IInteractable Interface** — All objects the player can interact with must implement the `IInteractable` interface. The interaction system uses raycasts and calls `IInteractable.Interact()` on hit objects.

6. **NavMesh Validation** — Before every `NavMeshAgent.SetDestination()` call, validate the target position with `NavMesh.SamplePosition()`. Never assume a position is on the NavMesh.

7. **URP Volume Overrides** — All post-processing effects (vignette, color grading, chromatic aberration for sanity, etc.) must use URP Volume overrides. Do not use legacy post-processing.

8. **PlayerMoved Always Fires** — `PlayerController` fires `GameEvents.PlayerMoved` every frame (even at speed 0) so systems like HunterAI can detect stationary players via vision cone.

9. **Hunter Cannot Vocalize** — Mike's tongue was surgically removed. HunterAudio is footsteps only. No breathing, no voice, no chase stinger on the Hunter.

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
      Player/          # PlayerController, PlayerInput, Flashlight, Stamina, PlayerAudio
      Hunter/          # HunterAI, HunterStateMachine, HunterStates, HunterAudio
      Sanity/          # SanityManager, SanityEffects, SanityUI
      Clues/           # ClueManager, CluePickup, ClueData
      Endings/         # EndingSystem, EndingEvaluator, EndingUI
      UI/              # MainMenuUI, PauseMenuUI, HUDManager, DeathScreenUI
      Audio/           # AudioConfig, AmbientAudioManager
      Doors/           # DoorController, LockedDoor, KeySystem
      Camera/          # CameraController, HeadBob, CameraShake
      Prologue/        # PrologueManager, PrologueSequence
      Core/            # GameManager, GameEvents, GameState, IInteractable
      Editor/          # Setup utilities, DarkBunkerSetup, FixAnimationImport
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
- Singleton pattern (manual, not base class).
- Manages game state FSM: `MainMenu`, `Prologue`, `Playing`, `Paused`, `Ending`.
- Handles scene loading/unloading transitions.
- Persists across scenes via `DontDestroyOnLoad`.
- `SetState()` has a same-state guard — always go through GameManager, never fire `GameEvents.GameStateChanged` directly.

### SanityManager
- Sanity is a `float` value clamped between `0` and `100`. Starts at `75`.
- Passive drain over time (configurable rate via ScriptableObject).
- Accelerated drain from events: darkness, seeing the Hunter, finding disturbing clues.
- Recovery from: being in lit areas, using certain items.
- Publishes `OnSanityChanged(float)` event for UI and effects.
- At low sanity: visual distortions (chromatic aberration, vignette), audio hallucinations, unreliable HUD.
- No sanity bar on HUD — effects are the feedback.
- **Status: deferred / non-functional / optional.**

### HunterAI
- Finite State Machine: `Patrol` → `Investigate` → `Chase` (no separate Search — merged into Investigate).
- `HunterStateMachine.cs` (plain C#) manages transitions, fires `GameEvents.HunterStateChanged`.
- `HunterAI.cs` (MonoBehaviour) owns the FSM, NavMeshAgent, detection, and event subscriptions.
- `PatrolState`, `InvestigateState`, `ChaseState` implement `IHunterState` interface.
- Uses `NavMeshAgent` for pathfinding (always validate with `SamplePosition`).
- **Vision detection:** `CanSeePlayer()` uses `Physics.RaycastAll` on all layers from EyePoint to player center mass (+0.8 Y). Skips self-colliders, first non-self hit determines visibility. No `_obstructionLayer` — any hit that isn't the player is an obstruction.
- **Flashlight cone detection:** `IsFlashlightHittingTarget()` — if Hunter is inside the player's flashlight cone (60°), detected regardless of Hunter's facing direction. Triggers investigate.
- **Detection ranges:** 3m dark vision, 24m flashlight (8x multiplier), 8m sprint hearing, 3m walk proximity, 15m door sounds.
- **Sound detection:** sprint <8m, walk <3m proximity, door open <15m. Sounds trigger investigate during BOTH Patrol AND Investigate states.
- Ignores door sounds within 3m (self-opened doors).
- Opens closed doors via forward raycast + `GetComponentInParent<DoorController>()`. Closes doors 3s after passing.
- Same dimensions as player (height 1.6, radius 0.28) — never gets stuck in doorways.
- 26 patrol waypoints across 3 floors under `--- WAYPOINTS ---` parent.
- LookingAround animation plays during waypoint idle.
- 3-second LOS grace period before losing chase target.
- Catch = instant death → DeathScreenUI fade → scene reload (no checkpoints).
- Uses `_isPaused` flag (not `enabled = false`) to pause during non-Playing states. Never toggle `enabled` on event-driven MonoBehaviours.
- Animator Controller: Idle/Walking/Running/LookingAround states, driven by `Speed` float + `IsLooking` bool.
- Configurable via `BunkerHunterConfig` ScriptableObject (speeds, detection ranges, timers).
- Door layer (layer 9) excludes doors/frames from NavMesh bake — walkable paths through doorways.
- Player must be on layer 8 (Player) for vision detection to work.
- Publishes `OnHunterStateChanged(HunterState)`, `OnPlayerDetected`, `OnPlayerLost`, `OnPlayerCaught`.
- **Mike cannot vocalize** — tongue surgically removed. HunterAudio is footsteps only.
- `PlayerController` reports intended speed (`CurrentSpeed`: 3.0 walk / 5.5 sprint) not `CharacterController.velocity` for reliable detection.

### HunterAudio
- Footsteps only — walk and run clips on a 3D spatial AudioSource.
- Interval scales with NavMeshAgent speed (0.55s walk, 0.3s run).
- No breathing, no voice, no chase stinger (Mike is mute — lore clue Mike_07).

### PlayerAudio
- Manages John's breathing reactions via event bus.
- **Idle breathing** (`Frozen_Loop_Mono`): plays after 5s of no WASD input (requires player to have moved at least once first).
- **Chase shock** (`Shocked_Mono_02`): one-shot gasp on `OnPlayerDetected`.
- **Post-chase relief** (`Mouth_Normal_Loop_Mono`): loops for 4s after `OnPlayerLost`.
- 2D AudioSource (spatialBlend = 0) — first-person audio.
- State machine: Moving → Idle → InChase → PostChase.

### DeathScreenUI
- Subscribes to `GameEvents.OnPlayerCaught`.
- Sequence: `GameManager.SetState(Paused)` → enable canvas → fade black 0.5s → fade "YOU DIED" text 0.3s → hold 1.5s → reload scene.
- Uses `Time.unscaledDeltaTime` and `WaitForSecondsRealtime` for pause-safe timing.
- Must go through `GameManager.SetState()` (never fire `GameStateChanged` directly) to keep state in sync for scene reload.

### ClueSystem
- 18 total clues across 2 categories:
  - **Truth clues (11)** — what really happened in the bunker.
  - **Mike clues (7)** — information about Mike and his role (includes Mike_07 Medical Report documenting tongue removal).
- Each category has a knowledge level based on clues found: `None`, `Low`, `Medium`, `High`.
- Clues are `ClueData` ScriptableObjects with text, category, and optional audio/image.
- Two-state pickup: first E reads the clue (shows reading panel), second E collects it.
- HUD shows per-category counter: `Truth: 0/11` / `Mike: 0/7`.
- Publishes `OnClueViewed(ClueData)` and `OnClueCollected(ClueData)` events.
- No journal — objective displayed via fade in/out at top-center (Tab key or auto on change).

### EndingSystem
- Endings derived from: 2 knowledge categories (Truth, Mike) x knowledge levels x final choices.
- Knowledge level per category determined by clues collected at the point of the final choice.
- Ending data stored in `EndingData` ScriptableObjects.
- `EndingEvaluator` calculates the ending based on current clue state + player choice.
- **Status: not yet implemented.**

### MainMenuUI
- `MainMenuUI.cs` — sets `GameState.MainMenu` in `Start()`.
- Play button → `GameManager.Instance.LoadScene("Prologue")`.
- Tutorial button → shows `TutorialUI` panel (3-section tabbed: Controls, Survival Tips, Clues & Endings).
- Quit button → `Application.Quit()`.
- Public `ShowMainMenu()` for tutorial back button.
- **Must use `InputSystemUIInputModule`** on EventSystem (not `StandaloneInputModule`) — project uses New Input System exclusively.

### TutorialUI
- `TutorialUI.cs` — multi-page tutorial with tab navigation.
- 3 sections: Controls, Survival Tips, Clues & Endings. Each has 1 page.
- Tab buttons jump to section. Prev/Next cycle pages. Back returns to main menu.
- Content hardcoded as static strings (not ScriptableObject — static UI text).

### WakeUpSequence
- `WakeUpSequence.cs` — coroutine-based first-person wake-up cinematic.
- Camera starts tilted 90° Z-roll (lying sideways on bed), blink overlay fully black.
- 3 blinks: Z-roll lerps 90°→60°→30°→0° with overlay fades simulating eye blinks.
- Final rise uses smoothstep easing for natural motion.
- Disables `FirstPersonCamera.IsEnabled` during sequence to prevent mouse look overriding Z-roll.
- `BunkerSceneBootstrap` sets `GameState.Prologue` first, then calls `WakeUpSequence.Begin()`.
- On completion: sets `GameState.Playing` → input enables, HUD appears, Hunter starts patrol.
- Replays on death + scene reload (wake-up is part of scene lifecycle).

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
- [x] `GameEvents.cs` — full event bus (incl. `OnPlayerCaught`, `OnClueViewed`, `OnObjectiveChanged`)
- [x] `GameManager.cs` — singleton, state FSM, scene management
- [x] `IInteractable.cs` — interaction interface
- [x] `Enums.cs` — all game enums (ClueCategory: Truth, Mike)
- [x] `ClueData.cs`, `EndingData.cs`, `HunterConfig.cs` — ScriptableObject data
- [x] `PrologueManager.cs` — prologue system
- [x] `InputSystem_Actions.inputactions` — input bindings (WASD, mouse, gamepad)
- [x] Test infrastructure (EditMode + PlayMode asmdefs)
- [x] `PlayerInputHandler.cs` — input caching, pause disable
- [x] `PlayerStamina.cs` — drain/regen math, sprint gating
- [x] `PlayerController.cs` — CharacterController movement, walk + sprint only, always fires PlayerMoved
- [x] `PlayerInteraction.cs` — raycast interaction, IInteractable detection
- [x] `PlayerFlashlight.cs` — spotlight toggle
- [x] `PlayerAudio.cs` — idle breathing, chase shock gasp, post-chase relief breathing
- [x] `FirstPersonCamera.cs` — manual mouse look (pitch/yaw)
- [x] `BunkerSceneBootstrap.cs` — sets GameState.Playing
- [x] Bunker scene (Asylum prefab, dark lighting, URP volume, player hierarchy, 18 clue pickups)
- [x] `PlayerStaminaTests.cs` — EditMode tests for stamina math
- [x] `ClueManager.cs` — tracks collected clues, knowledge levels per category
- [x] `CluePickup.cs` — two-state interaction (read → collect)
- [x] `HUDManager.cs` — interaction prompt, clue notification, clue reading panel, objective fade, per-category counter
- [x] `ObjectiveManager.cs` — objective text management, fade in/out on Tab
- [x] `DoorController.cs` — doors open/close with E, Hunter-navigable
- [x] `UILayoutSetup.cs` — editor utility for HUD layout
- [x] 18 ClueData ScriptableObjects (11 Truth + 7 Mike, includes Mike_07 Medical Report)
- [x] Hunter AI — FSM (Patrol/Investigate/Chase), NavMesh pathfinding, door opening + auto-close
- [x] Hunter vision detection — RaycastAll on all layers, EyePoint at Y=1.2, flashlight doubles range
- [x] Hunter sound detection — sprint <12m, walk <2m, door <15m, ignores self-opened <3m
- [x] Hunter Animator Controller — Walking/Running/LookingAround from Mixamo Humanoid FBX clips
- [x] LookingAround animation plays during patrol waypoint idle
- [x] `BunkerHunterConfig` ScriptableObject with tuned values
- [x] 26 patrol waypoints across 3 floors
- [x] Door layer (layer 9) — doors/frames excluded from NavMesh bake
- [x] `DeathScreenUI.cs` — fade-to-black death screen on catch, scene reload
- [x] `HunterAudio.cs` — footsteps only (Mike is mute)
- [x] `PlayerAudio.cs` — idle/chase/post-chase breathing
- [x] `AudioConfig.cs` + `AmbientAudioManager.cs` — ambient audio, door SFX, player footsteps
- [x] Dark bunker (80 lights disabled, ambient near-black, flashlight only)
- [x] `DarkBunkerSetup.cs` — editor tool for light management
- [x] Mixamo FBX clips reimported as Humanoid, stripped keepOriginalPositionY
- [x] 57 EditMode tests passing (21 detection + 8 state machine + 28 existing)
- [x] Flashlight cone detection — `IsFlashlightHittingTarget()` with 60° cone, 8x range multiplier
- [x] PlayerController reports intended speed (not CharacterController.velocity) for reliable detection
- [x] Sound registration works in Patrol AND Investigate states
- [x] Hunter Idle state uses LookingAround clip (no T-pose)
- [x] `PlayerFacingChanged` event for flashlight direction tracking
- [x] `MainMenuUI.cs` — Play/Tutorial/Quit with `InputSystemUIInputModule`
- [x] `TutorialUI.cs` — 3-section tabbed tutorial (Controls, Survival Tips, Clues & Endings)
- [x] `WakeUpSequence.cs` — first-person wake-up with camera blink + rise from bed
- [x] `FirstPersonCamera.cs` — `IsEnabled` property for cutscene camera control
- [x] `PlayerInputHandler.cs` — handles Prologue/MainMenu/Ending states (disables input)
- [x] `BunkerSceneBootstrap.cs` — triggers wake-up sequence before gameplay
- [x] `HUDManager.cs` — suppresses objective display during wake-up
- [x] MainMenu scene with full UI (created via `SetupMainMenuScene.cs` editor utility)
- [x] WakeUpCanvas + WakeUpSequence in Bunker scene (created via `SetupWakeUpSequence.cs`)
- [x] Build settings: MainMenu=0, Prologue=1, Bunker=2

### Upcoming
- [ ] SanityManager (deferred — non-functional/optional)
- [ ] EndingSystem + evaluator (knowledge levels x final choices)
- [ ] Pause menu UI
- [ ] Place CluePickup for Mike_07 Medical Report in scene
- [ ] Audio polish (ambient sounds, more footstep variety)
