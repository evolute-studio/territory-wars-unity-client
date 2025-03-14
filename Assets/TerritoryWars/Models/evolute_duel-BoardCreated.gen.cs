// Generated by dojo-bindgen on Mon, 24 Feb 2025 09:31:19 +0000. Do not modify this file manually.
using System;
using Dojo;
using Dojo.Starknet;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Enum = Dojo.Starknet.Enum;
using BigInteger = System.Numerics.BigInteger;

// Type definition for `core::option::Option::<core::integer::u32>` enum

// Model definition for `evolute_duel::eventsModels::EBoardCreated` model
public class evolute_duel_BoardCreated : ModelInstance {
    [ModelField("board_id")]
        public FieldElement board_id;

        [ModelField("initial_edge_state")]
        public byte[] initial_edge_state;

        [ModelField("top_tile")]
        public Option<byte> top_tile;

        [ModelField("state")]
        public (byte, byte, byte)[] state;

        [ModelField("player1")]
        public (FieldElement, PlayerSide, byte) player1;

        [ModelField("player2")]
        public (FieldElement, PlayerSide, byte) player2;

        [ModelField("last_move_id")]
        public Option<FieldElement> last_move_id;
        
        [ModelField("blue_score")]
        public (ushort, ushort) blue_score;
        
        [ModelField("red_score")]
        public (ushort, ushort) red_score;

        [ModelField("game_state")]
        public GameState game_state;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }
}

        