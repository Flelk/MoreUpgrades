# MoreUpgrades

More (and better-targeted) upgrades in the R.E.P.O. shop. BepInEx 5 mod, host-side.
Player docs: [thunderstore/README.md](thunderstore/README.md).

## How it works
`src/ShopPatches.cs`:
- Postfix on `ShopManager.GetAllItemsFromStatsManager`: weighted shuffle of `potentialItemUpgrades` so favored upgrades end up near the end (the shop picks from the end).
- Prefix on `PunManager.ShopPopulateItemVolumes`: raises `itemUpgradesAmount` (3 → 4 by default). If the shop has fewer upgrade slots than that, it turns spare small-item slots into upgrade slots.

## Build
`.\build.ps1` → `dist\MoreUpgrades-<version>.zip`. `build.cmd` also installs it into your r2modman profile.
