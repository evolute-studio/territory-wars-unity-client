using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.Tile;
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
        [SerializeField] private DeckManager deckManager;
        public TileSelector TileSelector;

        public Vector3[] SpawnPoints;

        public Character[] Characters;
        private Character _currentCharacter;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            Characters = new Character[2];
            Vector3 spawnPosition = new Vector3(SpawnPoints[0].x, SpawnPoints[0].y + 15, 0);
            GameObject player1 = Instantiate(PrefabsManager.Instance.GetNextPlayer(), spawnPosition, Quaternion.identity);
            spawnPosition = new Vector3(SpawnPoints[1].x, SpawnPoints[1].y + 15, 0);
            GameObject player2 = Instantiate(PrefabsManager.Instance.GetNextPlayer(), spawnPosition, Quaternion.identity);
            Characters[0] = player1.GetComponent<Character>();
            Characters[1] = player2.GetComponent<Character>();
            Characters[0].transform.localScale = new Vector3(-1, 1, 1);
            _currentCharacter = Characters[0];

            // анімація спуску персонажів до SpawnPoints
            Characters[0].transform.DOMove(SpawnPoints[0], 1f);
            Characters[1].transform.DOMove(SpawnPoints[1], 1f);

            // Підписуємось на події ходу
            TileSelector.OnTurnStarted.AddListener(OnTurnStarted);
            TileSelector.OnTurnEnding.AddListener(OnTurnEnding);
            
            Invoke(nameof(StartNewTurn), 2f);
        }

        private void OnTurnStarted()
        {
            // Активуємо поточного персонажа
            _currentCharacter.StartSelecting();

        }

        private void OnTurnEnding()
        {
            // Деактивуємо поточного персонажа
            _currentCharacter.EndTurn();

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
            _currentCharacter = _currentCharacter == Characters[0] ? Characters[1] : Characters[0];

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
    }
}