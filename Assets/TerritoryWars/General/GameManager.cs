using System.Collections.Generic;
using TerritoryWars.Tile;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.General
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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

        private void Start()
        {
            StartNewTurn();
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
            StartNewTurn();
        }
    }
}