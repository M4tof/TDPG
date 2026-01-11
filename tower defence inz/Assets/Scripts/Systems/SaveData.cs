using TDPG.Generators.Seed;
using System.Collections.Generic;
using UnityEngine;
using TDPG.Templates.Grid;
using TDPG.Templates.Enemies;
using TDPG.Templates.Turret;

[System.Serializable]
public struct Vec3
{
    public float x, y, z;
    
    public Vec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    
    // Magic to convert automatically between Unity Vector3 and Vec3
    public static implicit operator Vec3(Vector3 v) => new Vec3(v.x, v.y, v.z);
    public static implicit operator Vector3(Vec3 v) => new Vector3(v.x, v.y, v.z);

    public static implicit operator Vec3(Vector2 v) => new Vec3(v.x, v.y, 0);
    public static implicit operator Vector2(Vec3 v) => new Vector2(v.x, v.y);
}


// Data collected from the ResourceSystem
[System.Serializable]
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

[System.Serializable]
public class ElementSaveData
{
    
}

[System.Serializable]
public class TurretSaveData
{
    public string TurretID;     // "ArrowTurret"
    public int GridX;
    public int GridY;
    // Add Cooldown/Rotation if you want to save exact frame state
    public List<CardData> Upgrades; 
}


[System.Serializable]
public class EnemySaveData
{
    public string EnemyID;      // "Goblin"
    public float Health;        // 50.0f
    public int Damage;
    public float AttackSpeed;
    // public float MaxHealth;
    public Vec3 Position;    // (10.5, 2.0)
    // public int PathIndex;       // Optimization: Where on the path they are
    // Add overrides if needed
    public EnemyStatsOverride Ov;
}
[System.Serializable]
public class GridSaveData
{
    public int Width;
    public int Height;
    public float CellSize;
    public int[,] Grid;
    public TDPG.Templates.Grid.Grid.TileType[,] TypeGrid;
    // public int[,] BuildingGrid;
    public int DestX;
    public int DestY;

    public System.Collections.Generic.List<Vec3> SpawnerPositions = new System.Collections.Generic.List<Vec3>();
}

[System.Serializable]
public class WaveSaveData
{
    public int CurrentWaveNumber;
    public float CooldownTimer;
    public bool IsWaveActive;
    public bool IsSpawning;
    
    // Critical: If saved mid-spawn, we need to remember who hasn't spawned yet
    public Queue<string> RemainingEnemyQueue = new Queue<string>(); 
}

// The master data class that holds all saved data
[System.Serializable]
public class GameSaveData
{
    public float SaveVersion = 0.3f; // Good practice for backwards compatibility
    // public int SlotNumber;
    public System.DateTime SavedTime = System.DateTime.Now;

    public GlobalSeed GS;
    
    public ResourceSaveData Resources;
    public ElementSaveData Elements; // legacy, kept for compatibility

    public string SerializedRegistry;
    public Vec3 PlayerPosition;
    public List<TurretSaveData> Turrets;
    public List<EnemySaveData> Enemies;
    public GridSaveData GData;
    public int CardNextId;
    public WaveSaveData WaveState;
}