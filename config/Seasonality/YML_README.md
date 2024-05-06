# Seasonality YAML configurations
Use these files to create a more in-depth seasonal experience

## DISCLAIMER
Formatting the configurations this way allows for greater control, but also greater chance of creating errors if you do not set values with expected inputs
Use this file as reference to find correct syntax

## RESISTANCES
```yml
- VeryWeak
- Weak
- Normal
- Resistant
- VeryResistant
- Immune
- Ignore
```
Any resistances set as 'Normal' will be ignored

## ENVIRONMENTS
```yml
- None
- Clear
- Twilight_Clear
- Misty
- Darklands_dark
- HeathClear
- DeepForestMist
- GDKing
- Rain
- LightRain
- ThunderStorm
- Eikthyr
- GoblinKing
- nofogts
- SwampRain
- Bonemass
- Snow
- Twilight_Snow
- Twilight_SnowStorm
- SnowStorm
- Moder
- Ashrain
- Crypt
- SunkenCrypt
- Caves
- Mistlands_clear
- Mistlands_rain
- Mistlands_thunder
- InfectedMine
- Queen
- WarmSnow
- ClearWarmSnow
- NightFrost
- WinterClear
```
- Any brackets ( [ ] ) you see in the yml file can use different types of syntax
- Either add strings within brackets
- example:
```yml
 meadowWeather: ['WarmSnow','Snow','Clear']
```
- Or replace with a list
- example:
```yml
meadowWeather:
- WarmSnow
- Snow
- Clear
```
