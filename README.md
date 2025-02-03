# FinFix
Risk of Rain 2 mod. Attempts to fix Breaching Fin broken damage multiplier and its interaction with Deathmark.

![](https://raw.githubusercontent.com/Judgy53/FinFix/refs/heads/main/fix_showcase.png)

## Configuration
Every configuration entry is editable in-game via the [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) mod.

| Name                    | Type    | Default    | Description                                                                                                                                                                                                                                                                                                                                                                                                                    |
|-------------------------|---------|------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Fix Double Dip          | Boolean | True       | Breaching Fin's damage multiplier is applied multiple times during proc chains.<br><br>Enabling this setting ensures it's only applied once.                                                                                                                                                                                                                                                                                   |
| Fix Death Mark          | Boolean | True       | Breaching Fin applies 2 debuffs to keep track of knock ups, both of which counts for Death Mark.<br><br>Enabling this setting ensure only one of those 2 debuffs counts for Death Mark.                                                                                                                                                                                                                                        |
| Delay Juggle Stacks     | Boolean | False      | Breaching Fin's juggle stacks are applied before calculating the damage multiplier. This causes the very first attack to also have its damage multiplied, despite the enemy being on the ground.<br><br>Enabling this setting changes that behaviour and calculate the mutiplier based on stacks before the hit.<br>Warning: With this enabled, an extra attack is required to get the full multiplier after the last knockup. |

## Credits

- Nebby's mod [ConfigurableDeathMark](https://thunderstore.io/package/Nebby/ConfigurableDeathMark/) for the Deathmark filter implementation.
