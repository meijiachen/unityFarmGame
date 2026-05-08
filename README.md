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
5. If you have more Dirt RuleTiles, put them in `Additional Dirt Tiles`. The generator chooses one Dirt RuleTile for the whole generated map, because choosing different RuleTiles per dirt cell breaks RuleTile neighbor matching.
6. If different Tilemap nodes need different RuleTiles, configure `Terrain Layers`. Each layer has its own `Target Tilemap`, `Grass Tile`, `Dirt Tile`, and `Additional Dirt Tiles`, but all layers share the same generated grass/dirt mask so their boundaries line up.
7. Adjust `Dirt Amount` to control how much dirt appears.
8. Adjust `Terrain Noise Scale` to control patch size. Smaller values create larger continuous areas; larger values create more broken-up areas.
9. Use `Terrain Smoothing Iterations` to remove noisy one-cell fragments and fill small holes.
10. Keep `Min Cell` as `(-40, -40)` and `Max Cell` as `(40, 40)` to fill x=-40..40 and y=-40..40.
11. Click `Generate Random Map` in the component inspector.
12. Click `Clear Generated Area` if you need to clear that same range.

Example `Terrain Layers` setup:

- Layer 0
  - `Name`: `Ground1`
  - `Target Tilemap`: `Ground1`
  - `Grass Tile`: base grass Tile
  - `Dirt Tile`: `DirtRuleTile`
  - `Additional Dirt Tiles`: `DirtRuleTile2`, `DirtRuleTile3`
- Layer 1
  - `Name`: `Ground2`
  - `Target Tilemap`: `Ground2`
  - `Grass Tile`: empty if this layer should only draw dirt/detail
  - `Dirt Tile`: a Ground2-specific RuleTile
  - `Additional Dirt Tiles`: other Ground2-specific RuleTiles
- Layer 2
  - `Name`: `GroundDecoration1`
  - `Target Tilemap`: `GroundDecoration1`
  - `Grass Tile`: empty if this layer should only draw dirt/detail
  - `Dirt Tile`: a decoration/detail RuleTile

Recommended starting values for natural grass/dirt terrain:

- `Dirt Amount`: `0.25` to `0.4`
- `Terrain Noise Scale`: `0.015` to `0.03`
- `Terrain Smoothing Iterations`: `3`
- `Dirt Survival Neighbors`: `3`
- `Dirt Birth Neighbors`: `5`

`Style Groups` mode still exists for simple variant pools, but it is not recommended for palettes where most tiles are grass/dirt boundary pieces.
