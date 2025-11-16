using TDPG.Generators.Seed;

// Data collected from the ResourceSystem
public class ResourceSaveData
{
    public float MoneyValue;
    public float MoneyMax;
    public bool MoneyDebt;
    public float MoneyRegen;
    public float ManaValue;
    public float ManaMax;
    public bool ManaDebt;
    public float ManaRegen; 
}


public class ElementSaveData
{
    
}

public class TurretSaveData
{
    public int Id;
    public Vector2 TileSize;
    public float Multiplyer;
    public string SpriteName;
}

public class GridSaveData
{
    public int Width;
    public int Height;
    public float CellSize;
    public int[,] Grid;
    public Grid.TileType[,] TypeGrid;
    public int[,] BuildingGrid;
}

// The master data class that holds all saved data
public class GameSaveData
{
    public float SaveVersion = 0.1f; // Good practice for backwards compatibility
    // public int SlotNumber;
    public System.DateTime SavedTime = System.DateTime.Now;

    public GlobalSeed GS;
    
    public ResourceSaveData Resources;
    public ElementSaveData Elements;
    public List<TurretSaveData> Turrets;
    public GridSaveData GData;
}