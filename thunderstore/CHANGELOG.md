# Changelog

## 1.2.0
- The upgrade shelf is about twice as tall, with 42 slots instead of 14 (`ExtraShelfLevels`). Everyone in the lobby needs this version to see the new shelf.
- Rerolling only changes upgrades on the original two rows.

## 1.1.0
- Fix: 1.0.0 lowered the shop to 4 upgrades. The game's real limit is 10, not 3.
- Regular items, power crystals and health packs each get 5 to 10 more (`ExtraItemsMin`, `ExtraItemsMax`).
- Each shop rolls 10 to 25 upgrades by default. `MinUpgrades` and `MaxUpgrades` replace `ExtraUpgradesPercent`.
- One each of Grab Strength, Grab Range, Health and Energy is stocked whenever the shop rolled it as available (`GuaranteeFavored`).
- Favored upgrades are now 6x as likely, and Grab Range joins the list. The settings are now `PreferredUpgrades` and `PreferredWeight`, so old values are replaced by the new defaults.

## 1.0.0
- 33% more shop upgrade slots, weighted toward Grab Strength, Health and Energy.
