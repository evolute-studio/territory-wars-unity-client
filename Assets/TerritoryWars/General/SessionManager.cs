using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.Tile;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.General
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public float StartDuration = 5f;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("SessionManager already exists. Deleting new instance.");
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
        public Character CurrentTurnPlayer { get; private set; }
        public Character LocalPlayer { get; private set; }
        public Character RemotePlayer { get; private set; }

        private int[] jokerCount = new int[] { 3, 3 }; 
        private bool isJokerActive = false;
        
        public bool IsJokerActive => isJokerActive;
        
        public void ActivateJoker()
        {
            if (jokerCount[CurrentTurnPlayer.LocalId] > 0)
            {
                isJokerActive = true;
                jokerCount[CurrentTurnPlayer.LocalId]--;
                TileSelector.StartJokerPlacement();
            }
        }
        
        public void GenerateJokerTile(int x, int y)
        {
            // Отримуємо інформацію про сусідні тайли
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
            
            // Генеруємо новий тайл
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

        public void Start()
        {
            Invoke(nameof(Initialize), 5f);
        }

        public void Initialize()
        {
            InitializePlayers();
            Board.Initialize();
            gameUI.Initialize();
            StartGame();
        }

        private void InitializePlayers()
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

            GameObject player1 = Instantiate(PrefabsManager.Instance.GetNextPlayer(), path1[0], Quaternion.identity);
            GameObject player2 = Instantiate(PrefabsManager.Instance.GetNextPlayer(), path2[0], Quaternion.identity);

            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
            
            Characters[0] = player1.GetComponent<Character>();
            Characters[1] = player2.GetComponent<Character>();
            
            Characters[0].Initialize(board.player1.Item1, board.player1.Item2);
            Characters[1].Initialize(board.player2.Item1, board.player2.Item2);
            
            
            Characters[0].transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
            CurrentTurnPlayer = Characters[0];
            LocalPlayer = Characters[0].Address.Hex() == DojoGameManager.Instance.LocalBurnerAccount.Address.Hex() 
                ? Characters[0] : Characters[1];
            RemotePlayer = LocalPlayer == Characters[0] ? Characters[1] : Characters[0];

            // Анімація спуску персонажів по дузі
            Characters[0].transform
                .DOPath(path1, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            Characters[1].transform
                .DOPath(path2, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);
        }

        public void StartGame()
        {
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            // Підписуємось на події ходу
            TileSelector.OnTurnStarted.AddListener(OnTurnStarted);
            TileSelector.OnTurnEnding.AddListener(OnTurnEnding);

            if (CurrentTurnPlayer == LocalPlayer)
            {
                Invoke(nameof(StartLocalTurn), 2f);
            }
            else
            {
                Invoke(nameof(StartRemoteTurn), 2f);
            }
        }

        private void StartLocalTurn()
        {
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);

            TileData currentTile = DojoGameManager.Instance.SessionManager.GetTopTile();
            currentTile.OwnerId = LocalPlayer.LocalId;
            TileSelector.StartTilePlacement(currentTile);
        }

        private void StartRemoteTurn()
        {
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            UpdateTile();
            // Тут можна додати логіку для відображення "Очікування ходу противника"
            StartListeningForOpponentMove();
        }

        private void StartListeningForOpponentMove()
        {
            // Підписуємось на події від мережевого менеджера
            DojoGameManager.Instance.SessionManager.OnOpponentMoveReceived += HandleOpponentMove;
        }

        private void HandleOpponentMove(TileData tile, Vector2Int position, int rotation)
        {
            // Відписуємось від подій
            DojoGameManager.Instance.SessionManager.OnOpponentMoveReceived -= HandleOpponentMove;
            
            StartCoroutine(HandleOpponentMoveCoroutine(tile, position, rotation));
            
        }

        private IEnumerator HandleOpponentMoveCoroutine(TileData tile, Vector2Int position, int rotation)
        {
            tile.Rotate(rotation);
            tile.OwnerId = RemotePlayer.LocalId;
            TileSelector.SetCurrentTile(tile);
            TileSelector.tilePreview.SetPosition(position.x, position.y);
            yield return new WaitForSeconds(0.3f);
            TileSelector.tilePreview.PlaceTile(() =>
            {
                Board.PlaceTile(tile, position.x, position.y, RemotePlayer.LocalId);
            });
            yield return new WaitForSeconds(1f);
            TileSelector.tilePreview.ResetPosition();
            CompleteEndTurn();
        }

        private void UpdateTile()
        {
            TileData currentTile = DojoGameManager.Instance.SessionManager.GetTopTile();
            currentTile.OwnerId = RemotePlayer.LocalId;
            TileSelector.SetCurrentTile(currentTile);
        }

        private void OnTurnStarted()
        {
            // Активуємо поточного персонажа
            CurrentTurnPlayer.StartSelecting();

        }

        private void OnTurnEnding()
        {
            // Деактивуємо поточного персонажа
            CurrentTurnPlayer.EndTurn();

        }
        
        public int GetCurrentCharacter()
        {
            return CurrentTurnPlayer == Characters[0] ? 0 : 1;
        }

        public void RotateCurrentTile()
        {
            TileSelector.RotateCurrentTile();
        }

        public void EndTurn()
        {
            if (TileSelector.CurrentTile != null && CurrentTurnPlayer == LocalPlayer)
            {
                TileSelector.PlaceCurrentTile();
            }
        }

        public void CompleteEndTurn()
        {
            // Змінюємо поточного персонажа
            CurrentTurnPlayer = CurrentTurnPlayer == Characters[0] ? Characters[1] : Characters[0];

            if (CurrentTurnPlayer == LocalPlayer)
            {
                Invoke(nameof(StartLocalTurn), 2f);
            }
            else
            {
                Invoke(nameof(StartRemoteTurn), 2f);
            }
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
            int characterId = CurrentTurnPlayer == null ? 0 : CurrentTurnPlayer.LocalId;
            return !isJokerActive && jokerCount[characterId] > 0;
        }

        public void CompleteJokerPlacement()
        {
            isJokerActive = false;
            gameUI.UpdateUI();
            sessionUI.UseJoker(CurrentTurnPlayer.LocalId);
        }

        private void OnGUI()
        {
            if (LocalPlayer == null || RemotePlayer == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            // Створюємо напівпрозорий фон для тексту
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            backgroundTexture.Apply();
            style.normal.background = backgroundTexture;

            // Відступи для тексту
            int padding = 10;
            int yPosition = 10;
            int xPosition = 10;

            // Показуємо чий зараз хід
            string turnInfo = $"Turn of player: {(CurrentTurnPlayer == LocalPlayer ? "Your" : "Opponent")}";
            GUI.Label(new Rect(xPosition, yPosition, 300, 30), turnInfo, style);

            // // Показуємо кількість джокерів
            // yPosition += 40;
            // string jokersInfo = $"Jokers: Ви ({GetJokerCount(LocalPlayer.LocalId)}) | Противник ({GetJokerCount(RemotePlayer.LocalId)})";
            // GUI.Label(new Rect(xPosition, yPosition, 300, 30), jokersInfo, style);

            // Якщо очікуємо хід противника
            if (CurrentTurnPlayer != LocalPlayer)
            {
                yPosition += 40;
                float time = Time.time;
                string waitingText = "waiting for the turn" + new string('.', (int)(time % 3) + 1);
                GUI.Label(new Rect(xPosition, yPosition, 300, 30), waitingText, style);
            }

            // Очищаємо створену текстуру
            Destroy(backgroundTexture);
        }
    }
}