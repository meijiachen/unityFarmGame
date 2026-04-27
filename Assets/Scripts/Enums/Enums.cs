public enum InventoryLocation
{
    player,
    chest,
    count,
}

public enum ToolEffect
{
    none,
    watering
}

public enum PlayerState
{
    idle,
    walking,
    running,
    carrying,
    useTool,
    liftTool,
    pick,
    swing,
}

public enum Direction
{
    up,
    down,
    left,
    right,
    none,
}

public enum ItemType
{
    Seed,
    Commodity,
    Watering_tool,
    Hoeing_tool,
    Chopping_tool,
    Breaking_tool,
    Reaping_tool,
    Collecting_tool,
    Reapable_scenary,
    Furniture,
    none,
    count
}

public static class Tag
{
    public const string Player = "Player";
}