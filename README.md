# Stardew-MegaStorage

Adds Large Chests and Magic Chests to Stardew Valley.

# Large Chest
Capacity: 72 items (6 rows).

Recipe:
* 100 Wood
* 5 Copper Bar
* 5 Iron Bar
 
# Magic Chest
Infinite scrollable capacity.

Recipe:
* 100 Hardwood
* 5 Gold Bar
* 5 Iridium Bar
* 50 Solar Essence
* 50 Void Essence

# Categories
From v1.2.0 you can filter chest contents by clicking on the categories to the left of the chest inventory. Using vanilla UI there was only room for 7 categories. They are so far not configurable.
* All
* Crops: Forage, Fruit, Flower, Vegetable
* Seeds: Seed, Fertilizer
* Materials: Mineral, Resource, Crafting, Monster Loot
* Cooking: Cooking, Animal Product, Artisan Goods
* Fish: Fish, Fishing Tackle, Bait
* Misc: Tool, Footware, Ring, Artifact, Furniture, Decor, Trash, (no category) 

# Configuration
Names, descriptions, IDs and recipes for Large Chest and Magic Chest are configurable in config.json. The format of recipes is pairs of item IDs and how many of that item to use. Default:
* Large Chest: 388 100 334 5 335 5
* Magic Chest: 709 100 336 5 337 5 768 50 769 50

# Compatibility
* Requires [SMAPI](https://smapi.io/).
* Supports multiplayers and controllers.
* Compatible with Chests Anywhere, Stack Everything, Automate, Carry Chest and content packs for Content Patcher and Json Assets.
* Minor incompatibility with Convenient Chests: overlapping UI.
* NOT compatible with Save Anywhere. Using these mods together may result in crashes and loss of items.

# Is this safe?
Before saving, all Large Chests and Magic Chests are converted to normal chests. After saving, they are converted back. This makes sure your items aren't lost, even if uninstalling this mod. Normal chests actually have infinite capacity, it's only when adding items one at a time they are limited to 36 capacity.

# Credits
* Inspired by [Magic Storage](https://forums.terraria.org/index.php?threads/magic-storage.56294/) for Terraria.
* Custom sprites by [Revanius](https://www.nexusmods.com/users/40079).
* Convenient Chests save fix by [MaienM](https://www.nexusmods.com/stardewvalley/users/6392240).

[Nexus page](https://www.nexusmods.com/stardewvalley/mods/4089)
