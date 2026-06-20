craftingTable.remove(<item:cyclic:crystal_boots>);
craftingTable.addShaped("crystal_boots",<item:cyclic:crystal_boots>, [
	[<item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>], 
	[<item:cyclic:gem_obsidian>, <item:minecraft:netherite_boots>.withTag({Damage: 0 as int}), <item:cyclic:gem_obsidian>], 
	[<item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>]
]);