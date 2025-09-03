# Changelog

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