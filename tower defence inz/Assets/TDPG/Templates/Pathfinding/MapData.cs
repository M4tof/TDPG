using UnityEngine;

[CreateAssetMenu(fileName = "New Map Data", menuName = "Pathfinding/Map Data")]
public class MapData : ScriptableObject
{
    [System.Serializable]
    public struct Row
    {
        public int[] row;
    }

    public Row[] mapBitmap;
}