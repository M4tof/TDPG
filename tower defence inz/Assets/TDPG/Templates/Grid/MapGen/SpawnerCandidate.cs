using UnityEngine;

namespace TDPG.Templates.Grid.MapGen
{
    /// <summary>
    /// A lightweight data structure representing a potential location for an enemy spawner.
    /// <br/>
    /// Used by the <see cref="MapGenerator"/> to rank and select spawn points based on their 
    /// distance from the player/base.
    /// </summary>
    public struct SpawnerCandidate
    {
        /// <summary>
        /// The grid coordinates of the candidate tile.
        /// </summary>
        public Vector3Int pos;
        
        /// <summary>
        /// The calculated distance (pathfinding steps) from this tile to the map's Destination.
        /// <br/>
        /// Higher values usually indicate a "better" candidate (more challenging) for map generation.
        /// </summary>
        public int distanceFromDestination;

        /// <summary>
        /// Creates a new candidate.
        /// </summary>
        /// <param name="p">Grid position.</param>
        /// <param name="d">Calculated distance score.</param>
        public SpawnerCandidate(Vector3Int p, int d)
        {
            pos = p;
            distanceFromDestination = d;
        }
    }
}