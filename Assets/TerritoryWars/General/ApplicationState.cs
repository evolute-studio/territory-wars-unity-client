namespace TerritoryWars.General
{
    public enum ApplicationStates
    {
        Initializing,
        Menu,
        MatchTab,
        SnapshotTab,
        Session,
    }
    
    public static class ApplicationState
    {
        public static ApplicationStates CurrentState = ApplicationStates.Initializing;
        
        public static void SetState(ApplicationStates state)
        {
            CurrentState = state;
        }
        
    }
}