# The Order — Architecture Document

## Event Bus (GameEvents.cs)

All inter-system communication flows through a single static class: `GameEvents.cs`. No system holds a direct reference to another system. Instead, systems subscribe to events on enable and unsubscribe on disable.

### Event Catalog

| Event | Signature | Publisher | Subscribers |
|---|---|---|---|
| `OnGameStateChanged` | `Action<GameState>` | GameManager | All systems (disable input on pause, etc.) |
| `OnPlayerMoved` | `Action<Vector3, float>` | PlayerController | Detection System (sound propagation) |
| `OnPlayerSprinted` | `Action` | PlayerController | Detection System (20m hearing burst) |
| `OnPlayerCrouched` | `Action<bool>` | PlayerController | Detection System (silent movement) |
| `OnFlashlightToggled` | `Action<bool>` | Flashlight | Detection System (double sight range) |
| `OnInteraction` | `Action<IInteractable>` | InteractionSystem | Analytics, tutorial |
| `OnHunterStateChanged` | `Action<HunterState>` | HunterStateMachine | SanityManager, AudioManager, HUD |
| `OnPlayerDetected` | `Action` | DetectionSystem | HunterStateMachine (→ Chase) |
| `OnPlayerLost` | `Action` | DetectionSystem | HunterStateMachine (→ Search) |
| `OnSanityChanged` | `Action<float, float>` | SanityManager | SanityVFX, HUD, AudioManager |
| `OnSanityBreak` | `Action` | SanityManager | PlayerController (teleport), HunterAI (investigate) |
| `OnClueCollected` | `Action<ClueData>` | CluePickup | ClueJournal, EndingSystem, SanityManager |
| `OnDoorOpened` | `Action<Vector3>` | DoorController | Detection System (15m sound burst) |
| `OnPlayerHid` | `Action` | HidingSpot | HunterAI (check spots if saw entry) |
| `OnPlayerExitedHiding` | `Action` | HidingSpot | PlayerController (re-enable movement) |
| `OnBreathHoldFailed` | `Action` | HidingController | Detection System (noise burst) |
| `OnEndingTriggered` | `Action<EndingData>` | EndingSystem | GameManager (→ Ending state), EndingUI |

### Pattern
```csharp
// Publishing
GameEvents.PlayerSprinted();

// Subscribing
private void OnEnable() => GameEvents.OnPlayerSprinted += HandlePlayerSprinted;
private void OnDisable() => GameEvents.OnPlayerSprinted -= HandlePlayerSprinted;
```

---

## System Dependency Map

```
                         ┌──────────────┐
                         │  GameEvents   │ (central static event bus)
                         └──────┬───────┘
                                │
     ┌──────────────────────────┼──────────────────────────┐
     │                          │                           │
┌────▼─────┐            ┌──────▼──────┐            ┌───────▼──────┐
│  Player   │            │   Hunter    │            │    Sanity    │
│Controller │            │     AI      │            │   Manager    │
└────┬─────┘            └──────┬──────┘            └───────┬──────┘
     │ OnPlayerMoved            │ OnHunterStateChanged      │ OnSanityChanged
     │ OnPlayerSprinted         │ OnPlayerDetected          │ OnSanityBreak
     │ OnFlashlightToggled      │ OnPlayerLost              │
     │                          │                           │
┌────▼─────┐            ┌──────▼──────┐            ┌───────▼──────┐
│Interaction│            │  Detection  │            │  Sanity VFX  │
│  System   │            │   System    │            │ (URP Volume) │
└────┬─────┘            └─────────────┘            └──────────────┘
     │ IInteractable
     ├──► CluePickup ──► OnClueCollected ──► EndingSystem
     ├──► DoorController ──► OnDoorOpened ──► Hunter hears
     ├──► HidingSpot ──► OnPlayerHid / OnBreathHoldFailed
     └──► CameraTerminal
```

### Key Relationships
- **Player → Hunter**: Player fires movement/flashlight events; Detection System interprets them; Hunter FSM reacts.
- **Player → Sanity**: Seeing Mike, being in darkness, collecting disturbing clues all modify sanity. Sanity break teleports player.
- **Clues → Endings**: Clue collection count determines knowledge level which determines ending outcome.
- **Hiding → Hunter**: Failed QTE produces noise; Hunter investigates. Hunter checks hiding spots if player was seen nearby.

---

## ScriptableObject Data Flow

### ClueData (17 instances)
```
ClueData ScriptableObject
├── string Id           — Unique identifier (e.g., "truth_01")
├── ClueCategory        — Truth, Mike, or Weapon
├── string Title        — Display name in journal
├── string ContentText  — Full text shown when viewing
├── float SanityImpact  — Negative = drain, Positive = recovery
├── AudioClip AudioClip — Nullable, for audio log clues
└── Sprite Sprite       — Visual representation
```

### EndingData (9 instances)
```
EndingData ScriptableObject
├── EndingType          — Enum identifier (BlindViolence, Absolution, etc.)
├── string EndingName   — Display name for credits
├── KnowledgeLevel      — Low, Medium, or High
├── EndingChoice        — UseWeapon, ConfrontMike, or Flee
└── string NarrativeText — Ending sequence text
```

### HunterConfig (1 instance, tweaked during playtesting)
```
HunterConfig ScriptableObject
├── Movement: patrolSpeed, investigateSpeed, chaseSpeed, searchSpeed, chaseSpeedMultiplier
├── Sight: sightAngle, sightRange, flashlightSightMultiplier
├── Hearing: sprintHearingRadius, walkHearingRadius, doorOpenHearingRadius
├── Proximity: proximityDetectionRange
├── Detection: detectionFillRate, detectionDecayRate, detectionThreshold
├── Patrol: waypointIdleMin, waypointIdleMax
├── Investigate: investigateTimeout, investigateCheckSpots
├── Chase: losTimeout, catchDistance
└── Search: searchTimeout, searchRadius, elevatedAlertDuration
```

---

## Scene Architecture

```
┌─────────┐     ┌──────────┐     ┌────────────────────┐     ┌─────────┐
│MainMenu  │────►│ Prologue │────►│     Bunker         │────►│ Ending  │
│ Scene    │     │  Scene   │     │  (main gameplay)   │     │ Scene   │
└─────────┘     └──────────┘     │  Floor 1 (Ground)  │     └────┬────┘
                                  │  Floor 2 (Upper)   │          │
                                  │  Floor 3 (Basement)│          ▼
                                  └────────────────────┘     Main Menu
```

- **MainMenu**: New Game, Continue, Settings, Endings (unlock tracker), Quit
- **Prologue**: Country house scene, scripted sequence, blast wave, fade to bunker
- **Bunker**: Multi-floor gameplay area. Player wakes on bed. Explore, collect clues, avoid Mike, reach exit.
- **Ending**: Narrative text, monologue, credits with ending name, return to menu

### Scene Loading
- `GameManager` persists across all scenes (DontDestroyOnLoad)
- Async scene loading with loading screen for Prologue → Bunker transition
- Direct load for menu transitions

---

## Folder Structure

```
Assets/
  _Project/
    Scripts/
      Player/          # PlayerController, Flashlight, StaminaSystem
      Hunter/          # HunterStateMachine, HunterStates, DetectionSystem
      Sanity/          # SanityManager, SanityVFX
      Clues/           # ClueData (SO), CluePickup, ClueJournal
      Endings/         # EndingData (SO), EndingSystem, EndingEvaluator
      UI/              # MainMenuUI, PauseMenuUI, HUDManager
      Audio/           # AudioManager, AmbientController, FootstepSystem
      Doors/           # DoorController
      Hiding/          # HidingSpot, HidingController, QTESystem
      Camera/          # CameraTerminal
      Prologue/        # PrologueManager
      Core/            # Enums, GameEvents, GameManager, IInteractable
    Scenes/
      MainMenu/
      Prologue/
      Bunker/
      Ending/
    Prefabs/
      Player/          # Player prefab with controller, camera, flashlight
      Hunter/          # Mike prefab with NavMeshAgent, FSM, detection
      Clues/           # Clue pickup prefab (per-clue data via SO reference)
      Environment/     # Doors, hiding spots, camera terminals
      UI/              # HUD, journal, menus
    ScriptableObjects/
      ClueData/        # 17 ClueData assets
      EndingData/      # 9 EndingData assets
      HunterConfig/    # 1 HunterConfig asset
    Materials/
    Audio/
      Music/
      SFX/
      Ambience/
    Art/
      Textures/
      UI/
      Sprites/
    Animations/
    Tests/
      EditMode/        # Pure logic tests
      PlayMode/        # Integration tests
```
