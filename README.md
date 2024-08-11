# Death and Taxes

This is an attempt to fix the current gameplay loop of "Solve one case and you never have to worry about money again" while also preserving the lore that retiring to The Fields requires alot of money.

(Completely modular, all features can be enabled/disabled in the config file the mod creates at runtime)

## Features
- Money from jobs is tied to your social credit level. The higher your level the less "tax" gets applied
- Social Credit is lost if you die and are fined
- Fines are persistent (ie you leave the building the fines are still due)
- If you don't complete all objectives you won't get the full social credit reward (ie complete a murder but only get 3 questions right, you only get 300 credit not 500).
- Cases can be "failed" even if you get them right. If you don't complete all objectives, there's a chance that the client will be unhappy/you will not secure a conviction and lose social credit.
- "Land Value Tax" - pay a tax on your owned apartments every day or they will be repossessed (this is basically ApartmentRent but rather than locking you out, the apartment will just be lost).

## Known Issues
### Persistent Fines
- UI is a bit screwy. There's a bug in the base game where fines aren't persisted when loading a save, so what you see on screen will only be "fines since you loaded this save". Actual fines will be deducted and communicated to you when you die.
- Likewise the UI isn't really setup to persist fines between leaving buildings, so you'll see multiple "fine boxes". If it gets annoying just reload your save and they will disappear (due to above bug).
- I couldn't figure out a good way to get the fine amounts directly so we cheat and "simulate" taking the fines and then give you the money back. This means if you don't actually have enough money to pay, your penalty will be based on money in hand instead.

### Social Credit Adjustments
- Doesn't always tell you social credit has been removed. I'm telling the game to, but it doesn't always show. Actual social credit amounts will be correct.
- The Perks UI will show you still have that perk even if it's been removed. The buff will be removed correctly and it will update next time you level up.
