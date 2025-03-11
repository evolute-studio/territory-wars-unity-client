using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dojo.Starknet;
using TerritoryWars.Dojo;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
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
        public int skin_id;

        public PlayerData(evolute_duel_Player model)
        {
            UpdatePlayerData(model);
        }
        
        public void UpdatePlayerData(evolute_duel_Player profile)
        {
            if (profile == null) return;
            player_id = profile.player_id.Hex();
            username = CairoFieldsConverter.GetStringFromFieldElement(profile.username);
            skin_id = profile.active_skin;
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

            if (!CustomSceneManager.Instance.LoadingScreen.IsLoading)
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(true, DojoGameManager.Instance.CancelGame, LoadingScreen.connectingText);
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
        public bool IsLocalPlayerHost => LocalPlayer.LocalId == 0;
        
        private bool isJokerActive = false;
        
        public bool IsJokerActive => isJokerActive;
        
        public void ActivateJoker()
        {
            if (Players[CurrentTurnPlayer.LocalId].JokerCount > 0)
            {
                isJokerActive = true;
                Players[CurrentTurnPlayer.LocalId].JokerCount--;
                TileSelector.StartJokerPlacement();
            }
        }
        
        public void DeactivateJoker()
        {
            isJokerActive = false;
            Players[CurrentTurnPlayer.LocalId].JokerCount++;
            gameUI.UpdateUI();
        }
        
        public TileData GetGenerateJokerTile(int x, int y)
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

            // int roadCount = 0;
            // foreach (var side in randomSides)
            // {
            //     if (side == 'R') roadCount++;
            // }

            // if (roadCount == 1)
            // {
            //     for (int i = 0; i < 4; i++)
            //     {
            //         if (randomSides[i] == 'R')
            //         {
            //             int oppositeIndex = (i + 2) % 4;
            //             randomSides[oppositeIndex] = 'R';
            //         }
            //     }
            // }
            
            
            
            string baseTileConfig = new string(baseSides);
            string randomTileConfig = new string(randomSides);
            
            (string tileConfig, int rotation) = OnChainBoardDataConverter.GetRandomTypeAndRotationFromDeck(baseTileConfig);
            string configResult = String.IsNullOrEmpty(tileConfig) ? randomTileConfig : tileConfig;
            TileData jokerTile = new TileData(randomTileConfig);
            //jokerTile.Rotate((rotation + 2) % 4);
            return jokerTile;
        }
        
        private char GetRandomLandscape()
        {
            float random = Random.value;
            if (random < 0.4f) return 'F';      
            else if (random < 0.7f) return 'R';  
            else return 'C';                     
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
            Initialize();
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
        }

        public void Initialize()
        {
            CustomLogger.LogImportant("SessionManager.Initialize()");
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
            FieldElement lastMoveId = board.last_move_id switch
            {
                Option<FieldElement>.Some some => some.value,
                _ => null
            };
            SimpleStorage.SaveCurrentBoardId(board.id.Hex());
            
            InitializePlayers();
            Board.Initialize();
            if (lastMoveId != null)
            {
                CustomLogger.LogImportant("SessionManager.Initialize() - lastMoveId != null");
                evolute_duel_Move lastMove = DojoGameManager.Instance.GetMove(lastMoveId);
                int playerIndex = lastMove.player_side switch
                {
                    PlayerSide.Blue => 1,
                    PlayerSide.Red => 0,
                    _ => -1
                };
                CurrentTurnPlayer = Players[playerIndex];
                List<evolute_duel_Move> moves = DojoGameManager.Instance.GetMoves(new List<evolute_duel_Move>{lastMove});
                CustomLogger.LogImportant("SessionManager.Initialize() - moves.Count: " + moves.Count);
                int moveNumber = 0;
                foreach (var move in moves)
                {
                    int owner = move.player_side switch
                    {
                        PlayerSide.Blue => 0,
                        PlayerSide.Red => 1,
                        _ => -1
                    };
                    TileData tile = new TileData(OnChainBoardDataConverter.GetTopTile(move.tile));
                    int rotation = move.rotation;
                    int x = move.col + 1;
                    int y = move.row + 1;

                    tile.Rotate((rotation + 3) % 4);
                    Board.PlaceTile(tile, x, y, owner);
                }
                
                // set scores
            } 
            DojoGameManager.Instance.SessionManager.UpdateBoardAfterRoadContest();
            DojoGameManager.Instance.SessionManager.UpdateBoardAfterCityContest();
            gameUI.Initialize();
            sessionUI.Initialization();
            int cityScoreBlue = board.blue_score.Item1;
            int cartScoreBlue = board.blue_score.Item2;
            int cityScoreRed = board.red_score.Item1;
            int cartScoreRed = board.red_score.Item2;
            GameUI.Instance.SessionUI.SetCityScores(cityScoreBlue, cityScoreRed);
            GameUI.Instance.SessionUI.SetRoadScores(cartScoreBlue, cartScoreRed);
            GameUI.Instance.SessionUI.SetPlayerScores(cityScoreBlue + cartScoreBlue, cityScoreRed + cartScoreRed);
            gameUI.SessionUI.ShowPlayerJokerCount(LocalPlayer.LocalId);
            SetTilesInDeck(board.available_tiles_in_deck.Length);
            SetJokersCount(0, board.player1.Item3);
            SetJokersCount(1, board.player2.Item3);
            StartGame();
        }

        private void InitializePlayers()
        {
            Players = new Character[2];
            PlayersData = new PlayerData[2];

            
            Vector3[] leftCharacterPath = new Vector3[3];
            leftCharacterPath[0] = new Vector3(SpawnPoints[0].x, SpawnPoints[0].y + 15, 0); 
            leftCharacterPath[1] = new Vector3(SpawnPoints[0].x - 5, SpawnPoints[0].y + 7, 0); 
            leftCharacterPath[2] = SpawnPoints[0]; 

            
            Vector3[] rightCharacterPath = new Vector3[3];
            rightCharacterPath[0] = new Vector3(SpawnPoints[1].x, SpawnPoints[1].y + 15, 0);
            rightCharacterPath[1] = new Vector3(SpawnPoints[1].x + 5, SpawnPoints[1].y + 7, 0);
            rightCharacterPath[2] = SpawnPoints[1];

            
            
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;

            evolute_duel_Player hostData = DojoGameManager.Instance.GetPlayerData(board.player1.Item1.Hex());
            evolute_duel_Player guestData = DojoGameManager.Instance.GetPlayerData(board.player2.Item1.Hex()); 

            GameObject hostPrefab = PrefabsManager.Instance.GetPlayer(hostData.active_skin); 
            GameObject guestPrefab = PrefabsManager.Instance.GetPlayer(guestData.active_skin);
            GameObject hostObject = Instantiate(hostPrefab, Vector3.zero, Quaternion.identity);
            GameObject guestObject = Instantiate(guestPrefab, Vector3.zero, Quaternion.identity);
            
            Players[0] = hostObject.GetComponent<Character>();
            Players[1] = guestObject.GetComponent<Character>();
            
            Players[0].Initialize(board.player1.Item1, board.player1.Item2, board.player1.Item3);
            Players[1].Initialize(board.player2.Item1, board.player2.Item2, board.player2.Item3);
            
            PlayersData[0] = new PlayerData(hostData);
            PlayersData[1] = new PlayerData(guestData);
            
            
            CurrentTurnPlayer = Players[0];
            
            LocalPlayer = Players[0].Address.Hex() == DojoGameManager.Instance.LocalBurnerAccount.Address.Hex() 
                ? Players[0] : Players[1];
            RemotePlayer = LocalPlayer == Players[0] ? Players[1] : Players[0];
            Players[0].SetAnimatorController(sessionUI.charactersObject.GetAnimatorController(PlayersData[0].skin_id));
            Players[1].SetAnimatorController(sessionUI.charactersObject.GetAnimatorController(PlayersData[1].skin_id));

            int hostIndex = SetLocalPlayerData.GetLocalIndex(0);
            int guestPlayerIndex = SetLocalPlayerData.GetLocalIndex(1);
            Players[hostIndex].transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
            Players[hostIndex].transform.position = leftCharacterPath[0];
            Players[guestPlayerIndex].transform.position = rightCharacterPath[0];
            Players[hostIndex].transform
                .DOPath(leftCharacterPath, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            Players[guestPlayerIndex].transform
                .DOPath(rightCharacterPath, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);
            
            
        }

        public void StartGame()
        {
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            
            //TileSelector.OnTurnStarted.AddListener(OnTurnStarted);
            //TileSelector.OnTurnEnding.AddListener(OnTurnEnding);
            
            if (CurrentTurnPlayer == LocalPlayer)
            {
                Invoke(nameof(StartLocalTurn), 2f);
            }
            else
            {
                Invoke(nameof(StartRemoteTurn), 2f);
            }
            //UpdateTile();
            DojoGameManager.Instance.SessionManager.OnMoveReceived += HandleMove;
            DojoGameManager.Instance.SessionManager.OnSkipMoveReceived += SkipMove;
        }

        private void StartLocalTurn()
        {
            UpdateTile();
            LocalPlayer.StartSelecting();
            evolute_duel_Board board = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Board>().First().GetComponent<evolute_duel_Board>();
            Players[0].UpdateData(board.player1.Item3);
            Players[1].UpdateData(board.player2.Item3);
            //GameUI.Instance.SessionUI.UpdateJokerText(CurrentTurnPlayer.LocalId, Players[CurrentTurnPlayer.LocalId].JokerCount);
            //GameUI.Instance.SessionUI.UpdateDeckCount();
            
            
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
            UpdateTile();
            RemotePlayer.StartSelecting();
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
            Players[0].UpdateData(board.player1.Item3);
            Players[1].UpdateData(board.player2.Item3);
            //GameUI.Instance.SessionUI.UpdateJokerText(CurrentTurnPlayer.LocalId, Players[CurrentTurnPlayer.LocalId].JokerCount);
            //GameUI.Instance.SessionUI.UpdateDeckCount();
            
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.SetSkipTurnButtonActive(false);
            gameUI.SetActiveDeckContainer(false);
            //gameUI.SessionUI.ShowPlayerJokerCount(RemotePlayer.LocalId);
        }

        private void HandleMove(string playerAddress, TileData tile, Vector2Int position, int rotation, bool isJoker)
        {
            evolute_duel_Player player = DojoGameManager.Instance.GetPlayerData(playerAddress);
            if(playerAddress == Players[0].Address.Hex() &&  player != null)
            {
                PlayersData[0].UpdatePlayerData(player);
            }
            else if(playerAddress == Players[1].Address.Hex() &&  player != null)
            {
                PlayersData[1].UpdatePlayerData(player);
            }
            if (playerAddress == LocalPlayer.Address.Hex()) CompleteEndTurn(playerAddress);
            else StartCoroutine(HandleOpponentMoveCoroutine(playerAddress, tile, position, rotation, isJoker));
        }
        
        private void SkipMove(string playerAddress)
        {
            GameUI.Instance.SetJokerMode(false);
            TileSelector.EndTilePlacement();
            CurrentTurnPlayer.EndTurn();
            CompleteEndTurn(playerAddress);
        }

        private IEnumerator HandleOpponentMoveCoroutine(string playerAddress, TileData tile, Vector2Int position, int rotation, bool isJoker)
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
            yield return new WaitForSeconds(0.5f);
            CurrentTurnPlayer.EndTurn();
            yield return new WaitForSeconds(0.5f);
            TileSelector.tilePreview.ResetPosition();
            //CompleteEndTurn();
            CompleteEndTurn(playerAddress);
        }

        // private void UpdateTile()
        // {
        //     TileData currentTile = DojoGameManager.Instance.SessionManager.GetTopTile();
        //     currentTile.OwnerId = RemotePlayer.LocalId;
        //     TileSelector.SetCurrentTile(currentTile);
        // }

        private TileData _nextTile;
        public void UpdateTile()
        {
            _nextTile ??= DojoGameManager.Instance.SessionManager.GetTopTile();
            _nextTile.OwnerId = RemotePlayer.LocalId;
            TileSelector.SetCurrentTile(_nextTile);
            CustomLogger.LogImportant("UpdateTile. Tile: " + _nextTile.id);
        }

        public void SetNextTile(TileData tile)
        {
            CustomLogger.LogImportant("SetNextTile");
            _nextTile = tile;
        }

        public void SetTilesInDeck(int count)
        {
            gameUI.SessionUI.SetDeckCount(count);
        }
        
        public void SetJokersCount(int playerId, int count)
        {
            gameUI.SessionUI.SetJokersCount(playerId, count);
            gameUI.SessionUI.ShowPlayerJokerCount(LocalPlayer.LocalId);
        }
        
        

        private void OnTurnStarted()
        {
            
            if (IsLocalPlayerHost)
            {
                CurrentTurnPlayer.StartSelecting();
            }
            else
            {
                if (CurrentTurnPlayer == LocalPlayer)
                {
                    RemotePlayer.StartSelecting();
                }
                else
                {
                    LocalPlayer.StartSelecting();
                }
            }
        }

        private void OnTurnEnding()
        {
            if (IsLocalPlayerHost)
            {
                CurrentTurnPlayer.EndTurn();
            }
            else
            {
                if (CurrentTurnPlayer == LocalPlayer)
                {
                    RemotePlayer.EndTurn();
                }
                else
                {
                    LocalPlayer.EndTurn();
                }
            }

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
            TileSelector.ClearHighlights();
            TileSelector.tilePreview.ResetPosition();
            DojoGameManager.Instance.SessionManager.SkipMove();
        }

        public void CompleteEndTurn(string lastMovePlayerAddress)
        {
            bool isLocalPlayer = lastMovePlayerAddress == LocalPlayer.Address.Hex();
            
            if (isLocalPlayer)
            {
                CurrentTurnPlayer = RemotePlayer;
                Invoke(nameof(StartRemoteTurn), 1f);
            }
            else
            {
                CurrentTurnPlayer = LocalPlayer;
                Invoke(nameof(StartLocalTurn), 1f);
                gameUI.SetActiveDeckContainer(true);
            }
        }

        private void OnDestroy()
        {
            
            if (TileSelector != null)
            {
                //TileSelector.OnTurnStarted.RemoveListener(OnTurnStarted);
                //TileSelector.OnTurnEnding.RemoveListener(OnTurnEnding);
            }
            
            DojoGameManager.Instance.SessionManager.OnMoveReceived -= HandleMove;
            DojoGameManager.Instance.SessionManager.OnSkipMoveReceived -= SkipMove;
        }

        public int GetJokerCount(int playerId)
        {
            SetLocalPlayerData.GetLocalIndex(playerId);
            return Players[playerId].JokerCount;
        }

        public bool CanUseJoker()
        {
            int characterId = CurrentTurnPlayer == null ? 0 : CurrentTurnPlayer.LocalId;
            return !isJokerActive && Players[characterId].JokerCount > 0;
        }

        public void CompleteJokerPlacement()
        {
            isJokerActive = false;
            gameUI.SetJokerMode(false);
            gameUI.UpdateUI();
        }
        
        public void CancelJokerPlacement()
        {
            isJokerActive = false;
            gameUI.SetJokerMode(false);
            gameUI.UpdateUI();
            
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

            
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            backgroundTexture.Apply();
            style.normal.background = backgroundTexture;

            
            int padding = 10;
            int yPosition = 10;

            
            float screenCenterX = Screen.width / 2f;
            float labelWidth = 300f;
            float xPosition = screenCenterX - labelWidth / 2f;

            
            float time = Time.time;
            string turnInfo;
            string enemyNickname = IsLocalPlayerHost ? PlayersData[1].username : PlayersData[0].username;
            
            if (CurrentTurnPlayer != LocalPlayer)
            {
                turnInfo = $"Waiting for {enemyNickname} turn" + new string('.', (int)(time % 3) + 1);;
            }
            else
            {
                turnInfo = "Your turn now.";
            }
            GUI.Label(new Rect(xPosition, yPosition, labelWidth, 30), turnInfo, style);


            
            Destroy(backgroundTexture);
        }
    }
}