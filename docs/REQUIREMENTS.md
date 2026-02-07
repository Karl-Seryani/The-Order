# The Order — Requirements Document

## Functional Requirements

### FR-01: First-Person Player Controller
The player controls a first-person character using WASD movement with walk, sprint, and crouch modes. A stamina system (100 max) drains while sprinting and regenerates when not sprinting. Crouching reduces speed but produces no sound events. Sprint fires `OnPlayerSprinted` to alert the Hunter. Uses CharacterController component and Unity's New Input System.

### FR-02: Flashlight System
A toggleable spotlight attached to the player camera. When ON, doubles the Hunter's detection sight range (15m to 30m). No battery drain mechanic — the trade-off is detection risk. Fires `OnFlashlightToggled(bool)` event.

### FR-03: Interaction System
Physics raycast from camera center with ~2m range detects objects implementing `IInteractable`. Shows contextual UI prompt (e.g., "Press E to pick up"). All interactable objects (clues, doors, hiding spots, camera terminals) use this single interface.

### FR-04: Hunter AI — Finite State Machine
The Hunter ("Mike") uses a NavMeshAgent-based FSM with four states: Patrol (waypoint navigation), Investigate (move to sound/sight source), Chase (direct pursuit), and Search (post-chase area sweep). All parameters loaded from `HunterConfig` ScriptableObject. NavMesh validation before every `SetDestination` call.

### FR-05: Detection System
Multi-layered detection model:
- **Sight cone:** 110-degree angle, 15m range (30m when player flashlight is on), raycast wall occlusion
- **Hearing:** Sprint = 20m, Walk = 5m, Crouch = 0m, Door open = 15m burst
- **Proximity:** 2m = instant detection regardless of other factors
- **Detection meter:** Fills over time while player is in detection zone; triggers state transition at threshold

### FR-06: Sanity System
Sanity is a float value 0-100, starting at 75. Passive drain at ~1/sec. Accelerators: seeing Mike (3x drain), darkness/flashlight off (1.5x drain), disturbing clues (instant drain from ClueData.sanityImpact). Recovery: Mike-category clues (+5 sanity), safe rooms (slow regen). Fires `OnSanityChanged(current, max)`.

### FR-07: Sanity Break
At sanity = 0: screen distortion + blackout, teleport player to random room on current floor, reset sanity to 15, fire `OnSanityBreak` (triggers Mike to investigate near new position), 2-3 second invulnerability window.

### FR-08: Clue Collection System
17 collectible clues across 3 categories:
- **Truth** (~6 clues): What John did (classified order, launch codes, intercepted comms, casualty report, news clipping, dog tags)
- **Mike** (~6 clues): What Mike sacrificed (trial transcript, prison letter, medical report, photograph, diary entry, release papers)
- **Weapon** (~5 clues): Hidden gun location (armory manifest, blueprint fragment, security code, warning note, cache map)

Clue journal UI with 3 tabs, accessible via Tab key, pauses game. Each clue has: id, category, title, contentText, sanityImpact, optional audioClip and sprite.

### FR-09: Ending System
9 endings determined by: knowledge level (Low: 0-5 clues, Medium: 6-11, High: 12-17) crossed with final choice (Use Weapon, Confront Mike, Flee). Weapon choice requires 3+ weapon clues for success. Ending names: Blind Violence, Confused Rage, Hollow Escape, Guilty Execution, Bitter Standoff, Burdened Flight, Fratricide, Absolution (true ending at High + Confront), Coward's Exit. Endings tracked across sessions via PlayerPrefs.

### FR-10: Door System
Doors with rotation animation (open/close), implementing IInteractable. Some doors locked (require key/progression flag). Opening a door fires `OnDoorOpened(Vector3)` which alerts the Hunter within hearing radius (15m).

### FR-11: Hiding System
Hiding spots (lockers, under desks) implementing IInteractable. On enter: disable player movement, switch camera to hiding POV. QTE breath-hold mechanic: random WASD button prompts at ~3 second intervals, must press within 1 second. Miss = noise event alerting Mike. Mike checks hiding spots if player was seen entering the area.

### FR-12: Camera Terminal System
Fixed terminals around the bunker implementing IInteractable. Switch view to security cameras in other rooms — can see Mike through cameras for strategic advantage. Player is vulnerable while using (cannot move, no interaction).

---

## Non-Functional Requirements

### NFR-01: Performance
Minimum 60 FPS on target hardware (mid-range Windows PC). Baked lighting for performance, real-time only for flashlight, emergency lights, and flickering effects. Occlusion culling for multi-floor bunker.

### NFR-02: Rendering Pipeline
Universal Render Pipeline (URP) exclusively. All post-processing via URP Volume overrides (vignette, chromatic aberration, film grain, color grading, lens distortion). No legacy post-processing.

### NFR-03: Input System
Unity New Input System only. All input through InputActionAsset with Player action map. Actions: Move, Look, Sprint, Crouch, Interact, Flashlight, Pause, Journal. Never use `UnityEngine.Input`.

### NFR-04: Architecture — Event Bus
All inter-system communication via static `GameEvents.cs` event bus. No system holds direct references to other systems. Systems subscribe to and publish events only.

### NFR-05: Data-Driven Design
All gameplay values stored in ScriptableObjects (ClueData, EndingData, HunterConfig). No hardcoded values in MonoBehaviour scripts. This enables playtesting iteration without code changes.

### NFR-06: Code Organization
All scripts in `TheOrder` namespace. Assembly definitions for main code and test assemblies. One class per file. PascalCase for public, _camelCase for private fields. `[SerializeField]` for inspector exposure.

### NFR-07: Build Target
Windows standalone build (.exe). Must run independently without Unity Editor. Scene flow: MainMenu → Prologue → Bunker → Ending.

### NFR-08: Test Coverage
EditMode tests for all pure logic (sanity math, ending determination, detection calculations, stamina drain/regen). PlayMode tests for integration (interaction raycasts, scene loading, event bus). Test naming: `MethodName_Condition_ExpectedResult`.
