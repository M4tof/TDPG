using System.Collections.Generic;
using TDPG.Generators.FastNoiseLite;
using TDPG.Generators.Seed;
using UnityEngine;
using TDPG.Templates.Pathfinding;
using static TDPG.Templates.Grid.Grid;

namespace TDPG.Templates.Grid.MapGen
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map Type")] [SerializeField] private MapTypes mapType = MapTypes.Smooth;

        [Header("Map Settings")] [SerializeField, Min(Minsize)]
        private int width = 60;

        [SerializeField, Min(Minsize)] private int height = 60;
        [SerializeField] private float waterLevel = -0.5f;
        [SerializeField] private float wallLevel = 0.5f;


        [Header("Points of Interest Settings")] [SerializeField, Range(1, 3)]
        private int emptyCellsAroundPoints = 1;

        [SerializeField, Min(1)] private int numOfEnemySpawners = 1;
        [SerializeField, Min(1)] private int minimalDistance = 1;
        [SerializeField] private bool assumeCanSwim; 

        [Header("Debug Seed")] [SerializeField]
        private GlobalSeedGameObject providedSeed;

        private TileType[,] _mapInit;
        private Vector3Int _destinationPos;
        private Grid _grid;

        private List<SpawnerCandidate> _reachableCandidates = new List<SpawnerCandidate>();
        private PathFindingUtils _pathUtils;

        private const int Minsize = 20;
        private const int Fogland = 5;
        private int _boundsW0; // playable start X
        private int _boundsH0; // playable start Y
        private int _boundsWX; // playable end X
        private int _boundsHY; // playable end Y;

        public void Awake()
        {
            int playableWidth  = width;
            int playableHeight = height;

            // padding is at max FOGLAND, or 20% of size
            int padX = Mathf.Max((int)(playableWidth  * 0.20f), Fogland);
            int padY = Mathf.Max((int)(playableHeight * 0.20f), Fogland);

            // Expand total map by padding on BOTH sides
            width  = playableWidth  + padX * 2;
            height = playableHeight + padY * 2;

            // playable bounds inside the expanded map
            _boundsW0 = padX;                     // start X
            _boundsH0 = padY;                     // start Y
            _boundsWX = padX + playableWidth;     // end X
            _boundsHY = padY + playableHeight;    // end Y

            Debug.Log($"[Map Generator]: Actual size = 0-{width} x 0-{height}, " +
                      $"Playable area = {_boundsW0}-{_boundsWX} x {_boundsH0}-{_boundsHY}");
        }
        
        public TileType[,] GenerateMap(Seed seed)
        {
            _mapInit = new TileType[width, height];
            bool skipGeneration = false;

            if (providedSeed != null)
            {
                seed = providedSeed.GetNextSeed();
            }
            
            seed ??= new Seed(240_11_8, -1, "missingSeedInMapGen", false);
            
            seed.IsBitBased =  false;
            seed.NormalizeSeedValue();
            ulong seedVal = seed.GetBaseValue();
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
            
            Debug.Log($"Seed = {seedVal}");
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

                case MapTypes.Chaotic:
                    // Clamp to higher frequency, more water
                    freq = Mathf.Clamp(freq, 0.04f, 0.05f);
                    octaves = Mathf.Clamp(octaves, 4, 5);
                    gain = Mathf.Clamp(gain, 0.6f, 0.8f);
                    noise.SetFractalType(FastNoiseLite.FractalType.PingPong);
                    break;

                case MapTypes.Static:
                    _mapInit = DeterministicMapRetrieve(d5);
                    skipGeneration = true;
                    break;
            }
            
            // Apply noise settings
            noise.SetFractalGain(gain);
            noise.SetFractalWeightedStrength(weightedStrength);
            noise.SetFractalLacunarity(lacunarity);
            noise.SetFractalOctaves(octaves);
            noise.SetFrequency(freq);
            Debug.Log($"Clamped ({mapType}): freq={freq}, oct={octaves}, gain={gain}");
            
            // Generate map
            if (!skipGeneration)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // Outside Playable Area
                        if (x < _boundsW0 || x >= _boundsWX ||
                            y < _boundsH0 || y >= _boundsHY)
                        {
                            _mapInit[x, y] = TileType.DONT_EXISTS;
                            continue;
                        }
                        
                        float n = noise.GetNoise(x, y);

                        TileType tile;
                        if (n < waterLevel) // bellow sea level water
                            tile = TileType.WATER;
                        else if (n > wallLevel) // mountains
                            tile = TileType.WALL;
                        else // land
                            tile = TileType.EMPTY;

                        _mapInit[x, y] = tile;
                    }
                }
            }

            Vector3Int dst = DecideDestination(d5);
            _destinationPos = dst;
                
            CleanUpDestination(dst.x, dst.y);
            Debug.Log($"Destination prepared at position {_destinationPos}");
            
            return _mapInit;
        }
        
        public int Width => width;
        public int Height => height;
        public int NumOfEnemySpawners => numOfEnemySpawners;
        public MapTypes Type => mapType;

        private TileType[,] DeterministicMapRetrieve(int d5)
        {
            _mapInit = new TileType[width, height];
            float[,] mapHere = DeterministicMap.noiseMatrix;

            int noiseSize = mapHere.GetLength(0);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x < _boundsW0 || x >= _boundsWX ||
                        y < _boundsH0 || y >= _boundsHY)
                    {
                        _mapInit[x, y] = TileType.DONT_EXISTS;
                        continue;
                    }
                    
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
            Debug.Log($"[DETERMINISTIC] Destination set to {_destinationPos}");

            return _mapInit;
        }

        private void CleanUpDestination(int posX, int posY)
        {
            int x0Bound = Mathf.Clamp(posX - emptyCellsAroundPoints, _boundsW0, _boundsWX-1);
            int x1Bound = Mathf.Clamp(posX + emptyCellsAroundPoints, _boundsW0, _boundsWX-1);
            int y0Bound = Mathf.Clamp(posY - emptyCellsAroundPoints, _boundsH0, _boundsHY-1);
            int y1Bound = Mathf.Clamp(posY + emptyCellsAroundPoints, _boundsH0, _boundsHY-1);

            for (int x = x0Bound; x <= x1Bound; x++)
            {
                for (int y = y0Bound; y <= y1Bound; y++)
                {
                    _mapInit[x, y] = TileType.EMPTY;
                }
            }
        }

        private Vector3Int DecideDestination(int value)
        {
            switch (value)
            {
                default:
                    return FindCentralDestination();   // 0 or 9

                case 1:
                case 2:
                    // Top-right corner 
                    return FindCornerDestination(_boundsWX - 2, _boundsHY - 2, -1, -1);

                case 3:
                case 4:
                    // Top-left corner 
                    return FindCornerDestination(_boundsW0 + 1, _boundsHY - 2, 1, -1);

                case 5:
                case 6:
                    // Bottom-left corner 
                    return FindCornerDestination(_boundsW0 + 1, _boundsH0 + 1, 1, 1);

                case 7:
                case 8:
                    // Bottom-right corner, inward diagonal
                    return FindCornerDestination(_boundsWX - 2, _boundsH0 + 1, -1, 1);
            }
        }
        
        private Vector3Int FindCentralDestination()
        {
            int cx = _boundsWX;
            int cy = _boundsHY / 2;

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
                return new Vector3Int(x, y,0);
            }

            int steps = 1;
            int dirIndex = 0;

            while (steps < Mathf.Max(_boundsWX, _boundsHY))
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
                            return new Vector3Int(x, y,0);
                        }
                    }

                    dirIndex = (dirIndex + 1) % 4;
                }

                steps++;
            }

            Debug.LogWarning("No EMPTY destination found.");
            return new Vector3Int(cx, cy,0);
        }
        
        private Vector3Int FindCornerDestination(int startX, int startY, int dx, int dy)
        {
            int x = startX;
            int y = startY;

            while (x >= _boundsW0 && x < _boundsWX && y >= _boundsH0 && y < _boundsHY)
            {
                if (IsValidAndEmpty(x, y))
                {
                    bool isEdge =
                        x == _boundsW0 || y == _boundsH0 ||
                        x == _boundsWX - 1 || y == _boundsHY - 1;


                    if (!isEdge)
                    {
                        bool isTooClose =
                            x == _boundsW0 + 1 || y == _boundsH0 + 1 ||
                            x == _boundsWX - 2 || y == _boundsHY - 2;

                        if (isTooClose)
                        {
                            // Skip this tile, keep searching
                            x += dx;
                            y += dy;
                            continue;
                        }
                    }

                    CleanUpDestination(x, y);
                    return new Vector3Int(x, y, 0);
                }

                x += dx;
                y += dy;
            }

            Debug.LogWarning("No EMPTY destination found.");
            return new Vector3Int(startX + dx, startY + dy, 0);
        }
        
        private bool IsValidAndEmpty(int x, int y)
        {
            if (x < _boundsW0 || y < _boundsH0 || x >= _boundsWX || y >= _boundsHY)
                return false;

            if (_destinationPos.x == x && _destinationPos.y == y)
                return false;
            
            return _mapInit[x, y] == TileType.EMPTY;
        }

        private bool IsValidForFallback(int x, int y)
        {
            if (x < _boundsW0 || y < _boundsH0 || x >= _boundsWX || y >= _boundsHY)
                return false;

            if (_destinationPos.x == x && _destinationPos.y == y)
                return false;
            
            return (_mapInit[x, y] == TileType.WALL) || (_mapInit[x, y] == TileType.WATER);
        }

        public Vector3Int GetDestinationPosition()
        {
            return this._destinationPos;
        }
        
        public Vector3 GetDestinationWorldPosition()
        {
            Vector3 destinationWorldPosition = new Vector3(_destinationPos.x * _grid.GetCellSize(), _destinationPos.y * _grid.GetCellSize(), 0);
            return destinationWorldPosition;
        }

        public void setGrid(Grid grid)
        {
            this._grid = grid;

            // Create pathfinding helper
            _pathUtils = new PathFindingUtils(grid);
        }

        private bool IsCandidateSpawnerTile(int x, int y)
        {
            if (!IsValidAndEmpty(x, y))
                return false;

            // Distance from destination must be >= minimalDistance
            int dx = Mathf.Abs(x - _destinationPos.x);
            int dy = Mathf.Abs(y - _destinationPos.y);

            if (dx + dy < minimalDistance)
                return false;

            return true;
        }
        
        public void BuildValidSpawnerCandidates()
        {
            _reachableCandidates.Clear();

            if (_pathUtils == null)
            {
                Debug.LogError("MapGenerator: PathFindingUtils not set! Did you call setGrid() first?");
                return;
            }
            
            Vector3 dstWorld = GetDestinationWorldPosition();
            for (int x = _boundsW0; x < _boundsWX; x++)
            {
                for (int y = _boundsH0; y < _boundsHY; y++)
                {
                    if (!IsCandidateSpawnerTile(x, y))
                        continue;
                    
                    float cellSize = _grid.GetCellSize();
                    Vector3 startWorld = new Vector3(x * cellSize, y * cellSize, 0);
                    List<Vector3> path;
                    path = _pathUtils.FindPath(startWorld, dstWorld, assumeCanSwim, false, false);

                    
                    if (path == null || path.Count < 2)
                        continue;

                    int pathLength = path.Count; 

                    _reachableCandidates.Add(new SpawnerCandidate(
                        new Vector3Int(x, y, 0),
                        pathLength
                    ));
                }
            }
            Debug.Log($"Spawner candidate tiles found: {_reachableCandidates.Count}");
        }

        private void SortCandidatesByDistance()
        {
            _reachableCandidates.Sort((a, b) => b.distanceFromDestination.CompareTo(a.distanceFromDestination));
        }
        
        public Vector3Int[] SelectSpawnerPositions(int count)
        {
            SortCandidatesByDistance();

            List<Vector3Int> result = new List<Vector3Int>();

            foreach (var c in _reachableCandidates)
            {
                if (result.Count == count)
                    break;

                bool tooClose = false;

                foreach (var existing in result)
                {
                    int dx = Mathf.Abs(existing.x - c.pos.x);
                    int dy = Mathf.Abs(existing.y - c.pos.y);

                    if (dx + dy < minimalDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                    result.Add(c.pos);
            }

            return result.ToArray();
        }

        public void CreateMapBounds()
        {
            float cellSize = _grid.GetCellSize();

            // Convert grid bounds to world-space coordinates
            float left   = _boundsW0 * cellSize;
            float right  = _boundsWX * cellSize;
            float bottom = _boundsH0 * cellSize;
            float top    = _boundsHY * cellSize;
            
            float thickness = cellSize * 2f;   // wall thickness
            float overlap   = thickness;  // extension to avoid all gaps
            
            // Create parent object for organization
            GameObject wallsParent = new GameObject("MapBounds2D");
            wallsParent.transform.SetParent(transform);
            
            float heightExtended = (top - bottom) + overlap * 2f;
            float widthExtended  = (right - left) + overlap * 2f;
            
            // LEFT
            CreateWall2D(
                new Vector2(left - thickness / 2f, (top + bottom) / 2f),
                new Vector2(thickness, heightExtended),
                wallsParent.transform
            );

            // RIGHT
            CreateWall2D(
                new Vector2(right + thickness / 2f, (top + bottom) / 2f),
                new Vector2(thickness, heightExtended),
                wallsParent.transform
            );

            // BOTTOM
            CreateWall2D(
                new Vector2((left + right) / 2f, bottom - thickness / 2f),
                new Vector2(widthExtended, thickness),
                wallsParent.transform
            );

            // TOP
            CreateWall2D(
                new Vector2((left + right) / 2f, top + thickness / 2f),
                new Vector2(widthExtended, thickness),
                wallsParent.transform
            );
            
        }
        
        private void CreateWall2D(Vector2 center, Vector2 size, Transform parent)
        {
            GameObject wall = new GameObject("Wall2D");
            wall.transform.position = center;
            wall.transform.SetParent(parent);

            BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
            col.isTrigger = false;
        }

        public void SpawnersFallback(int reach)
        {
            Vector3Int dest = GetDestinationPosition();

            // Loop through all coordinates in a square around dest
            for (int dx = -reach; dx <= reach; dx++)
            {
                for (int dy = -reach; dy <= reach; dy++)
                {
                    // Chebyshev distance: ensures we include diagonals and everything within "reach"
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= reach)
                    {
                        int x = dest.x + dx;
                        int y = dest.y + dy;
                        
                        if (IsValidForFallback(x, y))
                        {
                            _mapInit[x, y] = TileType.EMPTY;
                        }
                    }
                }
            }
            
        }

        public int ReachableCandidatesCount()
        {
            return _reachableCandidates.Count;
        }

        public Grid.TileType[,] GetCurrentMap()
        {
            return _mapInit;
        }

    }
}