# LP28
This mod attempts to bring various glitches that were patched over the years back into 1.5. Any module can be disabled in the `Options -> Mods -> LP28` menu in-game. Requires Vasi as a dependency.

Please DM/ping `pseudorandom#9039` with feedback, bug reports, or feature requests.

### Supported glitches:
- **BadFloat**: Allows bad float
- **DisableOOBRespawnPlane**: Disables the out-of-bounds respawn plane below each level
- **DiveInvuln**: Allows Desolate Dive and Descending Dark to be interrupted and the hazard-only damage mode to be kept through the next room
- **DoorDreamSyncLoad**: Makes doors and dream entries synchronous loads, which remove all duped rooms
- **ExtendSpellInterruptState**: Allows the "interrupted spell" state of float and dive invuln to be kept after turning around on the ground
- **FlukeDupe**: Allows enemies to be killed by multiple flukes from one cast with Flukenest. Each kill counts for geo and arena waves
- **InfiniteQuickMapStorage**: Allows quick map storage to be performed unlimited times per quitout
- **[WIP] LaggyMap**: Attempts to mimic the lagginess of quick map and inventory prior to 1.3
- **LenientCanAction**: Allows quick map, inventory, and interaction to be performed under more circumstances
- **LoadExtender**: Configurable load extender. Extension time can be changed in the menu `Options -> Mods -> LP28 -> LoadExtender`
- **NoDreamNailFSMCancel**: Prevents dream nail use from cancelling other FSMs, notably crystal dash
- **NoExtraBossScenes**: Prevents boss scenes from introducing additional rooms in room dupes
- **RocketJump**: Allows rocket jumps
  - **Known bug**: Implementation will allow infinite height rocket jumps even if the knight does not end up grounded
- **ThornsStorage**: Allows the damaging hitbox of Thorns of Agony to be stored
- **TransitionMultiLoad**: Allows transitions to be hit multiple times, resulting in duped rooms
- **Underwater**: Allows the knight to sink under water
- **WallclingStorage**: Enables pre-1.3 style wallcling storage effects, such as keeping dash momentum
  - **Known bug**: WCS may be maintained after sliding down off a wall, especially if you hit the wall with crystal dash

### Compatibility
Use with QoL is highly recommended. However, you may want to disable the "FasterLoads" module in the settings JSON.