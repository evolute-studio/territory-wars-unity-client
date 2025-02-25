using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.General
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public float StartDuration = 5f;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }


        public Board Board;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private SessionUI sessionUI;
        [SerializeField] private DeckManager deckManager;
        public TileSelector TileSelector;

        public Vector3[] SpawnPoints;
        public AnimationCurve spawnCurve;

        public Character[] Characters;
        public CharactersObject CharactersObject;
        public Character CurrentCharacter { get; private set; }

        private int[] jokerCount = new int[] { 3, 3 }; 
        private bool isJokerActive = false;
        
        public bool IsJokerActive => isJokerActive;
        
        public void ActivateJoker()
        {
            if (jokerCount[CurrentCharacter.Id] > 0)
            {
                isJokerActive = true;
                jokerCount[CurrentCharacter.Id]--;
                TileSelector.StartJokerPlacement();
            }
        }
        
        public void GenerateJokerTile(int x, int y)
        {
            Dictionary<Side, LandscapeType> neighborSides = new Dictionary<Side, LandscapeType>();
            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + Board.GetXOffset(side);
                int newY = y + Board.GetYOffset(side);
                
                if (Board.IsValidPosition(newX, newY) && Board.GetTileData(newX, newY) != null)
                {
                    var neighborTile = Board.GetTileData(newX, newY);
                    neighborSides[side] = neighborTile.GetSide(Board.GetOppositeSide(side));
                }
            }
            
            
            char[] sides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                Side side = (Side)i;
                if (neighborSides.ContainsKey(side))
                {
                    sides[i] = LandscapeToChar(neighborSides[side]);
                }
                else
                {
                    
                    sides[i] = GetRandomLandscape();
                }
            }
            
            string tileConfig = new string(sides);
            TileData jokerTile = new TileData(tileConfig);
            TileSelector.StartJokerTilePlacement(jokerTile, x, y);
        }
        
        private char GetRandomLandscape()
        {
            float random = Random.value;
            if (random < 0.4f) return 'F';      // 40% шанс поля
            else if (random < 0.7f) return 'R';  // 30% шанс дороги
            else return 'C';                     // 30% шанс міста
        }
        
        private char LandscapeToChar(LandscapeType type)
        {
            return type switch
            {
                LandscapeType.City => 'C',
                LandscapeType.Road => 'R',
                LandscapeType.Field => 'F',
                _ => 'F'
            };
        }

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            Characters = new Character[2];

            // Створюємо точки для дугової траєкторії для першого персонажа
            Vector3[] path1 = new Vector3[3];
            path1[0] = new Vector3(SpawnPoints[0].x, SpawnPoints[0].y + 15, 0); // Початкова точка
            path1[1] = new Vector3(SpawnPoints[0].x - 5, SpawnPoints[0].y + 7, 0); // Контрольна точка дуги
            path1[2] = SpawnPoints[0]; // Кінцева точка

            // Створюємо точки для дугової траєкторії для другого персонажа
            Vector3[] path2 = new Vector3[3];
            path2[0] = new Vector3(SpawnPoints[1].x, SpawnPoints[1].y + 15, 0);
            path2[1] = new Vector3(SpawnPoints[1].x + 5, SpawnPoints[1].y + 7, 0);
            path2[2] = SpawnPoints[1];
            
            PlayerCharactersManager.ChangeCurrentCharacterId(1);// only for test
            PlayerCharactersManager.ChangeOpponentCurrentCharacterId(0);// only for test
            GameObject player1 = Instantiate(PrefabsManager.Instance.GetNextPlayer(), path1[0], Quaternion.identity);
            GameObject player2 = Instantiate(PrefabsManager.Instance.GetNextPlayer(), path2[0], Quaternion.identity);

            Characters[0] = player1.GetComponent<Character>();
            Characters[1] = player2.GetComponent<Character>();
            Characters[0].transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
            CurrentCharacter = Characters[0];
            Characters[0].Id = 0;
            Characters[1].Id = 1;

            Characters[0].SetAnimatorController(CharactersObject.GetAnimatorController(PlayerCharactersManager.GetCurrentCharacterId()));
            Characters[1].SetAnimatorController(CharactersObject.GetAnimatorController(PlayerCharactersManager.GetOpponentCurrentCharacterId()));

            // Анімація спуску персонажів по дузі
            Characters[0].transform
                .DOPath(path1, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            Characters[1].transform
                .DOPath(path2, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            // Підписуємось на події ходу
            TileSelector.OnTurnStarted.AddListener(OnTurnStarted);
            TileSelector.OnTurnEnding.AddListener(OnTurnEnding);

            Invoke(nameof(StartNewTurn), 2f);
        }

        private void OnTurnStarted()
        {
            // Активуємо поточного персонажа
            CurrentCharacter.StartSelecting();

        }

        private void OnTurnEnding()
        {
            // Деактивуємо поточного персонажа
            CurrentCharacter.EndTurn();

        }
        
        public int GetCurrentCharacter()
        {
            return CurrentCharacter == Characters[0] ? 0 : 1;
        }

        private void StartNewTurn()
        {
            if (!deckManager.HasTiles)
            {
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);
                return;
            }

            TileData currentTile = deckManager.DrawTile();
            TileSelector.StartTilePlacement(currentTile);
        }

        public void RotateCurrentTile()
        {
            TileSelector.RotateCurrentTile();
        }

        public void EndTurn() 
        {
            if (TileSelector.CurrentTile != null)
            {
                TileSelector.PlaceCurrentTile();
            }
        }

        public void CompleteEndTurn()
        {
            // Змінюємо поточного персонажа
            CurrentCharacter = CurrentCharacter == Characters[0] ? Characters[1] : Characters[0];

            StartNewTurn();
        }

        private void OnDestroy()
        {
            // Відписуємось від подій
            if (TileSelector != null)
            {
                TileSelector.OnTurnStarted.RemoveListener(OnTurnStarted);
                TileSelector.OnTurnEnding.RemoveListener(OnTurnEnding);
            }
        }

        public int GetJokerCount(int playerId)
        {
            return jokerCount[playerId];
        }

        public bool CanUseJoker()
        {
            int characterId = CurrentCharacter == null ? 0 : CurrentCharacter.Id;
            return !isJokerActive && jokerCount[characterId] > 0;
        }

        public void CompleteJokerPlacement()
        {
            isJokerActive = false;
            gameUI.UpdateUI();
            sessionUI.UseJoker(CurrentCharacter.Id);
        }
    }
}