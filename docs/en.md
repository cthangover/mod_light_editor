# Light Editor — static lights of a scene, placed on a night preview

Night scenes of `time_base` are lit by the `light_set` line of a scenario. The Light Editor turns that line into points you drag on the scene's background: the preview is rendered with the game's own night shader, so what you see is how the scene will look after dark.

## Opening it

Hammer button on the in-game toolbar → **Light Editor**.

## Placing lights

![The Light Editor with home_kitchen_cooking.scenario: two lights in the list, the kitchen background in the night preview with a lamp light and its radius circle, the selected light's X, Y, Radius 500, Strength 1.0 and a yellow colour.](assets/light-editor.jpg)

1. Choose the scenario whose `light_set` line you edit (● marks scenarios that already set lights). Its scene's background is shown; **Preview background** picks another.
2. Double-click the background (or **+ Light**) to add a light, drag lights to move them, Del deletes the selected one. At most 10 static lights.
3. **Selected light**: **X**/**Y** (0–1 of the screen), **Radius** (pixels of the 1920 px game screen), **Strength** (0–1) and **Colour**.
4. Ctrl+S writes the `light_set` line into the scenario; a scenario without one gets it inserted after its header. **Copy line** puts the command on the clipboard instead, to paste it by hand.

## Limits

- Folder mods only; packed scenarios cannot be written (the line can still be copied).
- Lights follow the scenario: another scenario of the same scene may set different ones.

## Dependencies

`tools_base`, `time_base`, `ui_base`, `scenario_tools_base` (reads scenarios; the Scenario Editor window is not needed).
The older `editor_tools` package installs this mod together with the other authoring windows.
