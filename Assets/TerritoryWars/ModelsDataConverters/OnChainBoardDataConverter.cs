using System;

namespace TerritoryWars.ModelsDataConverters
{
    public static class OnChainBoardDataConverter
    {
        public static char[] EdgeTypes = { 'C', 'R', 'M', 'F' };

        public static string[] TileTypes =
        {
            "CCCC", "FFFF", "RRRR", "CCCF", "CCCR", "CCRR", "CFFF", "FFFR", "CRRR", "FRRR",
            "CCFF", "CFCF", "CRCR", "FFRR", "FRFR", "CCFR", "CCRF", "CFCR", "CFFR", "CFRF",
            "CRFF", "CRRF", "CRFR", "CFRR"
        };
    
        public static char[] GetInitialEdgeState(byte[] initial_edge_state)
        {
            char[] charArray = Array.ConvertAll(initial_edge_state, c => EdgeTypes[(int)c]);
            return charArray;
        }
    
        public static string GetTopTile(Option<byte> top_tile)
        {
            if (top_tile is Option<byte>.Some some)
            {
                return TileTypes[some.value];
            }
            return "None";
        }
    }
}