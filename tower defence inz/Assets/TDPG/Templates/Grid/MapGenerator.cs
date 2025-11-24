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
        [Range(0f, 1f)] public float wallCutof = 0.45f;
        [Range(-1f, 0f)] public float waterCutof = -0.30f;
        
        private int width;
        private int height;
        private MapTypes mapType;

        public MapGenerator(int width, int height,MapTypes type = MapTypes.Smooth)
        {
            this.width = width;
            this.height = height;
            mapType = type;
        }
        
        public int[,] initMapGen(Seed seed)
        {
            int[,] mapInit = new int[width, height];
            
            seed.IsBitBased =  false;
            ulong seedVal = seed.GetBaseValue();
            seed.NormalizeSeedValue();
            string seedStr = seedVal.ToString();
            
            // Use different digits for different parameters
            int d0 = int.Parse(seedStr[0].ToString());
            int d1 = int.Parse(seedStr[1].ToString());
            int d2 = int.Parse(seedStr[2].ToString());
            int d3 = int.Parse(seedStr[3].ToString());
            int d4 = int.Parse(seedStr[4].ToString());
            
            var noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalPingPongStrength(2.0f);
            noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
            noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance);
            noise.SetCellularJitter(1.0f);
            noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            noise.SetDomainWarpAmp(3.0f);
            noise.SetSeed((int)seedVal);
            
            // Seed-derived values
            // Frequency: map digit to [0.009, 0.05]
            float freq = 0.009f + (d0 / 9f) * (0.05f - 0.009f);
            // Octaves: 2 + digit, clamp to 6
            int octaves = Mathf.Clamp(2 + d1, 2, 6);
            // Lacunarity: [1.5, 2.5]
            float lacunarity = 1.5f + (d2 / 9f) * (2.5f - 1.5f);
            // Gain: [0.3, 0.7]
            float gain = 0.3f + (d3 / 9f) * (0.7f - 0.3f);
            // Weighted strength: [0.0, 1.0]
            float weightedStrength = d4 / 9f;
            
            Debug.Log($"Seed = {(int)seedVal}");
            Debug.Log($"Raw: ({mapType}): freq={freq}, oct={octaves}, gain={gain}");
            
            switch (mapType)
            {
                case MapTypes.Smooth:
                    // Clamp to smoother ranges
                    freq = Mathf.Clamp(freq, 0.009f, 0.02f);
                    octaves = Mathf.Clamp(octaves, 2, 3);
                    gain = Mathf.Clamp(gain, 0.5f, 0.7f);

                    // Balanced cutoffs: ~33% water, ~33% wall, ~33% land (values with close bias to 0, so 30 not 33)
                    waterCutof = -0.30f;
                    wallCutof  = 0.30f;
                    break;


                case MapTypes.Mountainous:
                    // Clamp to more chaotic ranges
                    freq = Mathf.Clamp(freq, 0.02f, 0.05f);
                    octaves = Mathf.Clamp(octaves, 4, 6);
                    gain = Mathf.Clamp(gain, 0.3f, 0.5f);
                    waterCutof = -0.35f;
                    wallCutof = 0.15f;
                    noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
                    break;

                case MapTypes.Lakes:
                    // Clamp to higher frequency, more water
                    freq = Mathf.Clamp(freq, 0.04f, 0.05f);
                    octaves = Mathf.Clamp(octaves, 4, 5);
                    gain = Mathf.Clamp(gain, 0.6f, 0.8f);
                    waterCutof = -0.05f;
                    wallCutof = 0.5f;
                    noise.SetFractalType(FastNoiseLite.FractalType.PingPong);
                    break;
            }
            
            noise.SetFractalGain(gain);
            noise.SetFractalWeightedStrength(weightedStrength);
            noise.SetFractalLacunarity(lacunarity);
            noise.SetFractalOctaves(octaves);
            noise.SetFrequency(freq);
            Debug.Log($"Clamped ({mapType}): freq={freq}, oct={octaves}, gain={gain}");
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float n = noise.GetNoise(x, y);

                    TileType tile;
                    if (n < waterCutof)          // bellow sea level water
                        tile = TileType.WATER;
                    else if (n > wallCutof)      // mountains
                        tile = TileType.WALL;
                    else                             // land
                        tile = TileType.EMPTY;

                    mapInit[x, y] = (int)tile;
                }
            }
            
            return mapInit;
        }
        
        
        
        
        
    }
}