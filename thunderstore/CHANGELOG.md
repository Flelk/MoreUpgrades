# Changelog

## 3.0.0
- The upgrade stand is now filled completely and evenly, with 2 extra Health. Turn this off with EvenUpgrades and FillUpgradeStand.

## 2.0.0
- Removed personal info from the mod's files. The settings file is now flelk.moreupgrades.cfg, so settings go back to their defaults.

## 1.2.0
- The upgrade shelf is about twice as tall, with 42 slots instead of 14 (`ExtraShelfLevels`). Everyone in the lobby needs this version to see the new shelf.
- Upgrades are arranged by color from the bottom row up, including after a reroll (`SortUpgradesByColor`).
- Shopkeeper scans pass at 100%, then 50%, then 33% as it gives discounts, and never after the third (`ShopkeeperDiscountOdds`). Failed scans don't change the odds.
- Every shop item has one set price that never goes up (`SetPrices`).
- F8 adds 10 Grab Strength levels and F9 removes 10, for the host or solo (`StrengthHotkeys`).
- Rerolling changes the upgrades on every row. The new rows have no hatch, so their upgrades swap in the open.

## 1.1.0
- Fix: 1.0.0 lowered the shop to 4 upgrades. The game's real limit is 10, not 3.
- Regular items, power crystals and health packs each get 5 to 10 more (`ExtraItemsMin`, `ExtraItemsMax`).
- Each shop rolls 10 to 25 upgrades by default. `MinUpgrades` and `MaxUpgrades` replace `ExtraUpgradesPercent`.
- One each of Grab Strength, Grab Range, Health and Energy is stocked whenever the shop rolled it as available (`GuaranteeFavored`).
- Favored upgrades are now 6x as likely, and Grab Range joins the list. The settings are now `PreferredUpgrades` and `PreferredWeight`, so old values are replaced by the new defaults.

## 1.0.0
- 33% more shop upgrade slots, weighted toward Grab Strength, Health and Energy.
