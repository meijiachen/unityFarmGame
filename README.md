# unityFarmGame

## Scene view cell coordinate overlay

The editor extension in `Assets/Editor/SceneCellCoordinateOverlay.cs` shows the current Grid cell under the mouse cursor in the Scene view.

- Select a `Grid`, `Tilemap`, or any child object under a `Grid` to use that Grid.
- If nothing under a Grid is selected, the extension uses the first active `Grid` in the scene.
- Toggle it with `Tools > Scene Cell Coordinates > Enabled`.

## Random tilemap generator

`Assets/Scripts/RandomTilemapGenerator.cs` can fill a Tilemap with random Tile assets while keeping different visual styles grouped into larger areas.

Usage:

1. Add `RandomTilemapGenerator` to the `Ground1` GameObject, or assign `Ground1` to `Target Tilemap`.
2. In `Tile Style Groups`, create one element per visual style from the Tile Palette, such as grass, dirt, flowers, or light grass.
3. Drag only matching Tile assets into each group's `Tiles` array. Do not mix unrelated styles in the same group.
4. Use `Weight` to control how often that style appears. For example, grass `3` and dirt `1` makes grass more common.
5. Adjust `Style Noise Scale` to control patch size. Smaller values create larger continuous areas; larger values create more broken-up areas.
6. Keep `Min Cell` as `(-40, -40)` and `Max Cell` as `(40, 40)` to fill x=-40..40 and y=-40..40.
7. Click `Generate Random Map` in the component inspector.
8. Click `Clear Generated Area` if you need to clear that same range.
