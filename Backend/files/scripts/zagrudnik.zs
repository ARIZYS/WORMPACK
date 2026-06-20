craftingTable.remove(<item:cyclic:crystal_chestplate>);
craftingTable.addShaped("crystal_chestplate", <item:cyclic:crystal_chestplate>, [
    [<item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>],
    [<item:cyclic:gem_obsidian>, <item:minecraft:netherite_chestplate>.withTag({Damage: 0 as int}), <item:cyclic:gem_obsidian>],
    [<item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>, <item:cyclic:gem_obsidian>]
]);
