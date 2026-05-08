# Triple Match City

Triple-match clone for Android. Tap items, send them to a deck, match 3 to clear.

Unity 6000.0.62f1, URP 2D Renderer.

---

## Third-Party Libraries

1. **VContainer** — DI for Unity. Used for project, main menu, and gameplay scopes.
2. **UniTask** — Allocation-free async/await. Replaces coroutines.
3. **PrimeTween** — Tween engine. Drives the end-game popup star bounce. Gameplay item movement uses a state-driven lerp instead.
4. **NaughtyAttributes** — Inspector helpers (`[ReadOnly]`, `[AllowNesting]`, custom validators).

---

## Art Assets

1. **Kenney UI Pack** — Open-source UI sprites (CC0) | [see more](<https://kenney.nl/assets/ui-pack>)
2. **Craftpix Summer Downtown Pack** — Backgrounds and item sprites.  | [see more](<https://craftpix.net/freebies/free-simple-summer-top-down-vector-tileset>)

---

## Architecture

Three VContainer scopes:

- **RootLifetimeScope** — `IDataManager`, `ILevelManager`, `ISceneFlowService`, `IEventBus`. Lives across scenes.
- **MainMenuLifetimeScope** — `MainMenuView` + presenter.
- **GameSceneLifetimeScope** — board, deck, goals, timer, state machine, UI screens.

Cross-system communication is event-based via a DI-injected `IEventBus`. UI screens are managed by `IUIService` with stack semantics (`IScreen<TArgs>`).

`CollectibleItemView` runs a state machine (`Idle | MovingToSlot | Clearing`) and lerps to its target each frame, so concurrent commands don't race.

---

## Camera

- One-finger drag → pan (clamped to background bounds)
- Two-finger pinch → zoom
- Tap (≤ 20 px movement, ≤ 0.4 s) → pick item

Editor: mouse drag pans, scroll wheel zooms.

---

## Level Editor

**TripleMatch → Level → Level Builder**.

Place a `LevelRoot` in the scene:

```
LevelRoot
├── Background (SpriteRenderer)
├── CollectibleItems
└── NonCollectibleItems
```

- **Build / Update** writes scene state into a `LevelDataSO` asset.
- **Load Into Level Root** reads an existing asset back into the scene.

Naming: `Level_01`, `Level_02`, ... under `Assets/TripleMatchCity/Data/Levels/`.

Rule: an item type cannot appear in both `CollectibleItems` and `NonCollectibleItems`.

---

## Getting Started

```bash
git clone https://github.com/KursatSahin/mg-triple-match-city-project.git
```

1. Unity Hub → Add → project folder → Unity 6000.0.62f1
2. Open `Assets/TripleMatchCity/Scenes/Bootstrap.unity`
3. Press Play

Android build: IL2CPP, ARM64, min API 26

---

## APK

Download: [Releases](<release-link>)
