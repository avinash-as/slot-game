# Lucky Slots - Unity Slot Game

A playable slot machine game built with Unity (C#) plus an HTML5 WebGL build.

## Game Overview
- 5 reels x 3 rows slot machine.
- Win when all slots show the same symbol (Jackpot) or 3+ matching symbols on the middle payline.
- Symbols: Cherry, Lemon, Orange, Plum, Bell, Bar, Seven, Wild (⭐ substitutes), Scatter (💎).
- 3+ Scatters trigger 10 Free Spins with a 2x multiplier.
- RNG-based outcomes using weighted symbol probabilities.

## Project Structure
```
Assets/
  Scripts/   (Core, Symbols, UI, Audio, Bonus, Utilities)
  Prefabs/
  Animations/
  UI/
  Sounds/
  Scenes/
Build/WebGL/  (playable web build)
```

## Instructions to Run WebGL Build
1. Open `Build/WebGL/index.html` in any modern browser, or
2. Serve the folder: `python -m http.server 8000` inside `Build/WebGL` then visit http://localhost:8000
3. Click SPIN. Adjust bet with Bet +/-.

## Bonus Features
- Wild symbol substitutes for any symbol.
- Scatter triggers 10 free spins at 2x multiplier.
- Jackpot when all 15 visible cells match.

## Thought Process / Approach
- Separated concerns: RNGService (testable), PayTable (pure logic), Reel (animation), SlotMachine (orchestration), AudioManager, BonusFeature.
- Reels use a 3-phase animation (accelerate/cruise/decelerate) with predetermined RNG targets for fairness.
- Data-driven symbols via ScriptableObjects for easy balancing.
- Incremental git commits.

## Unity Notes
Open the project in Unity 2022 LTS+. Assign symbol pool, prefabs and UI references in the inspector on the SlotMachine GameObject.