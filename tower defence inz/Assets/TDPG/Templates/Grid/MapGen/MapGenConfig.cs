using UnityEngine;
using TDPG.Templates.Grid.MapGen;

[System.Serializable]
public class MapGenConfig
{
    [Header("Basic")]

    public MapTypes MapType = MapTypes.Mountainous;
    public int Width = 50;
    public int Height = 50;
    public int SpawnerCount = 3;

    [Header("Advanced Generation")]
    public float WaterLevel = -0.356f;
    public float WallLevel = 0.4f;
    public int MinimalDistance = 3; // Between spawners/base
    public bool AssumeCanSwim = false;
    public int EmptyCellsAroundPoints = 2;
}