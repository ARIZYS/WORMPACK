craftingTable.remove(<item:cyclic:charm_ultimate>);
craftingTable.addShaped("charm_ultimate", <item:cyclic:charm_ultimate>, [
	[<item:minecraft:netherite_block>, <item:cyclic:charm_wither>.withTag({Damage: 0 as int, onoff: 1 as int}), <item:minecraft:netherite_block>], 
	[<item:cyclic:charm_void>.withTag({Damage: 0 as int}), <item:minecraft:nether_star>, <item:cyclic:charm_fire>.withTag({Damage: 0 as int})], 
	[<item:minecraft:netherite_block>, <item:cyclic:charm_antidote>.withTag({Damage: 0 as int}), <item:minecraft:netherite_block>]
]);