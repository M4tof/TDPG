using NUnit.Framework;
using TDPG.Generators.Seed;
using TDPG.Templates.Grid;
using UnityEngine;
using Grid = TDPG.Templates.Grid.Grid;

namespace Tests.TemplateTests
{
    [TestFixture, Category("MapGenTests")]
    public class MapGenTests
    {

        [Test]
        public void MapGen_GivesMap()
        {
            Seed seed1 = new Seed(12345,-1,"MapGentest");
            MapGenerator mg1 = new MapGenerator(40,40);
            mg1.normalWeight = 0.60f;
            mg1.waterWeight = 0.20f;
            mg1.wallWeight = 0.10f;
            int[,] mapData = mg1.initMapGen(seed1);

            for (int y = 0; y < mapData.GetLength(1); y++)
            {
                string row = "";
                for (int x = 0; x < mapData.GetLength(0); x++)
                {
                    Grid.TileType tile = (Grid.TileType)mapData[x, y];
                    switch (tile)
                    {
                        case Grid.TileType.WATER: row += "|"; break;
                        case Grid.TileType.WALL:  row += "#"; break;
                        case Grid.TileType.EMPTY: row += "_"; break;
                    }
                }
                Debug.Log(row);
            }
            
        }
    }
}