# SHATTERHEART

> *A high-velocity, parkour-based FPS where movement is a weapon and every card you discard could save your life.*

**Genre:** Fast-paced First-Person Shooter with Parkour & Card-based Abilities  
**Engine:** Unity  

---

## Gameplay

The core loop is: **move fast → collect cards → eliminate enemies → reach the fragment → beat your time.**

- Speedrunning-friendly level design with S/A/B/C grade rankings
- Card pickups grant temporary weapons — discarding them triggers powerful movement abilities
- Explosive barrels and balloon platforms reward creative traversal
- Levels can't be completed until all enemies are eliminated

---

## Controls

| Action | Input |
|---|---|
| Move | WASD |
| Look | Mouse |
| Jump | Space |
| Dash | Shift |
| Updraft | — |
| Stomp | — |
| Shoot | Left Click |
| Discard Card / Use Ability | Right Click |
| Swap Cards | Q |

---

## Cards

| Card | Type | Weapon |
|---|---|---|
| Heartbreaker | Default | Katana |
| Reaper's Kiss | Precision | Pistol |
| Widow Maker | Rapid Fire | SMG |
| Damnation | Burst | Shotgun |
| Winter's Bite | — | — |

---

## Key Scripts

| Script | Purpose |
|---|---|
| `FirstPersonController.cs` | CharacterController-based FPS movement, coyote time, jump buffer |
| `PlayerAbilities.cs` | Dash, Updraft, Stomp |
| `Shooting.cs` | Raycast weapon system |
| `CardManager.cs` | Card inventory, switching, discard abilities |
| `LevelTimer.cs` | Run timer and grade evaluation |
| `MusicManager.cs` | Playlist management with scene-aware persistence |

---

## Assets Used

- [RealtimeCSG](https://assetstore.unity.com/packages/tools/modeling/realtime-csg-69542) — Level building
- [Footsteps Essentials](https://assetstore.unity.com/packages/audio/sound-fx/foley/footstepsessentials-189879)
- [Hit Effects FREE](https://assetstore.unity.com/packages/vfx/particles/hit-effects-free-284613)
- [Slash Effects FREE](https://assetstore.unity.com/packages/vfx/particles/spells/slash-effects-free295209)
- [Fantasy Skybox FREE](https://assetstore.unity.com/packages/2d/texturesmaterials/sky/fantasy-skybox-free-18353)
- ADG Texture Pack · Stylized Water
