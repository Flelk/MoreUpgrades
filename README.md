# MoreUpgrades

More (and better-targeted) upgrades in the R.E.P.O. shop. BepInEx 5 mod. The shop roll is host-side; the taller shelf is built on every client.
Player docs: [thunderstore/README.md](thunderstore/README.md).

## How it works
`src/ShopPatches.cs`:
- Postfix on `ShopManager.GetAllItemsFromStatsManager`: weighted shuffle of `potentialItemUpgrades` so favored upgrades end up near the end (the shop picks from the end).
- Prefix on `PunManager.ShopPopulateItemVolumes`: rolls `itemUpgradesAmount` (10 to 25 by default; the game's shop uses 10) and adds a random bonus to the regular item, power crystal and health pack targets. If the shop has fewer upgrade slots than that, it turns spare small-item slots into upgrade slots.

`src/ShelfPatches.cs`:
- Prefix on `ShopManager.ShopInitialize` (runs on every client): stacks copies of the upgrade stand's shelf mesh and collision boxes on top of it. On the host, it also copies the stand's upgrade slots upward before the shop collects them. The stand's reroll box is stretched to cover the new rows.

`src/ShelfLayout.cs`:
- Reorders the upgrades and stand slots right before the shop fills them, and replaces `UpgradeStand.SpawnNewUpgrades` so rerolls stay in color order. Colors are a name table taken from the upgrade textures.

`src/DiscountPatches.cs`: replaces the shopkeeper's verdict roll and keeps its attempt counter below the limit until the third discount.

`src/PricePatches.cs`: zeroes the shop's price increases and collapses each item's price range to its midpoint.

`src/StrengthHotkeys.cs`: F8 and F9 send the game's `TesterUpgradeCommandRPC`, which only the host can use.

## Build
`.\build.ps1` → `dist\MoreUpgrades-<version>.zip`. `build.cmd` also installs it into your r2modman profile.
