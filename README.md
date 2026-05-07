# unityFarmGame

## Scene view cell coordinate overlay

The editor extension in `Assets/Editor/SceneCellCoordinateOverlay.cs` shows the current Grid cell under the mouse cursor in the Scene view.

- Select a `Grid`, `Tilemap`, or any child object under a `Grid` to use that Grid.
- If nothing under a Grid is selected, the extension uses the first active `Grid` in the scene.
- Toggle it with `Tools > Scene Cell Coordinates > Enabled`.

## Random tilemap generator

`Assets/Scripts/RandomTilemapGenerator.cs` can fill a Tilemap with random Tile assets.

Usage:

1. Add `RandomTilemapGenerator` to the `Ground1` GameObject, or assign `Ground1` to `Target Tilemap`.
2. Drag the Tile assets from the Tile Palette into `Ground Tiles`.
3. Keep `Min Cell` as `(-40, -40)` and `Max Cell` as `(40, 40)` to fill x=-40..40 and y=-40..40.
4. Click `Generate Random Map` in the component inspector.
5. Click `Clear Generated Area` if you need to clear that same range.
