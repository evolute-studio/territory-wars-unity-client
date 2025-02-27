using TerritoryWars.General;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class SetLocalPlayerData
    {
        private static bool _isLocalPlayerHost => SessionManager.Instance.IsLocalPlayerBlue;

        public static int[] GetLocalPlayerInt(int bluePlayer, int redPlayer)
        {
            return _isLocalPlayerHost? new[] {bluePlayer, redPlayer} : new [] {redPlayer, bluePlayer};
        }
        
        public static string[] GetLocalPlayerString(string bluePlayer, string redPlayer)
        {
            return _isLocalPlayerHost? new[] {bluePlayer, redPlayer} : new [] {redPlayer, bluePlayer};
        }
        
        public static Image[] GetLocalPlayerImage(Image bluePlayer, Image redPlayer)
        {
            return _isLocalPlayerHost? new[] {bluePlayer, redPlayer} : new [] {redPlayer, bluePlayer};
        }  
        
        public static Sprite[] GetLocalPlayerSprite(Sprite bluePlayer, Sprite redPlayer)
        {
            return _isLocalPlayerHost? new[] {bluePlayer, redPlayer} : new [] {redPlayer, bluePlayer};
        }
        
        public static RuntimeAnimatorController[] GetLocalPlayerAnimator(RuntimeAnimatorController bluePlayer, RuntimeAnimatorController redPlayer)
        {
            return _isLocalPlayerHost? new[] {bluePlayer, redPlayer} : new [] {redPlayer, bluePlayer};
        }

        public static int GetLocalIndex(int index)
        {
            if (index < 0 || index > 1)
            {
                return index; 
            }
            
            return _isLocalPlayerHost ? index : index == 0 ? 1 : 0;
        }
    }
}