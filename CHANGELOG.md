# Changelog

### v1.0.10 - Quick start hotfix
* Fixed quick start menu number keys allowing you to specify an ascent that you haven't unlocked yet.

### v1.0.9 - More fixes and improvements
* Fixed a few miscellaneous exceptions that were happening due to code being out of sync with game updates.
* Fixed a bug where `SkipAirportUsesAscent` was not always working when specified.
* Replaced `SkipAirportLobby` with `AllowAirportLobbySkip` (defaults to true) that shows quick start buttons on the main menu instead (in addition to the regular start buttons).
	* `SkipAirportUsesAscent` is still usable, however you may also press the number keys while on the main menu to change it. The backquote (`) next to 1 sets the ascent to -1 (tenderfoot).
* Improved the code behind `FixAirportRope` to lessen error spam on lobby reloads.
* Improved `AirportElevatorDoorsAlwaysAnimate` to use an RPC for the fastest response time, but it only works if the hosting player uses this mod.
* Updated reference DLLs to ensure latest vanilla compatibility.

### v1.0.8 - More options and improvements
* Added `AirportElevatorSpawnBehavior` as a host-only option (defaults to `UseAllInOrder`) that makes all players make use of all elevators in the airport lobby when spawning in. Options:
	* `Vanilla` - Unchanged gameplay, uses a single elevator.
	* `UseAllInOrder` - Players spawn in the elevators in sequential order based on their join order
	* `UseAllRandomly`- Players spawn in a random elevator
* Added `AirportElevatorDoorsAlwaysAnimate` as an option (defaults to true) that will make the elevator that future joining players spawn into animate like they do when the room loads. Animations are local. 
* Added `ConsumableItemsGetLighter` as an option (defaults to false) that makes items having multiple uses weigh less each time they are used (scout cookies, ropes, sunscreen...)
* If left blank, `SkipAirportUsesAscent` will now recalculate each time the main menu loads, allowing the most recently unlocked ascent to always be used.
* Updated reference DLLs to ensure latest vanilla compatibility.

### v1.0.7 - Fixes and improvements
* Fixed `EmoteLoopMode` never detecting a stop condition and continuously looping emotes (especially obvious with clients).
* Fixed compatibility with [PEAKER](https://thunderstore.io/c/peak/p/lammas123/PEAKER/) where GI would call an RPC in a strange way and cause error log spam.
* Added `KilnCampfireIsSafeZone` as an option (defaults to true). Similar to `DisableFogTimer`, it will prevent the lava rising timer from starting until at least one person goes > 30m from the kiln campfire.
* Added `FixDeadPlayersPreventingFog` as an option (defaults to true) that fixes a vanilla bug where dead players would prevent the fog from being triggered with height checks.
* Fixed certain instanced meshes drawing as black objects when spectating players (mostly the grass at the peak).

### v1.0.6 - Infinite animations option
* Added `EmoteLoopMode` as an option (defaults to `NetworkedLooping`), that will make every emote except ragdoll loop until you move. Options:
	* `None` - Unchanged gameplay, emotes will not loop
	* `NetworkedLooping` - Emotes will loop, even on unmodded clients, but about half of the animations will jerk a bit at the beginning of each loop (on the clients only).
	* `LocalLoopingOnly` - Emotes will loop, but other players won't see the looping.
* Added `FixAirportRope` as an option (defaults to true) to control toggling the fix from v1.0.4.

### v1.0.5 - Hotfix and new feature
* Fixed `DisableFogTimer` not working at all when enabled.
* Added `HotSunCooksShieldingItems` as an option (defaults to `None`) to make the mesa sun slowly cook items *only when the player is holding them up to shade themselves.* Options:
	* `None` - Unchanged gameplay
	* `OnlyFood` - The hot sun will only cook food items that are used as shields
	* `AllCookables` - The hot sun will cook anything that can be cooked that is used as a shield

### v1.0.4 - Minor game bugfixes
* Fixed a bug where manually culled items (like the flowers on the peak) did not take spectated players into account.
* Items that fully cooked in lava will now be automatically destroyed (by a modded host only) to prevent infinite cooking and log spam.
* Fixed the airport climbing wall rope only appearing the first time you start a new game (subsequent airport loads would result in no rope). Modded host only.

### v1.0.3 - Hotfix and new option
* Tweaked the auto dismount behavior to more accurately dismount when on certain sides of vines.
* Added `SpawnMissingPropsOnLateJoins` as a host-only option (defaults to true) that will automatically spawn missing props like marshmallows when a player joins in the middle of a game.

### v1.0.2 - Bugfixes and improvements
* Fixed the `DisableFogTimer` setting showing up as `FogSetting` in the config.
* Fixed a bug where errors would spam the log after returning to airport if `CampfiresPreventHunger` was set to true.
* Added `RopeVineChainBehavior` as an option (defaults to `AllowClimbing`) to change the behavior of ropes/vines/chains. Options:
	* `Vanilla` - Unchanged gameplay
	* `AllowClimbing` - Allows player to climb on to another surface without first jumping
	* `AutoDismount`- Will also automatically get off and attempt to start climbing at the end
* Added `SkipAirportLobby` as an option (defaults to false). If set to true, starting any game will launch directly into the island of the day, bypassing the airport lobby entirely.
* Added `SkipAirportUsesAscent` as an option (defaults to empty). If specified as a number, AND using `SkipAirportLobby`, the game will use your specified ascent. Must be between -1 and your max unlocked ascent.
* Added `BringPassportToIsland` as an option (defaults to false). If set to true, you will wake up on the island holding your passport. Useful when skipping the airport.
	* The passport will now also be droppable and throwable, but to prevent other unmodded clients from picking it up and getting stuck with it, they will immediately be forced to drop it when picking it up.
* Moved `SkipPretitleScreen` from GUI to Menu section in the config.

### v1.0.1 - More fog and campfire options
* Added an option (defaults to false) to skip the pre-title screen.
* Added an option (defaults to true) to play the fog rising sound effect on every level, not just the shore.
* Added an option (defaults to true) to prevent the fog from having a wait timer, and only activating from height checks.
* Added an option (defaults to true) to prevent the player from getting hungry while near campfires.

### v1.0.0 - Initial Release
* Items can no longer fall through terrain when placed.