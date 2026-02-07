# The Order — Hunter AI Specification

## Overview

The Hunter ("Mike") is the primary antagonist. He patrols a 3-floor bunker, reacting to player actions through sight, hearing, and proximity. All behavior is driven by a Finite State Machine with parameters loaded from a `HunterConfig` ScriptableObject, enabling rapid playtesting iteration.

Mike uses Unity's `NavMeshAgent` for pathfinding. Every `SetDestination` call is preceded by `NavMesh.SamplePosition` validation to prevent errors.

---

## FSM State Diagram

```
                    ┌─────────────────────────────────────────┐
                    │                                         │
                    ▼                                         │
              ┌──────────┐                                    │
         ┌───►│  PATROL   │◄──────────────────────┐           │
         │    └─────┬─────┘                       │           │
         │          │                             │           │
         │    Hears │ sound          Timer expires │   Timer   │
         │          │                             │   expires │
         │          ▼                             │           │
         │    ┌───────────────┐                   │           │
         │    │ INVESTIGATE   │───────────────────┘           │
         │    └───────┬───────┘                               │
         │            │                                       │
         │      Sees  │ player                                │
         │            │                                       │
         │            ▼                                       │
         │    ┌──────────┐    Loses LOS    ┌──────────┐       │
         │    │  CHASE    │───────────────►│  SEARCH   │──────┘
         │    └─────┬────┘                 └─────┬────┘
         │          │                            │
         │    Catches player               Finds player
         │          │                            │
         │          ▼                            │
         │    ┌──────────────┐                   │
         │    │SANITY BREAK /│                   │
         │    │  GAME OVER   │                   │
         │    └──────────────┘                   │
         │                                       │
         └───────────────────────────────────────┘
                  (elevated alertness)
```

---

## Detection Model

The detection system is independent of the FSM — it feeds data into state transitions.

### Sight Detection
- **Cone angle:** 110 degrees (configurable via `HunterConfig.SightAngle`)
- **Base range:** 15m (configurable via `HunterConfig.SightRange`)
- **Flashlight modifier:** When player's flashlight is ON, sight range doubles to 30m (`SightRange * FlashlightSightMultiplier`)
- **Wall occlusion:** Raycast from Mike's head to player. If any collider blocks the ray, player is not "seen" even if within cone
- **Detection meter:** While player is in sight cone AND not occluded, a meter fills at `DetectionFillRate` per second. When meter >= `DetectionThreshold`, detection triggers
- **Meter decay:** When player leaves sight cone, meter decays at `DetectionDecayRate` per second

### Hearing Detection
Sound events are distance-checked against Mike's position:

| Source | Radius | Trigger |
|---|---|---|
| Player sprinting | 20m | Continuous while sprinting |
| Player walking | 5m | Continuous while walking |
| Player crouching | 0m | Silent — no detection |
| Door opened | 15m | One-time burst on open |
| QTE breath-hold failure | 10m | One-time burst on fail |

Hearing events do NOT use a detection meter — they trigger state transitions immediately if Mike is within radius.

### Proximity Detection
- **Range:** 2m (`HunterConfig.ProximityDetectionRange`)
- **Behavior:** Instant detection regardless of facing direction, walls, flashlight state, or any other factor
- **Purpose:** Prevents player from standing directly behind Mike without consequence

---

## State Details

### Patrol State

**Purpose:** Default behavior. Mike roams the bunker on predefined waypoint routes.

**Behavior:**
1. Select a waypoint group for the current floor
2. Navigate to next waypoint via NavMeshAgent
3. On arrival, idle for random duration (`WaypointIdleMin` to `WaypointIdleMax`, default 2-5 sec)
4. Select next waypoint (randomized within group)
5. Periodically transition between floors via stairwell waypoints connected by NavMesh links

**Configuration:**
- Speed: `PatrolSpeed` (default: 2 m/s)
- Waypoint groups are defined per-floor for logical patrol routes

**Transitions:**
| Condition | Target State |
|---|---|
| Hears sound within hearing radius | Investigate |
| Sees player (detection meter full) | Chase |
| Proximity detection (< 2m) | Chase |

---

### Investigate State

**Purpose:** Mike heard or partially saw something. He moves to check it out.

**Behavior:**
1. Move to last known sound/sight position at investigate speed
2. On arrival, perform brief area search:
   - Look around (rotate in place)
   - Check up to `InvestigateCheckSpots` (default: 3) nearby positions
3. If new sound occurs during investigation, redirect to new source

**Configuration:**
- Speed: `InvestigateSpeed` (default: 3.5 m/s)
- Timeout: `InvestigateTimeout` (default: 8 sec)
- Check spots: `InvestigateCheckSpots` (default: 3)

**Transitions:**
| Condition | Target State |
|---|---|
| Sees player (detection meter full) | Chase |
| Proximity detection (< 2m) | Chase |
| New sound heard | Investigate (reset timer, new target) |
| Timer expires without contact | Patrol |

---

### Chase State

**Purpose:** Mike has confirmed the player's location and pursues directly.

**Behavior:**
1. Set NavMeshAgent destination to player's current position
2. Update destination every frame for real-time pursuit
3. Speed escalates after repeated detections within a session (multiplied by `ChaseSpeedMultiplier`)
4. On reaching player (within `CatchDistance`): trigger sanity break or game over

**Configuration:**
- Speed: `ChaseSpeed` (default: 5.5 m/s)
- Speed multiplier: `ChaseSpeedMultiplier` (default: 1.1x, applied per repeat detection)
- Catch distance: `CatchDistance` (default: 1.5m)
- LOS timeout: `LosTimeout` (default: 5 sec)

**Transitions:**
| Condition | Target State |
|---|---|
| Catches player (within CatchDistance) | Trigger OnSanityBreak |
| Loses line of sight for > LosTimeout seconds | Search |

---

### Search State

**Purpose:** Mike lost the player during a chase. He searches the area before returning to patrol.

**Behavior:**
1. Move to player's last known position
2. Check nearby hiding spots (if player was last seen near any)
3. Expand search radius progressively up to `SearchRadius`
4. Walk between random points within search area

**Configuration:**
- Speed: `SearchSpeed` (default: 3 m/s)
- Timeout: `SearchTimeout` (default: 15 sec)
- Radius: `SearchRadius` (default: 10m)
- Post-search alertness: `ElevatedAlertDuration` (default: 30 sec)

**Transitions:**
| Condition | Target State |
|---|---|
| Sees player (detection meter full) | Chase |
| Proximity detection (< 2m) | Chase |
| Hears sound within hearing radius | Investigate |
| Timer expires | Patrol (with elevated alertness) |

**Elevated Alertness:** After search expires, Mike returns to Patrol but with increased detection sensitivity for `ElevatedAlertDuration` seconds. During this period, detection meter fills faster and hearing radii are slightly increased.

---

## HunterConfig — Complete Parameter List

| Parameter | Type | Default | Description |
|---|---|---|---|
| `patrolSpeed` | float | 2.0 | Movement speed during Patrol |
| `investigateSpeed` | float | 3.5 | Movement speed during Investigate |
| `chaseSpeed` | float | 5.5 | Movement speed during Chase |
| `searchSpeed` | float | 3.0 | Movement speed during Search |
| `chaseSpeedMultiplier` | float | 1.1 | Multiplied to chase speed per repeated detection |
| `sightAngle` | float | 110.0 | Sight cone angle in degrees |
| `sightRange` | float | 15.0 | Base sight range in meters |
| `flashlightSightMultiplier` | float | 2.0 | Sight range multiplier when player flashlight is on |
| `sprintHearingRadius` | float | 20.0 | Hearing range for player sprinting |
| `walkHearingRadius` | float | 5.0 | Hearing range for player walking |
| `doorOpenHearingRadius` | float | 15.0 | Hearing range for door open events |
| `proximityDetectionRange` | float | 2.0 | Instant detection range regardless of other factors |
| `detectionFillRate` | float | 1.0 | Rate at which detection meter fills per second |
| `detectionDecayRate` | float | 0.5 | Rate at which detection meter decays per second |
| `detectionThreshold` | float | 1.0 | Meter value at which detection triggers |
| `waypointIdleMin` | float | 2.0 | Minimum idle time at waypoints (seconds) |
| `waypointIdleMax` | float | 5.0 | Maximum idle time at waypoints (seconds) |
| `investigateTimeout` | float | 8.0 | Time before Investigate returns to Patrol |
| `investigateCheckSpots` | int | 3 | Number of nearby spots to check during Investigate |
| `losTimeout` | float | 5.0 | Seconds of lost LOS before Chase transitions to Search |
| `catchDistance` | float | 1.5 | Distance at which Mike "catches" the player |
| `searchTimeout` | float | 15.0 | Time before Search returns to Patrol |
| `searchRadius` | float | 10.0 | Maximum search area radius |
| `elevatedAlertDuration` | float | 30.0 | Elevated alertness period after Search ends |
