# The Order — Project Configuration

## Project Overview

**The Order** is a Granny-style first-person horror escape game (university course project).

- **Engine:** Unity 6000.3.2f1 | **Pipeline:** URP | **Language:** C# | **Namespace:** `TheOrder`
- **Target:** Windows build | **Build Settings:** MainMenu=0, Bunker=1

Player wakes up trapped, stalked by a silent killer ("The Hunter"). Find items to unlock areas, solve item-chain puzzles, and escape. Player can hide in lockers.

---

## Working Style

- Do NOT write or scaffold code unless explicitly asked. Plan and discuss first.
- When fixing bugs: STOP after 2 failed attempts — re-analyze root cause. Ask what's observed in editor. Change ONE thing at a time.
- When teaching (crypto, Django, PortSwigger, etc.): guide through concepts, don't implement. Ask what's next before showing it.
- **Always use available skills** — use `implement-feature` for new features, `new-system` for new systems, `fix-console` for console errors, `pre-commit` before committing, `playtest-check` to validate playability. Never skip skills when they match the task.

---

## Architecture Rules

1. **Event Bus** — ALL system-to-system communication via `GameEvents.cs` (20 event groups). No direct references between systems.
2. **ScriptableObject Data** — all gameplay values in SOs (`HunterConfig`, `ItemData`, `ClueData`, `EndingData`). Never hardcode in MonoBehaviours.
3. **New Input System Only** — never use `UnityEngine.Input`. Use `.inputactions` asset via `PlayerInputHandler`.
4. **`TheOrder` Namespace** — every script. Sub-namespaces: `Player`, `Hunter`, `Items`, `Doors`, `Clues`, `Ending`, `UI`, `Audio`, `PlayerCamera`.
5. **IInteractable Interface** — all interactables implement it. `PlayerInteraction` raycasts → `Interact()`. 10 types: DoorController, LockedDoor, SlidableFurniture, ItemPickup, ToolReceiver, ScrewInteractable, CluePickup, CarPartPickup, CarInstallZone, MainDoorEscapeTrigger.
6. **NavMesh Validation** — `NavMesh.SamplePosition()` before every `SetDestination()`.
7. **URP Volume Overrides** — no legacy post-processing.
8. **PlayerMoved Always Fires** — every frame (even speed 0) for Hunter vision detection.
9. **Hunter Is Silent** — footsteps only. No breathing or voice. Chase music is allowed as ambient atmosphere (not from the Hunter).
10. **`_isPaused` Flag** — never toggle `enabled` on event-driven MonoBehaviours (triggers OnDisable).
11. **Static Batching** — uncheck Static on any object that moves at runtime (SlidableFurniture, etc.).
12. **Singletons** — `GameManager`, `PlayerInventory`, `HeldItemController`, `ClueManager` persist across scenes.

---

## Systems Overview

### Player (on Player root GameObject)
- `PlayerController` — walk (3.0) / sprint (5.5) / crouch, gravity, fires `PlayerMoved` every frame
- `PlayerInputHandler` — caches New Input System values, disables input on Pause/Death/WakeUp
- `PlayerInventory` — singleton pocket for keys (static HashSet, persists across deaths)
- `PlayerInteraction` — raycast detection, priority logic for screws/lockers
- `PlayerStamina` — hidden stamina drain/regen, sprint limiter
- `PlayerFlashlight` — spotlight toggle, click SFX, doubles Hunter sight range
- `PlayerAudio` — breathing FSM (Idle/Moving/InChase/PostChase)
- `HeldItemController` — singleton, holds tools in hand, drop with gentle placement

### Camera (on Player/PlayerCamera child)
- `FirstPersonCamera` — manual mouse look (pitch + yaw), `IsEnabled` flag for sequences
- `WakeUpSequence` — cinematic blink + camera rotation on first spawn
- `PlayerAmbientLight` — very dim spotlight (0.5 intensity) on camera, 140° cone, lets player barely see ahead without flashlight

### Hunter (on Asylum/Hunter)
- `HunterAI` — main controller, sight/sound/flashlight detection, NavMesh navigation, door opening
- `HunterStateMachine` — plain C# FSM (non-MonoBehaviour)
- **Patrol** → waypoints, 2-5s idle, transitions on sight/sound
- **Investigate** → navigates to sound, looks around 4s, 8s timeout → back to patrol
- **Chase** → full-speed pursuit, repath every 0.2s, 3s LOS grace, catch at ≤1.5m
- `HunterConfig` SO — all AI tuning (speeds, ranges, timeouts, flashlight multiplier)
- `HunterAudio` — footstep sounds only

### Items
- `ItemData` SO — defines tools/keys with mesh, icon, impact audio
- `ItemPickup` — world pickups (keys → inventory, tools → hand). Destroyed on pickup, recreated by ItemSpawner on drop.
- `CarPartPickup` — scene-object pickups (car parts, drill). Hides mesh on pickup, re-shows with Rigidbody on drop. Tracks `_currentlyHeld` static for drop handling. Listens to `OnItemDropped` to intercept and destroy phantom ItemSpawner pickups.
- `ToolReceiver` — requires specific tool, spawns rewards, animates break
- `ScrewInteractable` / `ScrewLock` — screw-based locking mechanism

### Doors
- `DoorController` — smooth rotation, toggle open/close, configurable axis
- `LockedDoor` — key-based lock wrapper, unlock SFX
- `SlidableFurniture` — drawers/cabinets slide along configurable axis

### Clues
- `ClueData` SO — 18 notes with title, text, sprite, optional audio
- `ClueManager` — singleton tracker, knowledge level calculation (Low/Medium/High)
- `CluePickup` — IInteractable pickup

### Car Repair / Escape
- `CarRepairStation` — on Body_Goblin (car frame, outdoor area). Manages part installation, drill logic, car key start. Delegates zone-specific interactions to `CarInstallZone` children.
- `CarInstallZone` — child collider zones on Body_Goblin (Leftzonewheel, rightzonewheel, frontzonewheel, motor). Each references a specific `CarPartPickup`. Implements IInteractable, delegates to parent `CarRepairStation.InteractWithZone()`.
- **Parts**: Motor_4Banger, Left wheel, right wheel, front wheel — each a root scene object with `CarPartPickup` + `BoxCollider`. Wheels require drill after placement.
- **Drill** — at Gatehouse, uses `CarPartPickup` (Tool type, stays in hand when drilling wheels).
- **Car Key** — Key type (goes to inventory). Required to start car after all 4 parts installed.
- **Flow**: find parts in bunker → carry to Body_Goblin → place at zone → drill wheels → find car key → start car → `OnCarRepairComplete` → `EndingCutscene`
- **Audio**: drill sound on wheel drilling, car engine sound on start.

### Difficulty System
- `DifficultyLevel` enum — Practice, Easy, Medium, Hard (in `Enums.cs`)
- `GameManager` stores `CurrentDifficulty`, set via `SetDifficulty()` before scene load
- Convenience booleans: `HunterEnabled` (!= Practice), `HunterFullDetection` (>= Medium), `RequiresCarRepair` (Practice or Hard)
- **Practice** — no Hunter (deactivated in Start), car repair escape
- **Easy** — sight-only Hunter (sound/flashlight/door/noise detection disabled), main door escape
- **Medium** — full Hunter, main door escape
- **Hard** — full Hunter, car repair escape (original behavior)
- `MainDoorEscapeTrigger` — IInteractable on `Asylum/MainDoor/Door`, fires `CarRepairComplete` on E press. Disables itself (`enabled = false`) when `RequiresCarRepair`. `PlayerInteraction` prioritizes it over `LockedDoor` when enabled.
- `MainMenuUI` — Play button shows difficulty panel, difficulty buttons call `SetDifficulty` + `LoadScene`
- `ObjectiveManager` — difficulty-aware initial objective text

### Endings
- `EndingData` SO — 9 ending combinations (3 knowledge levels × 3 choices)
- `EndingCutscene` — wired to `OnCarRepairComplete` event
- `CarSeat` — IInteractable on car seat, enter/exit with camera transition, caches exact player position/rotation for clean restore

### UI
- `HUDManager` — interaction prompts, crosshair, clue panel, item notifications, objective fade
- `DeathScreenUI` — fade to black, "YOU DIED", death stinger, scene reload
- `MainMenuUI` — Play (→ difficulty panel) / Tutorial / Settings / Quit, background music
- `TutorialUI` — 4-section tabbed tutorial (Controls, The Hunter, Survival, Escape) with prev/next navigation
- `ObjectiveManager` — objective text management
- `HorrorFontApplier` — runtime font override on Canvas Awake. Nosifer (title, >=28pt) + Creepster (body). Attached to all canvases.

### Audio
- `AudioConfig` SO — centralized audio tuning (ambient, stingers, footsteps, outdoor/indoor ambient)
- `AmbientAudioManager` — ambient sound management, stinger playback, outdoor/indoor ambient swap
- `AudioZoneTrigger` — trigger collider that switches audio on zone transitions (config-swap or outdoor mode)
- `FloorCreakTrigger` — trigger zone plays creak SFX + fires `InteractableNoise` for Hunter alert
- `PlayerAudio` — breathing FSM
- `HunterAudio` — footstep sounds

---

## GameEvents.cs — All Events

| Event | Signature | Purpose |
|-------|-----------|---------|
| `OnGameStateChanged` | `Action<GameState>` | State transitions (MainMenu/Playing/Paused/Death/Ending) |
| `OnPlayerMoved` | `Action<Vector3, float>` | Position + speed every frame |
| `OnFlashlightToggled` | `Action<bool>` | Light on/off |
| `OnPlayerFacingChanged` | `Action<Vector3>` | Flashlight cone direction |
| `OnWakeUpStarted` | `Action` | Disable input/HUD/Hunter |
| `OnWakeUpCompleted` | `Action` | Re-enable everything |
| `OnHunterStateChanged` | `Action<HunterState>` | FSM transition |
| `OnPlayerDetected` | `Action` | Chase start |
| `OnPlayerLost` | `Action` | Chase end |
| `OnPlayerCaught` | `Action` | Death trigger |
| `OnClueViewed` | `Action<ClueData>` | Show reading panel |
| `OnClueCollected` | `Action<ClueData>` | Track + notification |
| `OnDoorUnlocked` | `Action<ItemData, Vector3>` | Key used on door |
| `OnLockedDoorAttempt` | `Action<ItemData>` | Wrong key feedback |
| `OnDoorOpened` / `OnDoorClosed` | `Action<Vector3>` | Hunter hearing |
| `OnInteractableNoise` | `Action<Vector3, float>` | Furniture/door noise for Hunter |
| `OnItemPickedUp` | `Action<ItemData>` | UI notification |
| `OnItemDropped` | `Action<ItemData, Vector3>` | Sound + Hunter alert |
| `OnItemUsed` | `Action<ItemData>` | Tool consumption |
| `OnObjectiveChanged` | `Action<string>` | HUD objective text |
| `OnCarPartInstalled` | `Action<ItemData, int, int>` | Part installed (part, count, total) |
| `OnCarRepairComplete` | `Action` | All parts installed + car started |
| `OnEndingTriggered` | `Action<EndingData>` | Ending sequence |

---

## Code Style

- **PascalCase**: public methods, properties, classes. **_camelCase**: private fields. **UPPER_SNAKE_CASE**: constants.
- `[SerializeField] private` for inspector fields. Never `public` fields.
- `/// <summary>` on all public API. `#region` blocks for large classes. One class per file.
- Null-check `GetComponent<T>()`. Use `TryGetComponent<T>()` where appropriate.

---

## Git Rules

- **Never commit:** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `*.csproj`, `*.sln`
- **Conventional Commits:** `feat:`, `fix:`, `refactor:`, `chore:`, `test:`, `docs:`, `style:`
- **No Co-Authored-By tags.** No AI references anywhere in the project.

---

## Key Constraints (non-obvious stuff)

- **Door layer = 9** — doors/frames excluded from NavMesh bake so Hunter walks through doorways.
- **Player layer = 8** — required for Hunter vision detection.
- `GameManager.SetState()` has same-state guard — never fire `GameStateChanged` directly.
- `DeathScreenUI` uses `Time.unscaledDeltaTime` / `WaitForSecondsRealtime` (pause-safe).
- `WakeUpSequence` skipped on respawn via `SkipWakeUpSequence` flag.
- `FindFirstObjectByType` in Update is expensive — cache with search-once flag.
- `CharacterController.velocity` underreports — use `PlayerController.CurrentSpeed` (3.0 walk / 5.5 sprint).
- `CharacterController` must be disabled before direct `transform.position` changes.
- Furniture name patterns: `SheetRackCase_*` = drawers, `Cupboard_Door_*`, `MirrorShelf_Door*`, `MedRackDoor_*`, `Case_Door_*`.
- Hunter skeleton path: `Asylum/Hunter/root/pelvis/spine_01/.../hand_r` (for weapon attachment).
- Mixamo FBX animations must be set to **Humanoid** rig type to match the Hunter avatar.
- Serialized field defaults are overridden by scene values — use MCP `set_property` to update scene values.
- `CarPartPickup` uses `OnEnable`/`OnDisable` for event subscription — these scene objects stay active (only renderers/colliders toggle), so subscription is stable.
- `CarPartPickup._currentlyHeld` is static — resets on domain reload. Only one car part can be held at a time.
- Car parts are root scene objects (no parent). `Place()` sets `transform.position` to world home position. Drop sets `transform.position` to drop position.
- `HunterConfig.CanOpenDoors` field controls whether Hunter can open doors (disabled for outdoor area).

---

## Upcoming

- [ ] Death cinematic sequence (reverted — needs full redesign with better animations)
- [ ] Hiding system (locker assets imported, no C# mechanic yet)
- [ ] Settings panel UI (button exists, panel empty)
- [ ] Pause menu (GameState.Paused works, no menu UI)
- [ ] Car Key pickup needs to be placed in the bunker scene
- [x] Horror UI — Nosifer + Creepster fonts, blood red/parchment palette, 1920x1080 canvas, Very High quality
- [x] Difficulty system (Practice/Easy/Medium/Hard with Hunter + escape variants)
- [x] Car repair escape system (4 parts + drill + car key → escape ending)
- [x] Item progression system (ItemPickup → HeldItemController → ToolReceiver → LockedDoor chain works)
- [x] Clue collection system (18 notes, knowledge levels, journal)
- [x] Audio system (breathing FSM, door sounds, item impacts, ambient, flashlight click, drill, car start)
- [x] Audio enhancements (MachineRoomKey cinematic stinger, outdoor forest ambient, floor creak triggers + Hunter alert)
