using System;
using TDPG.Generators.FastNoiseLite;
using TDPG.Generators.Seed;
using UnityEngine;
using static TDPG.Templates.Grid.Grid;

namespace TDPG.Templates.Grid.MapGen
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField, Min(1)] private int width = 140;
        [SerializeField, Min(1)] private int height = 140;
        [SerializeField] private MapTypes mapType = MapTypes.Smooth;

        private float wallCutof;
        private float waterCutof;
        
        public TileType[,] GenerateMap(Seed seed)
        {
            TileType[,] mapInit = new TileType[width, height];
            
            if (seed == null)
            {
                seed = new Seed(2401999, -1,"missingSeedInMapGen",false);
            }
            
            seed.IsBitBased =  false;
            ulong seedVal = seed.GetBaseValue();
            seed.NormalizeSeedValue();
            string seedStr = seedVal.ToString();
            
            // Use first 5 digits of seed for noise parameters
            int d0 = int.Parse(seedStr[0].ToString());
            int d1 = int.Parse(seedStr[1].ToString());
            int d2 = int.Parse(seedStr[2].ToString());
            int d3 = int.Parse(seedStr[3].ToString());
            int d4 = int.Parse(seedStr[4].ToString());
            
            // Initialize FastNoise
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
            
            // Configure per map type
            switch (mapType)
            {
                case MapTypes.Smooth:
                    // Clamp to smoother ranges
                    freq = Mathf.Clamp(freq, 0.009f, 0.02f);
                    octaves = Mathf.Clamp(octaves, 2, 3);
                    gain = Mathf.Clamp(gain, 0.5f, 0.7f);
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
                
                case MapTypes.NineDog:
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            float n = MapReference.Values[x, y];

                            TileType tile;
                            if (n < waterCutof)
                                tile = TileType.WATER;
                            else if (n > wallCutof)
                                tile = TileType.WALL;
                            else
                                tile = TileType.EMPTY;

                            mapInit[x, y] = tile;
                        }
                    }

                    return mapInit;   // Deterministic output
            }
            
            // Apply noise settings
            noise.SetFractalGain(gain);
            noise.SetFractalWeightedStrength(weightedStrength);
            noise.SetFractalLacunarity(lacunarity);
            noise.SetFractalOctaves(octaves);
            noise.SetFrequency(freq);
            Debug.Log($"Clamped ({mapType}): freq={freq}, oct={octaves}, gain={gain}");
            
            // Generate map
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

                    mapInit[x, y] = tile;
                }
            }
            
            return mapInit;
        }
        
        
        public int Width => width;
        public int Height => height;
        public MapTypes Type => mapType;
        
    }
}