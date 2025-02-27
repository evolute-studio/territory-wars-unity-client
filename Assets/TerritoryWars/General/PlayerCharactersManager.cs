using System.Collections.Generic;

namespace TerritoryWars.General
{
    public class PlayerCharactersManager
    {
        private static int _currentCharacterId = 0;
        private static int _opponentCharacterId => SessionManager.Instance.IsLocalPlayerBlue ? SessionManager.Instance.PlayersData[1].skin_id : SessionManager.Instance.PlayersData[0].skin_id;
        private List<int> _availableCharacters = new List<int> { 0, 1 };
        
        
        public static void ChangeCurrentCharacterId(int id)
        {
            _currentCharacterId = id;
        }
        
        public static int GetCurrentCharacterId()
        {
            return _currentCharacterId;
        }
        
        public static int GetOpponentCurrentCharacterId()
        {
            return _opponentCharacterId;
        }
        
        // public static void ChangeOpponentCurrentCharacterId(int id)
        // {
        //     _opponentCharacterId = id;
        // }
        
        public bool IsCharacterAvailable(int id)
        {
            return _availableCharacters.Contains(id);
        }
        
        public void AddCharacter(int id)
        {
            if(_availableCharacters.Contains(id))
                return;
            _availableCharacters.Add(id);
        }
    }
}