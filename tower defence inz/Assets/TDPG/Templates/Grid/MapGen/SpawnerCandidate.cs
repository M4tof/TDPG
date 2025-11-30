using UnityEngine;

namespace TDPG.Templates.Grid.MapGen
{
    public struct SpawnerCandidate
    {
        public Vector3Int pos;
        public int distanceFromDestination;

        public SpawnerCandidate(Vector3Int p, int d)
        {
            pos = p;
            distanceFromDestination = d;
        }
    }
}