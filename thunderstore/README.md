# MoreUpgrades

The shop between levels stocks **more upgrades**, and favors the ones you want.

- **More upgrades**: each shop rolls 10 to 25 (configurable, up to 50). It stocks fewer if it runs out of room or available upgrades, and extra upgrades take over small-item spots.
- **More of everything else**: regular items, power crystals and health packs each get 5 to 10 more, up to what the shelves can hold.
- **Taller upgrade shelf**: the upgrade stand is about twice as tall, with 42 slots instead of 14. Rerolling changes every row.
- **Sorted by color**: upgrades fill the stand from the bottom row up, grouped by color, so the top rows are only used when a shop rolls a lot of them.
- **Favored upgrades**: one each of Grab Strength, Grab Range, Health and Energy (stamina) is stocked whenever the shop rolled it as available. Beyond that, favored upgrades are 6x as likely to be picked.
- **Shopkeeper discounts**: scanning an item passes at 100%, then 50%, then 33% as the shopkeeper gives discounts in that shop, and never after the third. Failed scans don't change the odds, and there's no limit on tries before then.
- **Set prices**: every item costs the middle of its normal price range, and prices never go up. Bigger lobbies still get the game's discount.
- **Strength hotkeys**: F8 adds 10 Grab Strength levels and F9 removes 10, for the host or solo. Changes are kept in your save.
- Vanilla rules still apply: each upgrade's per-shop limit, and unaffordable upgrades being skipped most of the time.

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
| SortUpgradesByColor | true | arrange the stand by color (host only) |
| ShopkeeperDiscountOdds | true | false = game default of 3 scans at 1 in 3 (host only) |
| SetPrices | true | false = game default of random prices that go up (host only) |
| StrengthHotkeys | true | turn the F8 and F9 keys on or off |
