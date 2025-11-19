namespace TDPG.Templates.Pathfinding
{
    public static class PathfindingEvents
    {
        public static System.Action OnGridChanged;
    
        public static void TriggerGridChanged()
        {
            OnGridChanged?.Invoke();
        }
    }
}