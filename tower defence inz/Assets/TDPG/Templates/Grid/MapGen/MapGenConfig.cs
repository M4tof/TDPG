using UnityEngine;

namespace TDPG.Templates.Grid.MapGen
{
    /// <summary>
    /// Map Generation data for serialization
    /// </summary>
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
        public int MinimalDistance = 3; // A minimal distance between the spawners and the base
        public bool AssumeCanSwim = false;
        public int EmptyCellsAroundPoints = 2;
    }
}