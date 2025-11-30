using NUnit.Framework;
using TDPG.Generators.Seed;
using TDPG.Templates.Grid.MapGen;
using UnityEngine;
using Grid = TDPG.Templates.Grid.Grid;
using static TDPG.Generators.Scalars.InitializerFromDate;

namespace Tests.TemplateTests
{
    [TestFixture, Category("MapGenTests")]
    public class MapGenTests
    {
        private GlobalSeed globalSeed;
        private GameObject testGO;

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
                .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(obj, value);
        }

        private (float water, float wall, float land) RunMapTest(MapTypes type, string label)
        {
            Seed seed = globalSeed.NextSubSeed("MapGenTests");

            // Add MapGenerator component to the temp GameObject
            MapGenerator generator = testGO.AddComponent<MapGenerator>();
            SetPrivate(generator, "mapType", type);
            SetPrivate(generator, "width", 100);
            SetPrivate(generator, "height", 100);
            SetPrivate(generator, "waterLevel", -0.5f);
            SetPrivate(generator, "wallLevel", 0.5f);

            Grid.TileType[,] mapData = generator.GenerateMap(seed);
            
            int waterCount = 0, wallCount = 0, landCount = 0;

            for (int y = 0; y < mapData.GetLength(1); y++)
            {
                string row = "";
                for (int x = 0; x < mapData.GetLength(0); x++)
                {
                    Grid.TileType tile = mapData[x, y];
                    switch (tile)
                    {
                        case Grid.TileType.WATER: row += "|"; waterCount++; break;
                        case Grid.TileType.WALL:  row += "^"; wallCount++; break;
                        case Grid.TileType.EMPTY: row += "_"; landCount++; break;
                    }
                }
                Debug.Log(row);
            }

            int totalTiles = waterCount + wallCount + landCount;
            float actualWater = (float)waterCount / totalTiles * 100f;
            float actualWall  = (float)wallCount / totalTiles * 100f;
            float actualLand  = (float)landCount / totalTiles * 100f;

            Debug.Log($"{label} Actual: Water={actualWater:F2}%, Wall={actualWall:F2}%, Land={actualLand:F2}%");

            return (actualWater, actualWall, actualLand);
        }

        [Test]
        public void MapGen_SmoothGivesBalancedMap()
        {
            var (water, wall, land) = RunMapTest(MapTypes.Smooth, "Smooth");

            // Smooth → low freq, few octaves → balanced terrain mixes
            Assert.That(water, Is.GreaterThan(10f).And.LessThan(60f));
            Assert.That(wall,  Is.GreaterThan(5f).And.LessThan(60f));
            Assert.That(land,  Is.GreaterThan(20f).And.LessThan(75f));
        }

        [Test]
        public void MapGen_MountainousGivesMoreWalls()
        {
            var (water, wall, land) = RunMapTest(MapTypes.Mountainous, "Mountainous");

            // Mountainous → Ridged fractal, high octaves, high walls
            Assert.Greater(wall, 25f, "Mountainous should produce more wall tiles.");
        }

        [Test]
        public void MapGen_LakesGivesMoreWater()
        {
            var (water, wall, land) = RunMapTest(MapTypes.Lakes, "Lakes");

            // Lakes → higher water frequency, strong ping-pong fractal
            Assert.Greater(water, 30f, "Lakes should produce more water.");
        }
        
        
    }
}
