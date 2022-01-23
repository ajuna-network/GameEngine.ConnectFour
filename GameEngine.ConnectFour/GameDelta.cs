using System;
using System.Collections.Generic;

namespace GameEngine.ConnectFour
{
    public abstract class GameDelta
    {
        public byte GameState { get; }
        public byte CurrentPlayer { get; }

        public GameDelta(byte gameState, byte currentPlayer)
        {
            GameState = gameState;
            CurrentPlayer = currentPlayer;
        }

        public virtual byte[] Encode()
        {
            List<byte> result = new()
            {
                GameState,
                CurrentPlayer
            };
            return result.ToArray();
        }
    }

    public class GameDeltaInit : GameDelta
    {
        public byte PlayerCount { get; set; }
        public byte PlayerIdLen { get; set; }
        public List<byte[]> Players { get; set; }

        public GameDeltaInit(byte gameState, byte currentPlayer, List<byte[]> players) : base(gameState, currentPlayer)
        {
            Players = players;
            PlayerCount = (byte)players.Count;
            PlayerIdLen = (byte) players[0].Length;
        }

        public override byte[] Encode()
        {
            List<byte> result = new(base.Encode());
            result.Add(PlayerCount);
            result.Add(PlayerIdLen);
            Players.ForEach(p => result.AddRange(p));
            return result.ToArray();
        }

        public static GameDeltaInit Decode(byte[] encoded)
        {
            var playerCount = encoded[2];
            var playerIdLen = encoded[3];

            var players = new List<byte[]>();

            for (int i = 0; i < playerCount; i++)
            {
                var playerId = new byte[playerIdLen];
                Array.Copy(encoded, 4 + i * playerIdLen, playerId, 0, playerIdLen);
                players.Add(playerId);
            }


            return new GameDeltaInit(encoded[0], encoded[1], players);
        }
    }

    public class GameDeltaAction : GameDelta
    {
        public List<byte[]> Positions { get; set; }

        public GameDeltaAction(byte gameState, byte currentPlayer, List<byte[]> positions) : base(gameState, currentPlayer)
        {
            Positions = positions;
        }

        public override byte[] Encode()
        {
            List<byte> result = new(base.Encode());
            result.Add((byte)Positions.Count);
            if (Positions.Count > 0)
            {
                result.Add((byte)Positions[0].Length);
                Positions.ForEach(p => result.AddRange(p));
            }
            return result.ToArray();
        }

        public static GameDeltaAction Decode(byte[] encoded)
        {
            var positionCount = encoded[2];
            var positionLen = encoded[3];

            var positions = new List<byte[]>();

            for (int i = 0; i < positionCount; i++)
            {
                var position = new byte[positionLen];
                Array.Copy(encoded, 4 + i * positionLen, position, 0, positionLen);
                positions.Add(position);
            }

            return new GameDeltaAction(encoded[0], encoded[1], positions);
        }
    }
}
