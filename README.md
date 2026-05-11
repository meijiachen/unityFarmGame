# unityFarmGame

## Scene view cell coordinate overlay

The editor extension in `Assets/Editor/SceneCellCoordinateOverlay.cs` shows the current Grid cell under the mouse cursor in the Scene view.

- Select a `Grid`, `Tilemap`, or any child object under a `Grid` to use that Grid.
- If nothing under a Grid is selected, the extension uses the first active `Grid` in the scene.
- Toggle it with `Tools > Scene Cell Coordinates > Enabled`.

## Random tilemap generator

`Assets/Scripts/RandomTilemapGenerator.cs` generates a shared grass/dirt terrain mask and paints it into one or more Tilemap layers.

Usage:

1. Add `RandomTilemapGenerator` to the parent `Tilemap Grid` object.
2. Configure `Terrain Layers`. Each layer targets one Tilemap node and can use its own grass tile, dirt RuleTile, additional dirt RuleTiles, and paint chance.
3. For base ground layers, use `Paint On = Grass And Dirt` and `Paint Chance = 1`.
4. For decoration layers such as flowers, use `Paint On = Grass Only` and a low `Paint Chance`, such as `0.05` to `0.15`.
5. Adjust `Dirt Amount` to control how much dirt appears.
6. Adjust `Terrain Noise Scale` to control patch size. Smaller values create larger continuous areas; larger values create more broken-up areas.
7. Use `Terrain Smoothing Iterations` to remove noisy one-cell fragments and fill small holes.
8. Keep `Min Cell` as `(-40, -40)` and `Max Cell` as `(40, 40)` to fill x=-40..40 and y=-40..40.
9. Keep `Force Dirt Border Width` at `1` if the playable map edge should not contain grass strips.
10. If Dirt RuleTiles touching the generated rectangle edge show grass fringes, set `Rule Tile Neighbor Padding` to `1`.
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
  - `Paint On`: `Grass Only`
  - `Paint Chance`: `0.05` to `0.15`
  - `Grass Tile`: flower or decoration Tile/RuleTile
  - `Dirt Tile`: empty unless you also want decorations on dirt

Decoration RuleTile note:

- RuleTile `Output = Random` randomizes which sprite is used after a rule matches.
- It does not control whether a cell should contain decoration.
- Use layer `Paint Chance` for sparse placement, otherwise every matching grass/dirt cell can receive a decoration and look too regular.

Boundary RuleTile note:

- RuleTiles treat empty cells outside the generated area as `Not This`.
- If the generated mask makes the area border grass, you will see grass strips at the playable edge.
- `Force Dirt Border Width = 1` changes the outer generated cells to dirt.
- If a dirt region touches `Min Cell` or `Max Cell`, the RuleTile may still choose a grass-edge sprite because outside cells are empty.
- Set `Rule Tile Neighbor Padding` to `1` to paint one extra neighbor ring outside the generated area so boundary dirt tiles match as `This`.
- That padding ring is real Tilemap data, so make sure it is outside the visible/playable bounds or clear it with `Clear Generated Area` if you change your map size.

Recommended starting values for natural grass/dirt terrain:

- `Dirt Amount`: `0.25` to `0.4`
- `Terrain Noise Scale`: `0.015` to `0.03`
- `Terrain Smoothing Iterations`: `3`
- `Dirt Survival Neighbors`: `3`
- `Dirt Birth Neighbors`: `5`
