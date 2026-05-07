# unityFarmGame

## Scene view cell coordinate overlay

The editor extension in `Assets/Editor/SceneCellCoordinateOverlay.cs` shows the current Grid cell under the mouse cursor in the Scene view.

- Select a `Grid`, `Tilemap`, or any child object under a `Grid` to use that Grid.
- If nothing under a Grid is selected, the extension uses the first active `Grid` in the scene.
- Toggle it with `Tools > Scene Cell Coordinates > Enabled`.

## Random tilemap generator

`Assets/Scripts/RandomTilemapGenerator.cs` can fill a Tilemap with random terrain while keeping grass/dirt boundaries manageable.

Usage:

1. Add `RandomTilemapGenerator` to the `Ground1` GameObject, or assign `Ground1` to `Target Tilemap`.
2. Keep `Generation Mode` as `Terrain Noise` for grass/dirt maps.
3. Assign a normal grass Tile or grass RuleTile to `Grass Tile`.
4. Assign a dirt RuleTile to `Dirt Tile`. Configure that RuleTile with the dirt center, edge, and corner sprites from the Tile Palette.
5. Adjust `Dirt Amount` to control how much dirt appears.
6. Adjust `Terrain Noise Scale` to control patch size. Smaller values create larger continuous areas; larger values create more broken-up areas.
7. Keep `Min Cell` as `(-40, -40)` and `Max Cell` as `(40, 40)` to fill x=-40..40 and y=-40..40.
8. Click `Generate Random Map` in the component inspector.
9. Click `Clear Generated Area` if you need to clear that same range.

`Style Groups` mode still exists for simple variant pools, but it is not recommended for palettes where most tiles are grass/dirt boundary pieces.
