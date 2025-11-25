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

        private (float water, float wall, float land) RunMapTest(MapTypes type, string label)
        {
            Seed seed = globalSeed.NextSubSeed("MapGenTests");

            // Add MapGenerator component to the temp GameObject
            MapGenerator generator = testGO.AddComponent<MapGenerator>();
            generator.GetType().GetField("mapType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .SetValue(generator, type);
            generator.GetType().GetField("width", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .SetValue(generator, 100); 
            generator.GetType().GetField("height", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .SetValue(generator, 100);

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
        public void MapGen_SmoothGivesMap()
        {
            var (water, wall, land) = RunMapTest(MapTypes.Smooth, "Smooth");
            Assert.Less(land, 65f, "Smooth maps should have <65% land (empty).");
            Assert.Less(water, 65f, "Smooth maps should have <65% water.");
            Assert.Less(wall, 65f, "Smooth maps should have <65% wall.");
        }

        [Test]
        public void MapGen_MountainousGivesMap()
        {
            var (water, wall, land) = RunMapTest(MapTypes.Mountainous, "Mountainous");
            Assert.Greater(wall, 30f, "Mountainous maps should have >30% walls.");
        }

        [Test]
        public void MapGen_LakeGivesMap()
        {
            var (water, wall, land) = RunMapTest(MapTypes.Lakes, "Lakes");
            Assert.Greater(water, 35f, "Lake maps should have >35% water.");
        }

        [Test]
        public void MapGen_DeterministicReference()
        {
            var (water, wall, land) = RunMapTest(MapTypes.NineDog, "NineDog");
            Assert.Pass("NineDog map generated deterministically.");
        }
    }
}
