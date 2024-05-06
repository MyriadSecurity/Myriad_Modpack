# Yaml Configurations
Use these files to add custom prefabs to control seasonally.
Add the prefab names as strings into the brackets of the season you wish to NOT allow.
#### 
Example: 
```yml
season_spring: ['CottonWoodSapling_RtD','OakSapling_RtD']
season_summer: []
season_fall: []
season_winter:
- BlossomSapling_RtD
- ThinPineSapling_RtD
```
#### 
You will notice that the YML accepts two formats for adding to the list.
Direct string inputs into the brackets or line inputs.
#### 
CustomValues.yml will automatically generate if file is missing once game is loaded.
Once generated, you can open file and tweak the values.
If you ever add new custom pickables, simply delete the file, and let regenerate, then tweak again.
#### Notes
Have fun - Rusty
