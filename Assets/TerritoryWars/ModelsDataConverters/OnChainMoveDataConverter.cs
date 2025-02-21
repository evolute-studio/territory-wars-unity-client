using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.ModelsDataConverters
{
    public static class OnChainMoveDataConverter
    {
        public static (PlayerSide, TileData, int, Vector2Int, bool) GetMoveData(evolute_duel_Move moveModel)
        {
            PlayerSide Side = moveModel.player_side;
            TileData Tile = moveModel.tile is Option<byte>.Some some ? new TileData(OnChainBoardDataConverter.TileTypes[some.value]) : null;
            Vector2Int Position = new Vector2Int(moveModel.col, moveModel.row);
            int Rotation = moveModel.rotation;
            bool IsJoker = moveModel.is_joker;
        
            return (Side, Tile, Rotation, Position, IsJoker);
        }
    }
}