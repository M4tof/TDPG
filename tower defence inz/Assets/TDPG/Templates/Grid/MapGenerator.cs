using System;
using UnityEngine;
using TDPG.Generators.FastNoiseLite;
using TDPG.Generators.Seed;
using static TDPG.Templates.Grid.Grid;

namespace TDPG.Templates.Grid
{
    public class MapGenerator
    {
        [Header("Spawn Chances (0–1)")] 
        [Range(0f, 1f)] public float normalWeight = 0.75f;
        [Range(0f, 1f)] public float wallWeight = 0.15f;
        [Range(0f, 1f)] public float waterWeight = 0.10f;
        
        private int width;
        private int height;

        public MapGenerator(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        
        public int[,] initMapGen(Seed seed)
        {
            int[,] mapInit = new int[width, height];
            var noise = new FastNoiseLite();
            ulong seedVal = seed.GetBaseValue();
            byte[] byteVal = BitConverter.GetBytes(seedVal);
            
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetSeed((int)seedVal);
            
            //weighted strength 0.0 - 1.0
            //set freqauency based on seed 0.009 - 0.05
            noise.SetFrequency(UnityEngine.Random.Range(0.009f, 0.05f));
            //octaves 2-5
            noise.SetFractalOctaves(UnityEngine.Random.Range(2, 6));
            //lacauarity 1-2.5
            noise.SetFractalLacunarity(UnityEngine.Random.Range(1.5f, 2.5f));
            // gain 0.3 - 0.7
            noise.SetFractalGain(UnityEngine.Random.Range(0.3f, 0.7f));
            
            // Normalize weights so they sum to 1
            float total = wallWeight + waterWeight + normalWeight;
            float wallChance = wallWeight / total;
            float waterChance = waterWeight / total;
            float normalChance = normalWeight / total;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float n = (noise.GetNoise(x, y) + 1f) * 0.5f; // normalize [-1,1] -> [0,1]

                    TileType tile;
                    if (n < waterChance)
                        tile = TileType.WATER;
                    else if (n < waterChance + wallChance)
                        tile = TileType.WALL;
                    else
                        tile = TileType.EMPTY;

                    mapInit[x, y] = (int)tile;
                }
            }
            
            return mapInit;

        }
        
        
        
        
    }
}