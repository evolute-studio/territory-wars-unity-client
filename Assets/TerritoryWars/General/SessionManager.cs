using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TerritoryWars.General
{
    public class PlayerData
    {
        public string player_id;
        public string username;

        public PlayerData(evolute_duel_Player model)
        {
            UpdatePlayerData(model);
        }

        public void UpdatePlayerData(evolute_duel_Player profile)
        {
            if (profile == null) return;
            player_id = profile.player_id.Hex();
            username = CairoFieldsConverter.GetStringFromFieldElement(profile.username);
        }
    }

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

        public Character[] Players;
        public PlayerData[] PlayersData;
        public Character CurrentTurnPlayer { get; private set; }
        public Character LocalPlayer { get; private set; }
        public Character RemotePlayer { get; private set; }

        public bool IsLocalPlayerTurn => CurrentTurnPlayer == LocalPlayer;

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

        public void DeactivateJoker()
        {
            isJokerActive = false;
            gameUI.UpdateUI();
        }

        public TileData GetGenerateJokerTile(int x, int y)
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
            char[] baseSides = new char[4];
            char[] randomSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                Side side = (Side)i;
                if (neighborSides.ContainsKey(side))
                {
                    baseSides[i] = LandscapeToChar(neighborSides[side]);
                    randomSides[i] = LandscapeToChar(neighborSides[side]);
                }
                else
                {
                    baseSides[i] = 'X';
                    randomSides[i] = GetRandomLandscape();
                }
            }

            string baseTileConfig = new string(baseSides);
            string randomTileConfig = new string(randomSides);

            (string tileConfig, int rotation) =
                OnChainBoardDataConverter.GetRandomTypeAndRotationFromDeck(baseTileConfig);
            string configResult = String.IsNullOrEmpty(tileConfig) ? randomTileConfig : tileConfig;
            TileData jokerTile = new TileData(randomTileConfig);
            //jokerTile.Rotate((rotation + 2) % 4);
            return jokerTile;
        }

        private char GetRandomLandscape()
        {
            float random = Random.value;
            if (random < 0.4f) return 'F'; // 40% шанс поля
            else if (random < 0.7f) return 'R'; // 30% шанс дороги
            else return 'C'; // 30% шанс міста
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
            Players = new Character[2];
            PlayersData = new PlayerData[2];

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

            Players[0] = player1.GetComponent<Character>();
            Players[1] = player2.GetComponent<Character>();

            Players[0].Initialize(board.player1.Item1, board.player1.Item2);
            Players[1].Initialize(board.player2.Item1, board.player2.Item2);

            PlayersData[0] = new PlayerData(DojoGameManager.Instance.GetPlayerData(Players[0].Address.Hex()));
            PlayersData[1] = new PlayerData(DojoGameManager.Instance.GetPlayerData(Players[1].Address.Hex()));


            Players[0].transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
            CurrentTurnPlayer = Players[0];
            LocalPlayer = Players[0].Address.Hex() == DojoGameManager.Instance.LocalBurnerAccount.Address.Hex()
                ? Players[0]
                : Players[1];
            RemotePlayer = LocalPlayer == Players[0] ? Players[1] : Players[0];

            // Анімація спуску персонажів по дузі
            Players[0].transform
                .DOPath(path1, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            Players[1].transform
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

            DojoGameManager.Instance.SessionManager.OnMoveReceived += HandleMove;
            DojoGameManager.Instance.SessionManager.OnSkipMoveReceived += SkipMove;
        }

        private void StartLocalTurn()
        {
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.SetSkipTurnButtonActive(true);

            TileData currentTile = DojoGameManager.Instance.SessionManager.GetTopTile();
            currentTile.OwnerId = LocalPlayer.LocalId;
            TileSelector.StartTilePlacement(currentTile);
            gameUI.SetActiveDeckContainer(true);
        }

        private void StartRemoteTurn()
        {
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.SetSkipTurnButtonActive(false);
            UpdateTile();
            gameUI.SetActiveDeckContainer(false);
        }

        private void HandleMove(string playerAddress, TileData tile, Vector2Int position, int rotation)
        {
            evolute_duel_Player player = DojoGameManager.Instance.GetPlayerData(playerAddress);
            if (playerAddress == Players[0].Address.Hex() && player != null)
            {
                PlayersData[0].UpdatePlayerData(player);
            }
            else if (playerAddress == Players[1].Address.Hex() && player != null)
            {
                PlayersData[1].UpdatePlayerData(player);
            }

            if (playerAddress == LocalPlayer.Address.Hex()) CompleteEndTurn(playerAddress);
            else StartCoroutine(HandleOpponentMoveCoroutine(playerAddress, tile, position, rotation));
        }

        private void SkipMove(string playerAddress)
        {
            TileSelector.ClearHighlights();
            CompleteEndTurn(playerAddress);
        }

        private IEnumerator HandleOpponentMoveCoroutine(string playerAddress, TileData tile, Vector2Int position,
            int rotation)
        {
            tile.Rotate(rotation);
            tile.OwnerId = RemotePlayer.LocalId;
            TileSelector.SetCurrentTile(tile);
            TileSelector.tilePreview.SetPosition(position.x + 1, position.y + 1);
            yield return new WaitForSeconds(0.3f);
            TileSelector.tilePreview.PlaceTile(() =>
            {
                Board.PlaceTile(tile, position.x + 1, position.y + 1, RemotePlayer.LocalId);
            });
            yield return new WaitForSeconds(1f);
            TileSelector.tilePreview.ResetPosition();
            //CompleteEndTurn();
            CompleteEndTurn(playerAddress);
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
            return CurrentTurnPlayer == Players[0] ? 0 : 1;
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

        public void SkipMove()
        {
            DojoGameManager.Instance.SessionManager.SkipMove();
        }

        public void CompleteEndTurn(string lastMovePlayerAddress)
        {
            bool isLocalPlayer = lastMovePlayerAddress == LocalPlayer.Address.Hex();

            if (isLocalPlayer)
            {
                CurrentTurnPlayer = RemotePlayer;
                StartRemoteTurn();
            }
            else
            {
                CurrentTurnPlayer = LocalPlayer;
                StartLocalTurn();
                gameUI.SetActiveDeckContainer(true);
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

            DojoGameManager.Instance.SessionManager.OnMoveReceived -= HandleMove;
            DojoGameManager.Instance.SessionManager.OnSkipMoveReceived -= SkipMove;
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
            gameUI.SetJokerMode(false);
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
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            // Створюємо напівпрозорий фон для тексту
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            backgroundTexture.Apply();
            style.normal.background = backgroundTexture;

            // Відступи для тексту
            int padding = 10;
            int yPosition = 10;

            // Розраховуємо позицію по центру екрану
            float screenCenterX = Screen.width / 2f;
            float labelWidth = 300f;
            float xPosition = screenCenterX - labelWidth / 2f;

            // Показуємо чий зараз хід
            string turnInfo = $"Waiting for {(CurrentTurnPlayer == LocalPlayer ? "Your" : PlayersData[1].username)} turn";
            GUI.Label(new Rect(xPosition, yPosition, labelWidth, 30), turnInfo, style);

            // Якщо очікуємо хід противника
            if (CurrentTurnPlayer != LocalPlayer)
            {
                yPosition += 40;
                float time = Time.time;
                string waitingText = "waiting for the turn" + new string('.', (int)(time % 3) + 1);
                GUI.Label(new Rect(xPosition, yPosition, labelWidth, 30), waitingText, style);
            }

            // Очищаємо створену текстуру
            Destroy(backgroundTexture);
        }
    }
}