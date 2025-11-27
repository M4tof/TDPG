using TDPG.Generators.FastNoiseLite;
using TDPG.Generators.Seed;
using UnityEngine;
using static TDPG.Templates.Grid.Grid;

namespace TDPG.Templates.Grid.MapGen
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map Type")]
        [SerializeField] private MapTypes mapType = MapTypes.Smooth;
        
        [Header("Map Settings")]
        [SerializeField, Min(1)] private int width = 140;
        [SerializeField, Min(1)] private int height = 140;
        [SerializeField, Range(-1.0f,-0.001f)] private float waterLevel = -0.5f;
        [SerializeField, Range(0.001f,1.0f)] private float wallLevel = 0.5f;
        
        [Header("Points of Interest Settings")]
        [SerializeField,Range(1,3)] private int emptyCellsAroundPoints = 1;
        [SerializeField, Min(1)] private int numOfEnemySpawners = 1; //TODO:
        [SerializeField, Min(1)] private int minimalDistance = 1;

        private TileType[,] _mapInit;
        private Vector3Int _destinationPos;
        
        public TileType[,] GenerateMap(Seed seed)
        {
            _mapInit = new TileType[width, height];
            // if (seed == null)
            // {
                seed = new Seed(2401999, -1,"missingSeedInMapGen",false);
            // }
            
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
            
            //used in deciding destination point
            int d5 = int.Parse(seedStr[5].ToString());
            
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
                    break;


                case MapTypes.Mountainous:
                    // Clamp to more chaotic ranges
                    freq = Mathf.Clamp(freq, 0.02f, 0.05f);
                    octaves = Mathf.Clamp(octaves, 4, 6);
                    gain = Mathf.Clamp(gain, 0.3f, 0.5f);
                    noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
                    break;

                case MapTypes.Lakes:
                    // Clamp to higher frequency, more water
                    freq = Mathf.Clamp(freq, 0.04f, 0.05f);
                    octaves = Mathf.Clamp(octaves, 4, 5);
                    gain = Mathf.Clamp(gain, 0.6f, 0.8f);
                    noise.SetFractalType(FastNoiseLite.FractalType.PingPong);
                    break;

                case MapTypes.Static:
                    return DeterministicMapRetrieve();
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
                    if (n < waterLevel)          // bellow sea level water
                        tile = TileType.WATER;
                    else if (n > wallLevel)      // mountains
                        tile = TileType.WALL;
                    else                             // land
                        tile = TileType.EMPTY;

                    _mapInit[x, y] = tile;
                }
            }

            Vector2 dst = DecideDestination(d5);
            _destinationPos = new Vector3Int((int)dst.x, (int)dst.y, 0);
                
            CleanUpDestination((int)dst.x,(int)dst.y);
            Debug.Log($"Destination set to position {_destinationPos}");
            
            //TODO: set up N enemy spawners, distance from destination from settings, ensure at least one path from it (destruction of buildings included)
            
            return _mapInit;
        }
        
        
        public int Width => width;
        public int Height => height;
        public MapTypes Type => mapType;

        private TileType[,] DeterministicMapRetrieve()
        {
            _mapInit = new TileType[width, height];
            float[,] mapHere = DeterministicMap.noiseMatrix;

            int noiseSize = mapHere.GetLength(0);

            // 1. Build the deterministic terrain
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int nx = x % noiseSize;
                    int ny = (height - 1 - y) % noiseSize;

                    float n = mapHere[ny, nx];

                    TileType tile;
                    if (n < waterLevel)
                        tile = TileType.WATER;
                    else if (n > wallLevel)
                        tile = TileType.WALL;
                    else
                        tile = TileType.EMPTY;

                    _mapInit[x, y] = tile;
                }
            }

            // 2. Destination selection uses digit 5 from seed
            int seedBasedValue = 0; 
            Vector2 dst = DecideDestination(seedBasedValue);

            _destinationPos = new Vector3Int((int)dst.x, (int)dst.y, 0);

            // 3. Apply cleanup (clears around destination)
            CleanUpDestination((int)dst.x, (int)dst.y);

            Debug.Log($"[DETERMINISTIC] Destination set to {_destinationPos}");

            // 4. TODO: Enemy spawners (same logic as procedural)

            return _mapInit;
        }


        private void CleanUpDestination(int posX, int posY)
        {
            int x0Bound = Mathf.Clamp(posX - emptyCellsAroundPoints, 0, width);
            int x1Bound = Mathf.Clamp(posX + emptyCellsAroundPoints, 0, width);
            int y0Bound = Mathf.Clamp(posY - emptyCellsAroundPoints, 0, height);
            int y1Bound = Mathf.Clamp(posY + emptyCellsAroundPoints, 0, height);

            for (int x = x0Bound; x <= x1Bound; x++)
            {
                for (int y = y0Bound; y <= y1Bound; y++)
                {
                    _mapInit[x, y] = TileType.EMPTY;
                }
            }
        }

        private Vector2 DecideDestination(int value)
        {
            switch (value)
            {
                default:
                    return FindCentralDestination();   // 0 or 9

                case 1:
                case 2:
                    // Top-right corner 
                    return FindCornerDestination(width - 1, height - 1, -1, -1);

                case 3:
                case 4:
                    // Top-left corner 
                    return FindCornerDestination(0, height - 1, 1, -1);

                case 5:
                case 6:
                    // Bottom-left corner 
                    return FindCornerDestination(0, 0, 1, 1);

                case 7:
                case 8:
                    // Bottom-right corner, inward diagonal
                    return FindCornerDestination(width - 1, 0, -1, 1);
            }
        }
        
        private Vector2 FindCentralDestination()
        {
            int cx = width / 2;
            int cy = height / 2;

            int[][] directions = new int[][]
            {
                new[]{ 1, 0 },   // right
                new[]{ 0, 1 },   // up
                new[]{ -1,0 },   // left
                new[]{ 0,-1 }    // down
            };

            int x = cx;
            int y = cy;

            if (IsValidAndEmpty(x, y))
            {
                CleanUpDestination(x, y);
                return new Vector2(x, y);
            }

            int steps = 1;
            int dirIndex = 0;

            while (steps < Mathf.Max(width, height))
            {
                for (int i = 0; i < 2; i++)
                {
                    var d = directions[dirIndex];

                    for (int s = 0; s < steps; s++)
                    {
                        x += d[0];
                        y += d[1];

                        if (IsValidAndEmpty(x, y))
                        {
                            CleanUpDestination(x, y);
                            return new Vector2(x, y);
                        }
                    }

                    dirIndex = (dirIndex + 1) % 4;
                }

                steps++;
            }

            Debug.LogWarning("No EMPTY destination found.");
            return new Vector2(cx, cy);
        }
        
        private Vector2 FindCornerDestination(int startX, int startY, int dx, int dy)
        {
            int x = startX;
            int y = startY;

            while (x >= 0 && x < width && y >= 0 && y < height)
            {
                if (IsValidAndEmpty(x, y))
                {
                    CleanUpDestination(x, y);
                    return new Vector2(x, y);
                }

                x += dx;
                y += dy;
            }

            Debug.LogWarning("No EMPTY destination found.");
            return new Vector2(startX, startY);
        }
        
        private bool IsValidAndEmpty(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return false;

            return _mapInit[x, y] == TileType.EMPTY;
        }

        public Vector3Int GetDestinationPosition()
        {
            return this._destinationPos;
        }

        public Vector3Int GetSpawnerPosition(int spawnerId)
        {
            return new Vector3Int(0, 0, 0); //TODO:
        }
        
    }
}