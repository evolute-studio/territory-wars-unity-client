using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.Tools
{
    [CreateAssetMenu(fileName = "CharacterObject", menuName = "CharacterObject", order = 0)]
    
    public class CharactersObject : ScriptableObject
    {
        public List<CharacterInfo> Characters;
        
        public RuntimeAnimatorController GetAnimatorController(int id)
        {
            return Characters.Find(character => character.Id == id).AnimatorController;
        }
        
        public Sprite GetIcon(int id)
        {
            return Characters.Find(character => character.Id == id).Icon;
        }
        
        public string GetCharacterName(int id)
        {
            return Characters.Find(character => character.Id == id).CharacterName;
        }

        
        [Serializable]
        public class CharacterInfo
        {
            public string CharacterName;
            public RuntimeAnimatorController AnimatorController;
            public Sprite Icon;
            public int Id;
        }
    }
    
}