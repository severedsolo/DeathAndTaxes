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

### Social Credit Adjustments
- Doesn't always tell you social credit has been removed. I'm telling the game to, but it doesn't always show. Actual social credit amounts will be correct.
- The Perks UI will show you still have that perk even if it's been removed. The buff will be removed correctly and it will update next time you level up.
