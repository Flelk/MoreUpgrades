# MoreUpgrades

The shop between levels stocks **more upgrades**, and favors the ones you want.

- **More upgrades**: each shop rolls 10 to 25 (configurable, up to 50). It stocks fewer if it runs out of room or available upgrades, and extra upgrades take over small-item spots.
- **More of everything else**: regular items, power crystals and health packs each get 5 to 10 more, up to what the shelves can hold.
- **Taller upgrade shelf**: the upgrade stand is about twice as tall, with 42 slots instead of 14. New slots fill at random, so the top rows can be hard to reach, and rerolling only changes the original two rows.
- **Favored upgrades**: one each of Grab Strength, Grab Range, Health and Energy (stamina) is stocked whenever the shop rolled it as available. Beyond that, favored upgrades are 6x as likely to be picked.
- Vanilla rules still apply: each upgrade's per-shop limit, the price going up 50% per purchase, and unaffordable upgrades being skipped most of the time.

## Who needs it
**The host** (or you, solo) rolls the shop, so the host has to install it. **Everyone else** should install it too: without it, upgrades on the new rows look like they're floating. To play with people who don't have it, the host can set `ExtraShelfLevels` to 0.

## Config (`gabe.moreupgrades.cfg`, or r2modman's Config editor)
| Setting | Default | |
| --- | --- | --- |
| MinUpgrades | 10 | lowest number a shop can roll (1 to 50) |
| MaxUpgrades | 25 | highest number a shop can roll (1 to 50) |
| ExtraItemsMin | 5 | lowest bonus for other items (0 and 0 = game default) |
| ExtraItemsMax | 10 | highest bonus for other items |
| GuaranteeFavored | true | stock one of each favored upgrade when available |
| PreferredUpgrades | `Grab Strength, Grab Range, Health, Energy` | comma-separated; empty = no bias |
| PreferredWeight | 6 | 1 = vanilla odds |
| ExtraShelfLevels | 2 | levels added to the upgrade stand, each with 14 slots (0 to 2; 0 = game default) |
