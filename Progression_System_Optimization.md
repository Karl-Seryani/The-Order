# Progression System Optimization: Audit, Benchmark, and Playtest

**Team:** Group 47 | **Game:** *The Order* | Karl Seryani, Raghav Gulati, Kirill Delyukin

---

## 1. Progression System Audit

### Progression Model

The Order uses **world-gated progression** rather than XP or leveling. The player advances by discovering items, solving item-chain puzzles, and unlocking new areas of the bunker. Progression is measured by access — how much of the map the player can reach — and by proximity to escape.

### Progression Map

```
START: Wake up in bedroom (Day 1)
  │
  ├─► Find Flashlight (immediate area)
  │     └─ Risk/reward tool: see items vs. attract Hunter
  │
  ├─► Explore accessible rooms → find Keys
  │     └─ Key → Locked Door → new wing/area
  │
  ├─► Find Tools (Crowbar, Screwdriver, etc.)
  │     └─ Tool → ToolReceiver (break padlock/vent) → new area + reward items
  │
  ├─► Screw-locked barriers
  │     └─ Screwdriver → unscrew all screws → ScrewLock opens
  │
  ├─► Clue Pickups (lore/narrative, scattered throughout)
  │     └─ Optional collection — story context, no mechanical gate
  │
  ├─► [Easy/Medium] Find Main Door Key → escape via main door
  │
  └─► [Practice/Hard/Nightmare] Car Repair Escape:
        ├─ Find Motor (bunker interior)
        ├─ Find 3 Wheels (scattered across bunker)
        ├─ Find Drill (Gatehouse)
        ├─ Carry each part to Body_Goblin (outdoor car frame)
        ├─ Install parts at zone colliders
        ├─ Drill wheels after placement
        ├─ Find Car Key (bunker interior)
        └─ Start car → Ending Cutscene
```

### Difficulty as Meta-Progression

| Tier | Hunter | Detection | Escape Route | Intended Audience |
|------|--------|-----------|--------------|-------------------|
| Practice | None | N/A | Car repair | First-time players, learning the map |
| Easy | Sight only | Visual only | Main door (1 key) | Casual horror fans |
| Medium | Full AI | Sight + sound + flashlight + doors | Main door (1 key) | Standard experience |
| Hard | Full AI | Full detection | Car repair (6 items) | Experienced players |
| Nightmare | Enhanced AI | Full + extended ranges | Car repair (6 items) | Mastery challenge |

### 3-Day Run System (Death Progression)

Death is not a hard reset — it advances the day counter (Day 1 → Day 2 → Day 3). On Day 3, the next death triggers Game Over and returns to the main menu. This creates a **death budget**: players can die twice before facing elimination pressure. State persists across deaths within a run:
- Keys remain in inventory
- Unlocked doors stay open
- Used ToolReceivers stay broken
- Unscrewed screws stay removed
- Dropped items retain their positions
- Hunter position persists (except bedroom kills)

### Identified Weak Points

**1. Front-loaded exploration, back-loaded payoff**
The first 10–15 minutes involve searching dark rooms with minimal feedback. Keys and tools look similar to environmental clutter. Players reported items being "hidden too well with no visual indication." The progression feels flat until the first locked door is opened, creating a slow start that risks losing player engagement.

**2. No intermediate milestones or feedback**
Between finding an item and using it (which may be minutes apart), there is no progression signal. No map percentage, no area-clear indicator, no "you're getting closer" feedback. The only progression cue is the objective text update, which changes infrequently.

**3. Knowledge as invisible progression**
The most significant form of progression — learning the map layout, Hunter patrol routes, and item locations — is entirely invisible. A player on their 5th attempt is dramatically more capable than on their 1st, but nothing in the game acknowledges or rewards this learning. Deaths feel punishing rather than educational.

**4. Car repair objective complexity spike**
On Hard/Nightmare, players must locate 6 separate items (4 parts + drill + car key), carry them one at a time to the outdoor area, and complete a multi-step installation process. This roughly triples the required exploration time compared to the main-door escape (1 key). The jump from "find 1 key" to "find and install 6 items" lacks intermediate steps.

**5. Flashlight as a trap mechanic (Medium+)**
The flashlight is the primary tool for finding items in dark rooms, but its 12x detection multiplier makes it a death sentence on Medium and above. Players universally stop using it after initial deaths, removing a core mechanic from the game and making item discovery slower and more frustrating.

**6. Clue collection has no mechanical reward**
Clues provide narrative context but offer no gameplay advantage. There is no incentive to collect them beyond story interest, making them feel disconnected from the core survival-escape loop.

**7. Difficulty tier gaps create walls, not ramps**
Medium → Hard simultaneously changes the escape objective (1 key → 6 items) and maintains full Hunter AI. Hard → Nightmare adds a Hunter faster than the player (7.0 vs 5.5 m/s). Deaths spike 3–5x between adjacent tiers rather than scaling gradually.

---

## 2. Competitor Benchmarking

### Games Selected

| Game | Genre | Why Selected |
|------|-------|-------------|
| **Granny** (DVloper, 2017) | First-person horror escape | Direct genre match — item-chain puzzles, stalker AI, day-based death system, single-location escape |
| **Outlast** (Red Barrels, 2013) | First-person survival horror | No-combat horror, stealth/flee gameplay, battery management parallels flashlight risk/reward |
| **Amnesia: The Dark Descent** (Frictional, 2010) | First-person survival horror | Sanity/light management, area-gated progression, puzzle-based advancement |

### Progression Breakdown

#### Granny

| Element | Implementation |
|---------|---------------|
| **Structure** | 5-day death budget (vs. The Order's 3-day). Each death = next day. Day 5 death = game over. |
| **Item progression** | ~10 items needed to escape, with multiple escape routes (front door, car, helicopter in sequels). Items are color-coded and distinct from environment. |
| **Area gating** | Keys unlock rooms; some items unlock shortcuts (cutting pliers → fan vent). Map is compact — most items reachable within 2 minutes. |
| **Difficulty scaling** | Easy (enemy slower, extra day), Normal, Hard (faster enemy, fewer hiding spots), Extreme (no hiding spots, fastest enemy). Each tier adjusts one axis at a time. |
| **Feedback** | Items visually snap into escape mechanisms (door locks, car parts). Player sees tangible progress toward escape with each placement. |
| **Death penalty** | Minimal — wake up in same room, items stay where placed/dropped. Only cost is day advancement. |

**Key takeaway:** Granny's progression is transparent — items are visually distinct, escape mechanisms show partial completion, and each item placed is a visible step toward freedom. The Order's items blend into the environment with no visual distinction.

#### Outlast

| Element | Implementation |
|---------|---------------|
| **Structure** | Linear level progression through an asylum. No death counter — checkpoint-based respawn. |
| **Item progression** | Batteries (consumable, manage camera night-vision), documents (lore), key items (valves, fuses) gate specific doors. |
| **Area gating** | Locked doors require finding a specific item or completing an objective (turn on generators, find 2 valves). Always one clear next-objective. |
| **Difficulty scaling** | Normal, Hard, Nightmare (no checkpoints), Insane (one life, permadeath). Difficulty affects enemy damage and resource scarcity, not objective complexity. |
| **Feedback** | Camera battery meter provides constant resource tension. Objective list updates frequently. Environmental storytelling (blood trails, bodies) signals danger progression. |
| **Risk/reward tool** | Night-vision camera drains batteries but is essential for navigation — directly parallels The Order's flashlight. Key difference: battery scarcity creates gradual tension rather than binary on/off risk. |

**Key takeaway:** Outlast's battery system creates graduated risk (batteries deplete over time, creating increasing tension) whereas The Order's flashlight is binary (on = detected, off = safe). Outlast also keeps objectives clear with frequent updates and a single critical path.

#### Amnesia: The Dark Descent

| Element | Implementation |
|---------|---------------|
| **Structure** | Semi-linear hub areas with locked wings. Progression unlocks new areas of a castle. |
| **Item progression** | Tinderboxes (light sources, limited), oil (lantern fuel), puzzle items (chemicals, machine parts). Dual resource management. |
| **Area gating** | Puzzle-based: combine items, operate machinery, find hidden passages. Multi-step puzzles with intermediate feedback (machine partially assembled). |
| **Difficulty scaling** | No traditional difficulty settings. Difficulty emerges from resource management — using tinderboxes/oil freely makes later areas darker and more dangerous. |
| **Feedback** | Sanity system provides constant feedback: darkness and monster proximity degrade sanity (screen distortion, hallucinations). Staying in light restores it. Creates a tension loop where players must balance safety (light) with resource conservation. |
| **Risk/reward tool** | Lantern consumes oil but maintains sanity and visibility. Similar to The Order's flashlight but with a consumable resource that forces strategic rationing rather than binary avoidance. |

**Key takeaway:** Amnesia's sanity system gives constant invisible-to-visible feedback about player state. Progression feels meaningful because each safe room reached, each puzzle solved, and each resource cache found provides tangible relief. The Order lacks an equivalent feedback mechanism — the player's "state" (safe vs. in danger) is only communicated by whether the Hunter is visible.

### Comparative Analysis

| Feature | Granny | Outlast | Amnesia | The Order |
|---------|--------|---------|---------|-----------|
| Item visibility | High (color-coded, distinct) | Medium (contextual glow) | Medium (interactive highlight) | Low (no visual distinction) |
| Progression feedback | Visual (items snap into place) | Textual (objective updates) | Systemic (sanity meter) | Minimal (occasional objective text) |
| Risk/reward tool | Stun gun (limited uses) | Camera (battery drain) | Lantern (oil drain) | Flashlight (binary detection risk) |
| Death penalty | Low (wake up, items persist) | Low (checkpoint respawn) | Low (checkpoint respawn) | Medium (day advances, 3 deaths = game over) |
| Difficulty scaling | Single-axis (AI speed) | Single-axis (resources/checkpoints) | Emergent (resource scarcity) | Multi-axis (AI + objectives + speed) |
| Escape clarity | High (visible locks/mechanisms) | High (one clear path) | Medium (puzzle-based) | Low on Hard+ (6 scattered items) |

### Takeaways for The Order

1. **Add item visibility cues** — All three competitors make interactive objects visually distinguishable. A subtle glow, particle effect, or distinct coloring on pickupable items would address the #1 playtester complaint without breaking immersion.

2. **Graduate the flashlight risk** — Outlast and Amnesia both use consumable-resource tools that create tension through scarcity rather than instant punishment. Reducing the flashlight multiplier (from 12x to 4x, as proposed in Activity 4) would shift it from "never use" to "use carefully," matching competitor design patterns.

3. **Show partial escape progress** — Granny visually shows items placed on the escape mechanism. For car-repair mode, showing installed parts on Body_Goblin (already implemented via CarInstallZone) could be reinforced with a HUD counter ("Parts installed: 2/4, Wheels drilled: 1/3").

4. **Single-axis difficulty scaling** — All three competitors scale one dimension per tier. The Order's Medium → Hard jump changes both AI capability and objective complexity simultaneously, creating a compounding spike. Decoupling these (as proposed in Activity 4) would align with industry best practices.

5. **Reduce death penalty variance** — Granny gives 5 days; Outlast/Amnesia use checkpoints. The Order's 3-day system is punishing by comparison, especially on Hard/Nightmare where each run takes 30–60+ minutes. The persistence system (keys, doors, items) helps, but a 4th day or mid-run checkpoint could smooth the experience.

---

## 3. Progression Playtesting

### Methodology

We conducted structured playtest sessions across all 5 difficulty levels with 3 team members (5+ hours total). Each player completed full runs on Easy through Hard; Nightmare was attempted by all 3, completed by 1. Players rated progression aspects on a 1–10 scale and provided qualitative feedback at defined checkpoints.

### Playtest Checkpoints

| Checkpoint | Description | Progression Stage |
|------------|-------------|-------------------|
| CP1 | First 5 minutes — initial exploration | Early (no items found) |
| CP2 | First key/tool found | Early-mid (first unlock) |
| CP3 | First locked area opened | Mid (area expansion) |
| CP4 | First death | Mid (death system engagement) |
| CP5 | 50% of escape items collected | Late-mid (approaching endgame) |
| CP6 | Escape attempt | Endgame |

### Quantitative Ratings (1–10 scale, averaged across 3 testers)

| Aspect | Easy | Medium | Hard | Nightmare |
|--------|------|--------|------|-----------|
| Pacing of unlocks | 7 | 6 | 4 | 3 |
| Balance of challenge & reward | 8 | 7 | 5 | 3 |
| Power/capability growth feeling | 5 | 5 | 4 | 2 |
| Clarity of next objective | 7 | 6 | 4 | 4 |
| Satisfaction of item discovery | 6 | 5 | 5 | 4 |
| Overall progression satisfaction | 7 | 6 | 4 | 3 |

### Quantitative Metrics (from Activity 4 playtesting data)

| Metric | Easy | Medium | Hard | Nightmare |
|--------|------|--------|------|-----------|
| Avg. deaths per run | 1–2 | 3–4 | 10–15 | 20+ |
| Avg. completion time | 6–10 min | 20–30 min | 20–40 min | ~1 hr |
| Completion rate (team) | 3/3 | 3/3 | 3/3 | 1/3 |
| Flashlight usage (est.) | High | Low | Minimal | Almost never |

### Qualitative Feedback

**What moments felt most rewarding?**
- Opening the first locked door after finding a key ("felt like real progress")
- Finding items hidden inside furniture drawers (discovery satisfaction)
- Surviving a Hunter chase through corner-juking ("adrenaline rush, felt earned when it worked")
- Reaching the outdoor area for the first time (environment change = progress signal)
- Completing the car repair and hearing the engine start (strong endgame payoff)

**When did players feel stuck or frustrated?**
- First 5–10 minutes of every run: "wandering in the dark with no idea where to go"
- Items blending into the environment — "couldn't tell what was pickupable vs. decoration." Players suggested adding a subtle glow to pickupable objects
- After finding a key but not knowing which door it opens — no directional hint
- On Hard/Nightmare, carrying car parts one at a time across the entire map while dodging the Hunter felt tedious on repeat deaths
- Flashlight usage on Medium+ was described as "a death wish" — players unanimously stopped using it after initial deaths, even though dark rooms made item discovery frustrating

**What part of progression was least satisfying?**
- "Power growth" scored lowest across all difficulties (2–5/10). Players noted they never feel "stronger" — the player's capabilities are identical from minute 1 to minute 60. Progression is purely spatial (access to new areas) with no mechanical growth.
- Clue collection felt disconnected: "cool lore but doesn't help me escape"
- The jump from Medium to Hard was described as "hitting a wall" — switching from finding 1 key to finding 6+ items while the Hunter is fully active
- Knowledge progression (learning the map/Hunter patterns through death) was acknowledged as real but invisible: "I got better but the game doesn't know that"

**Observations on difficulty tier progression:**
- 2/3 testers described Nightmare's Hunter (faster than player sprint) as "unfair" rather than "challenging"
- The 3-day death budget felt appropriate on Easy/Medium but punishing on Hard/Nightmare where runs require 30–60+ minutes
- Rare cases where the Hunter failed to detect the player in obvious open spaces were observed but deemed infrequent enough to not impact overall balance

### Identified Pain Points (Priority Ranked)

| Priority | Issue | Affected Tiers | Impact |
|----------|-------|---------------|--------|
| **HIGH** | Items have no visual distinction from environment | All | Players waste time examining non-interactive objects; frustration in dark rooms |
| **HIGH** | Flashlight unusable on Medium+ (12x multiplier) | Medium, Hard, Nightmare | Core mechanic eliminated; dark room navigation becomes guesswork |
| **HIGH** | Medium → Hard difficulty wall (AI + objective spike) | Hard, Nightmare | 3–5x death increase between tiers; players feel unprepared |
| **MEDIUM** | No intermediate progression feedback | All | Long stretches with no reward signal; pacing feels flat |
| **MEDIUM** | No mechanical power growth | All | Player capability is static; progression is purely spatial |
| **MEDIUM** | Car part carry-one-at-a-time tedium | Hard, Nightmare | Repetitive traversal amplified by Hunter deaths |
| **LOW** | Clue collection has no gameplay reward | All | Optional content feels disconnected from core loop |
| **LOW** | Knowledge progression is invisible | All | Skill growth is real but unacknowledged by the game |

### Recommended Adjustments

**Immediate (parameter changes, no code):**
1. Reduce flashlight detection multiplier from 12x to 4x (SO parameter tweak)
2. Increase stamina regen during chase: 10 → 15/s, delay 1.5 → 0.75s (SO parameter tweak)

**Short-term (minor code/scene changes):**
3. Add subtle emissive glow or particle effect to pickupable items — increases item visibility without breaking horror atmosphere
4. Add a HUD progress indicator for car-repair mode ("Parts: 2/4 | Drilled: 1/3")
5. Update objective text more frequently with directional hints ("A locked door on the east wing..." rather than just "Find a way out")

**Medium-term (design changes):**
6. Decouple objective complexity from AI difficulty — let Hard use main-door escape with full Hunter; offer car-repair as a separate modifier
7. Consider a 4th day on Hard/Nightmare to reduce per-run pressure given longer completion times
8. Introduce minor mechanical progression (e.g., sprint duration slightly increases after each day survived, or finding a clue reveals nearby item locations on the HUD briefly)
