# Lucky Slots

A simple slot machine game I built with Unity and C#. You spin 5 reels, match symbols on the middle line, and try to hit the jackpot. Wilds substitute for other symbols, and 3 scatters give you 10 free spins at 2x.

## Play it

Live build: https://avinash-as.github.io/slot-game/

To run locally, open `Build/WebGL/index.html` in a browser, or serve the folder:

```
cd Build/WebGL
python -m http.server 8000
```

Then visit http://localhost:8000.

## How the game works

- 5 reels, 3 rows. The middle row is the payline.
- Match 3 or more of the same symbol on the payline to win.
- All 15 cells the same symbol = jackpot.
- 3 or more scatters (💎) trigger 10 free spins with a 2x multiplier.
- Wild (⭐) counts as any symbol.

## Project layout

```
Assets/
  Scripts/        C# scripts (core, symbols, ui, audio, bonus, utils)
  Scenes/         Main scene
  Prefabs/
  Animations/
  UI/
  Sounds/
Build/WebGL/      Playable web build
README.md
```

## Running the Unity project

Open the folder in Unity 2022.3 LTS or newer. Assign the serialized references on the SlotMachine object (reels, symbol prefab, symbol pool, UI elements), then build for WebGL into `Build/WebGL`.

## Notes on my approach

I kept the game logic separate from the animation. RNGService handles randomness, PayTable handles wins, Reel handles the spin animation, and SlotMachine ties it together. Symbols are ScriptableObjects so payouts and weights can be tweaked without code changes. Reels spin in three phases (speed up, cruise, slow down) and always land on a predetermined outcome so the RNG stays fair.

## Bonus features

- Wild symbol
- Scatter-triggered free spins with multiplier
- Jackpot for a full screen of matching symbols

Feel free to reach out if you have questions.