namespace TerritoryWars.General
{
    public enum GameStateEnum
    {
        MainMenu,
        Game,
        GameOver
    }
    
    public static class GameState
    {
        public static GameStateEnum CurrentState { get; private set; } = GameStateEnum.MainMenu;

        public static void SetState(GameStateEnum state)
        {
            CurrentState = state;
        }
        
    }
}