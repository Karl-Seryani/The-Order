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

1. **Event Bus** — ALL system-to-system communication via `GameEvents.cs` (18 event groups). No direct references between systems.
2. **ScriptableObject Data** — all gameplay values in SOs (`HunterConfig`, `ItemData`, `ClueData`, `EndingData`). Never hardcode in MonoBehaviours.
3. **New Input System Only** — never use `UnityEngine.Input`. Use `.inputactions` asset via `PlayerInputHandler`.
4. **`TheOrder` Namespace** — every script. Sub-namespaces: `Player`, `Hunter`, `Items`, `Doors`, `Clues`, `Ending`, `UI`, `Audio`, `PlayerCamera`.
5. **IInteractable Interface** — all interactables implement it. `PlayerInteraction` raycasts → `Interact()`. 7 types: DoorController, LockedDoor, SlidableFurniture, ItemPickup, ToolReceiver, ScrewInteractable, CluePickup.
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
- `ItemPickup` — world pickups (keys → inventory, tools → hand)
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

### Endings (partially implemented)
- `EndingData` SO — 9 ending combinations (3 knowledge levels × 3 choices)
- `EndingCutscene` — exists but currently disabled (returns early)

### UI
- `HUDManager` — interaction prompts, crosshair, clue panel, item notifications, objective fade
- `DeathScreenUI` — fade to black, "YOU DIED", death stinger, scene reload
- `MainMenuUI` — Play/Tutorial/Settings/Quit, background music
- `ObjectiveManager` — objective text management

### Audio
- `AudioConfig` SO — centralized audio tuning
- `AmbientAudioManager` — ambient sound management
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

---

## Upcoming

- [ ] Death cinematic sequence (reverted — needs full redesign with better animations)
- [ ] Hiding system (locker assets imported, no C# mechanic yet)
- [ ] Ending system (EndingData SOs exist, EndingCutscene disabled, needs car escape flow)
- [ ] Settings panel UI (button exists, panel empty)
- [ ] Pause menu (GameState.Paused works, no menu UI)
- [x] Item progression system (ItemPickup → HeldItemController → ToolReceiver → LockedDoor chain works)
- [x] Clue collection system (18 notes, knowledge levels, journal)
- [x] Audio system (breathing FSM, door sounds, item impacts, ambient, flashlight click)
