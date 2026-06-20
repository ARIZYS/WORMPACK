craftingTable.remove(<item:cyclic:crystal_helmet>);
craftingTable.addShaped("cyclic_helmet", <item:cyclic:crystal_helmet>, [
	[<item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>], 
	[<item:cyclic:gem_obsidian>, <item:minecraft:netherite_helmet>.withTag({Damage: 0 as int}), <item:cyclic:gem_obsidian>], 
	[<item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>]
]);