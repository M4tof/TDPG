using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TDPG.Generators.Seed;
using TDPG.Templates.Grid;
using TDPG.Templates.Grid.MapGen;
using UnityEngine;
using UnityEngine.Tilemaps;
using Grid = TDPG.Templates.Grid.Grid;
using static TDPG.Generators.Scalars.InitializerFromDate;

namespace Tests.TemplateTests
{
    [TestFixture, Category("MapGenTests")]
    public class MapGenTests
    {
        private GlobalSeed globalSeed;
        private GameObject testGO;
        
        private const int SampleCount = 100;
        private const int DistanceTestSampleCount = 100;
        private const int SpawnerCount = 5;
        private const int MapW = 40;
        private const int MapH = 40;
        
        private static readonly Dictionary<MapTypes, MapGenCutoffs> CutoffConfig =
            new Dictionary<MapTypes, MapGenCutoffs>
            {
                { MapTypes.Smooth,       new MapGenCutoffs(water: -0.09f, wall: 0.09f) },
                { MapTypes.Mountainous,  new MapGenCutoffs(water: -0.2f, wall: 0.3f) },
                { MapTypes.Chaotic,        new MapGenCutoffs(water: -0.15f,  wall: 0.1f) }
            };
        
        [SetUp]
        public void Setup()
        {
            globalSeed = new GlobalSeed(QuickGenerate(1));

            // Create temporary GameObject for MapGenerator
            testGO = new GameObject("MapGenerator_TestGO");
        }

        [TearDown]
        public void TearDown()
        {
            if (testGO != null)
            {
                GameObject.DestroyImmediate(testGO);
            }
        }
        
        private void SetPrivate(object obj, string fieldName, object value)
        {
            obj.GetType()
                .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(obj, value);
        }
        
        private GridManager CreateGridManagerWithMap(MapTypes type)
        {
            // Root GM
            GameObject gmGO = new GameObject("GridManager_Test");
            GridManager gm = gmGO.AddComponent<GridManager>();

            // MapGenerator
            MapGenerator generator = gmGO.AddComponent<MapGenerator>();
            SetPrivate(generator, "mapType", type);
            SetPrivate(generator, "width", MapW);
            SetPrivate(generator, "height", MapH);
            SetPrivate(generator, "numOfEnemySpawners", SpawnerCount);

            // Tilemap setup
            GameObject rootGridGO = new GameObject("UnityGrid");
            var unityGrid = rootGridGO.AddComponent<UnityEngine.Grid>();

            GameObject tilemapGO = new GameObject("Tilemap");
            tilemapGO.transform.SetParent(rootGridGO.transform);
            var tilemap = tilemapGO.AddComponent<Tilemap>();
            tilemapGO.AddComponent<TilemapRenderer>();

            // Assign to GM
            SetPrivate(gm, "mapGenerator", generator);
            SetPrivate(gm, "tilemap", tilemap);
            SetPrivate(gm, "gridComponent", unityGrid);

            // Camera
            GameObject camGO = new GameObject("Camera");
            var cam = camGO.AddComponent<Camera>();
            SetPrivate(gm, "mainCamera", cam);

            // Fake tiles
            SetPrivate(gm, "emptyTile", ScriptableObject.CreateInstance<Tile>());
            SetPrivate(gm, "wallTile", ScriptableObject.CreateInstance<Tile>());
            SetPrivate(gm, "waterTile", ScriptableObject.CreateInstance<Tile>());

            // === MANUALLY TRIGGER UNITY LIFECYCLE ===
            CallUnityMethod(gm, "Awake");
            CallUnityMethod(gm, "Start");

            return gm;
        }
        
        private static void CallUnityMethod(object instance, string methodName)
        {
            var method = instance.GetType().GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
            );

            Assert.IsNotNull(method, $"Method not found: {methodName}");
            method.Invoke(instance, null);
        }
        
        private (float water, float wall, float land) RunMapTest(MapTypes type)
        {
            Seed seed = globalSeed.NextSubSeed("MapGenTests");

            // Add MapGenerator component to the temp GameObject
            MapGenerator generator = testGO.AddComponent<MapGenerator>();
            var cutoffs = CutoffConfig[type];
            
            SetPrivate(generator, "mapType", type);
            SetPrivate(generator, "width", 100);
            SetPrivate(generator, "height", 100);
            SetPrivate(generator, "waterLevel", cutoffs.WaterLevel);
            SetPrivate(generator, "wallLevel",  cutoffs.WallLevel);
            
            Grid.TileType[,] mapData = generator.GenerateMap(seed);
            
            int waterCount = 0, wallCount = 0, landCount = 0;
            
            int w = mapData.GetLength(0);
            int h = mapData.GetLength(1);
            
            for (int y = 0; y < h; y++)
            {
                string row = "";
                for (int x = 0; x < w; x++)
                {
                    Grid.TileType tile = mapData[x, y];
                    switch (tile)
                    {
                        case Grid.TileType.WATER: row += "|"; waterCount++; break;
                        case Grid.TileType.WALL:  row += "^"; wallCount++; break;
                        case Grid.TileType.EMPTY: row += "_"; landCount++; break;
                    }
                }
                // Debug.Log(row);
            }
            int totalTiles = waterCount + wallCount + landCount;
            float actualWater = (float)waterCount / totalTiles * 100f;
            float actualWall  = (float)wallCount / totalTiles * 100f;
            float actualLand  = (float)landCount / totalTiles * 100f;

            Debug.Log($"Actual: Water={actualWater:F2}%, Wall={actualWall:F2}%, Land={actualLand:F2}%");

            return (actualWater, actualWall, actualLand);
        }
        
        private (float water, float wall, float land) RunMultipleMapTests(MapTypes type, int samples = SampleCount)
        {
            float totalWater = 0f, totalWall = 0f, totalLand = 0f;

            for (int i = 0; i < samples; i++)
            {
                var (water, wall, land) = RunMapTest(type);
                totalWater += water;
                totalWall  += wall;
                totalLand  += land;

                // Reset the component between runs to avoid state bleed
                DestroyImmediateComponents();
            }

            return (totalWater / samples, totalWall / samples, totalLand / samples);
        }
        
        private void DestroyImmediateComponents()
        {
            foreach (var comp in testGO.GetComponents<MapGenerator>())
                GameObject.DestroyImmediate(comp);
        }
        
        [Test]
        public void MapGen_SmoothGivesBalancedMap()
        {
            var (water, wall, land) = RunMultipleMapTests(MapTypes.Smooth);
            
            Debug.Log("---------------");
            Debug.Log("Water% = "+water);
            Debug.Log("Wall% = "+wall);
            Debug.Log("Empty% = "+land);
            
            Assert.That(water, Is.GreaterThan(5f).And.LessThan(60f));
            Assert.That(wall,  Is.GreaterThan(5f).And.LessThan(60f));
            Assert.That(land,  Is.GreaterThan(10f).And.LessThan(75f));
        }

        [Test]
        public void MapGen_MountainousGivesMoreWalls()
        {
            var (water, wall, land) = RunMultipleMapTests(MapTypes.Mountainous);
            Debug.Log("---------------");
            Debug.Log("Water% = "+water);
            Debug.Log("Wall% = "+wall);
            Debug.Log("Empty% = "+land);
            
            Assert.Greater(wall, 20f, "Mountainous should produce more wall tiles.");
        }

        [Test]
        public void MapGen_ChaoticLakesGivesMoreWater()
        {
            var (water, wall, land) = RunMultipleMapTests(MapTypes.Chaotic);
            Debug.Log("---------------");
            Debug.Log("Water% = "+water);
            Debug.Log("Wall% = "+wall);
            Debug.Log("Empty% = "+land);
            
            Assert.Greater(water, 25f, "Lakes should produce more water.");
        }
        
        private float RunDistanceTest(MapTypes type)
        {
            float threshold = MapW / 3f;
            float totalMin = 0f;

            for (int i = 0; i < DistanceTestSampleCount; i++)
            {
                GridManager gm = CreateGridManagerWithMap(type);

                MapGenerator gen = (MapGenerator)gm
                    .GetType()
                    .GetField("mapGenerator", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(gm);

                Vector3Int dest = gen.GetDestinationPosition();
                Vector3Int[] spawners = gen.SelectSpawnerPositions(SpawnerCount);

                float minDist = float.MaxValue;
                foreach (var sp in spawners)
                    minDist = Mathf.Min(minDist, Vector3Int.Distance(dest, sp));

                Assert.Greater(
                    minDist, threshold,
                    $"{type}: closest spawner is too close (dist={minDist:F1} < {threshold:F1})"
                );

                totalMin += minDist;

                GameObject.DestroyImmediate(gm.gameObject);
            }

            return totalMin / DistanceTestSampleCount;
        }
        
        [Test]
        public void Smooth_Spawners_Far_From_Destination()
        {
            float avg = RunDistanceTest(MapTypes.Smooth);
            Debug.Log("---------------");
            Debug.Log($"Average Smooth min distance = {avg:F2}");
        }

        [Test]
        public void Mountainous_Spawners_Far_From_Destination()
        {
            float avg = RunDistanceTest(MapTypes.Mountainous);
            Debug.Log("---------------");
            Debug.Log($"Average Mountainous min distance = {avg:F2}");
        }

        [Test]
        public void Lakes_Spawners_Far_From_Destination()
        {
            float avg = RunDistanceTest(MapTypes.Chaotic);
            Debug.Log("---------------");
            Debug.Log($"Average Lakes min distance = {avg:F2}");
        }
        
    }
    
    internal class MapGenCutoffs
    {
        public float WaterLevel;
        public float WallLevel;

        public MapGenCutoffs(float water, float wall)
        {
            WaterLevel = water;
            WallLevel  = wall;
        }
    }
    
    
    
}

