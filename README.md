# PeakGeneralImprovements

Everything is mostly configurable and improves (IMO) several things about the game, with more to come.

### GENERAL IMPROVEMENTS:
* Vines, ropes, and chains' behaviors may be configured to allow direct climbing while still on them, and to support automatically dismounting at the ends.
* Marshmallows (and other similar props) will correctly spawn at upcoming and active campfires when new players join in the middle of a game. (Configurable as host)
* All emotes except ragdoll can now loop inifinitely, and even unmodded clients will see it. (Configurable)
* The pre-title screen is skippable. (Configurable)
* The airport lobby is skippable as a "quick start" option when starting a new game, both online and off. (Configurable)
	* You may also specify which ascent to use when quick starting, if any. If left blank, it uses your max unlocked ascent.
* The passport may be brought along to the island. Useful when skipping the airport. (Configurable)
* The fog rising sound effect now plays on subsequent levels, not only on the shore. (Configurable)
* The fog no longer has a wait timer, and only activates from height checks. (Configurable as host)
* The kiln campfire now functions as a safe zone and will prevent the lava rising timer from counting down until at least one player leaves the campfire. (Configurable as host)
* The player no longer gets hungry while near campfires. (Configurable)
* Players entering the airport lobby may now spawn in all elevators, not only the single one. (Configurable as host)
* The airport elevator doors now open each time a player joins the lobby you are in. (Configurable)

### NEW FEATURES:
* The hot sun (Mesa) may be allowed to cook items that are being actively held up by the player to shade themselves. If so, it will continue to cook as long as the item is being used to block the sun.
* Multiple use consumable items, such as scout cookies, ropes, and antidotes, may be configured to weigh less the more that is used.

### BUGFIXES:
* Fixes being able to accidentally drop items through terrain.
* Fixes certain object's culling distance checks not working when spectating another player.
* Items that have been fully cooked in lava will now be automatically destroyed to prevent infinite cooking and log spam. (Modded host only)
* The climbing wall rope at the airport will now work every time the scene loads, instead of only the first time. (Configurable. Modded host only, but fix propogates to clients)
* Dead players will no longer contribute to height checks for triggering the fog rising. (Configurable)