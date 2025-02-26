using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        
        public Image GetAvatar(int id)
        {
            return Characters.Find(character => character.Id == id).Avatar;
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
            public Image Avatar;
            public int Id;
        }
    }
    
}