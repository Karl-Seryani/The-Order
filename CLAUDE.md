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

---

## Architecture Rules

1. **Event Bus** — ALL system-to-system communication via `GameEvents.cs`. No direct references between systems.
2. **ScriptableObject Data** — all gameplay values in SOs. Never hardcode in MonoBehaviours.
3. **New Input System Only** — never use `UnityEngine.Input`. Use `.inputactions` asset.
4. **`TheOrder` Namespace** — every script (sub-namespaces OK: `TheOrder.Player`, etc.).
5. **IInteractable Interface** — all interactables implement it. Raycast → `Interact()`.
6. **NavMesh Validation** — `NavMesh.SamplePosition()` before every `SetDestination()`.
7. **URP Volume Overrides** — no legacy post-processing.
8. **PlayerMoved Always Fires** — every frame (even speed 0) for Hunter vision detection.
9. **Hunter Is Silent** — footsteps only. No breathing or voice. Chase music is allowed as ambient atmosphere (not from the Hunter).
10. **`_isPaused` Flag** — never toggle `enabled` on event-driven MonoBehaviours (triggers OnDisable).
11. **Static Batching** — uncheck Static on any object that moves at runtime (SlidableFurniture, etc.).

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
- Furniture name patterns: `SheetRackCase_*` = drawers, `Cupboard_Door_*`, `MirrorShelf_Door*`, `MedRackDoor_*`, `Case_Door_*`.

---

## Upcoming

- [ ] Item progression system (Granny-style: items → unlock areas → more items → keys → escape)
- [ ] Hiding system (locker assets imported, no C# mechanic yet)
- [ ] Ending system
- [ ] Settings panel UI
- [ ] Pause menu
