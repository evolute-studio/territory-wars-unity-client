using UnityEngine;

namespace TerritoryWars.General
{
    public class PrefabsManager : MonoBehaviour
    {
        public static PrefabsManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public GameObject[] Players;
        public GameObject MillPrefab;
        public GameObject ClashAnimationPrefab;
        private int _currentPlayerIndex = 0;
        
        public GameObject GetNextPlayer()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % Players.Length;
            return Players[_currentPlayerIndex];
        }
        
        public GameObject InstantiateObject(GameObject prefab)
        {
            return Instantiate(prefab);
        }
    }
}