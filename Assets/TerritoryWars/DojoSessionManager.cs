using Dojo;
using Dojo.Starknet;
using UnityEngine;

namespace TerritoryWars
{
    public class DojoSessionManager
    {
        private DojoGameManager _dojoGameManager;
        
        public evolute_duel_Board LocalPlayerBoard => GetLocalPlayerBoard();

        public DojoSessionManager(DojoGameManager dojoGameManager)
        {
            Debug.Log("DojoSessionManager initialized. Session started");
            _dojoGameManager = dojoGameManager;
            _dojoGameManager.WorldManager.synchronizationMaster.OnModelUpdated.AddListener(ModelUpdated);
        }

        public void ModelUpdated(ModelInstance modelInstance)
        {
            
        }

        public void CheckBoardUpdate(ModelInstance modelInstance)
        {
            if (!modelInstance.transform.TryGetComponent(out evolute_duel_Board board)) return;    
        }
        
        public evolute_duel_Board GetLocalPlayerBoard()
        {
            return GetBoard(_dojoGameManager.LocalBurnerAccount.Address);
        }

        private evolute_duel_Board GetBoard(FieldElement player)
        {
            GameObject[] boardsGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Board>();
            foreach (var boardGO in boardsGO)
            {
                if (boardGO.TryGetComponent(out evolute_duel_Board board))
                {
                    //public (FieldElement, PlayerSide, byte, bool) player1;
                    if (board.player1.Item1.Hex() == player.Hex() || board.player2.Item1.Hex() == player.Hex())
                    {
                        return board;
                    }
                }
            }
            return null;
        }

    }
}