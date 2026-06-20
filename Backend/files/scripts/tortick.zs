craftingTable.remove(<item:cyclic:inventory_cake>);
craftingTable.addShaped("inventory_cake",<item:cyclic:inventory_cake>, [
	[<item:minecraft:enchanted_golden_apple>, <item:minecraft:emerald_block>, <item:minecraft:poisonous_potato>], 
	[<item:minecraft:ender_chest>, <item:minecraft:cake>, <item:minecraft:sculk_sensor>], 
	[<item:minecraft:trident>.withTag({Damage: 0 as int}), <item:minecraft:netherite_block>, <item:cyclic:gem_obsidian>]
]);