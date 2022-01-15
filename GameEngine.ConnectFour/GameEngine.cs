using Ajuna.GenericGameEngine;
using Ajuna.GenericGameEngine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.ConnectFour
{

    public class GameDelta
    {
        public byte GameState { get; }
        public byte CurrentPlayer { get; }
        public byte PosX { get; set; }
        public byte PosY { get; set; }
        public byte Stone { get; set; }

        public GameDelta(byte gameState, byte currentPlayer)
        {
            GameState = gameState;
            CurrentPlayer = currentPlayer;
        }

        public byte[] Encode()
        {
            return new byte[5] { GameState, CurrentPlayer, PosX, PosY, Stone };
        }

        public static GameDelta Decode(byte[] encoded)
        {
            return new GameDelta(encoded[0], encoded[1]) {

                PosX = encoded[2],
                PosY = encoded[3],
                Stone = encoded[4]
            };
        }
    }

    public class GameEngine : GenericGameEngine
    {
        internal byte[,] board;

        internal byte currentPlayer;

        public GameEngine(byte[] gameId)
        {
            base.gameId = gameId;
         }

        public override byte[][] BlockTick()
        {
            return new byte[][] { };
        }

        public override byte[] ExecuteAction(byte[] gameId, byte[] player, byte[] action)
        {
            // use validate action first
            var validateMessage = IsValidAction(player, action);

            if ((MessageCode)validateMessage[0] != MessageCode.OK)
            {
                return validateMessage;
            }

            var oldBoard = (byte[,])board.Clone();

            var playerId = GetPlayerId(player);

            if (!Logic.AddStone(ref board, action[0], (byte)playerId))
            {
                return GetErrorCode(ErrorCode.BAD_ACTION);
            }

            // check for winner
            if (Logic.Evaluate(board, (byte)playerId))
            {
                gameState = GameState.FINISHED;
            } 
            else if (Logic.Full(board))
            {
                gameState = GameState.FINISHED;
            } 
            else
            {
                // Set next players turn
                SetNextPlayer();
            }

            GameDelta gameDelta = GetDelta(oldBoard, board);

            return GetStateDiff(StateDiffCode.ACTION, gameDelta.Encode());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public override byte[] GetState(byte[] gameId)
        {
            if (gameState == GameState.NONE)
            {
                return GetErrorCode(ErrorCode.WRONG_STATE);
            }

            var state = new byte[board.Length + 3];
            state[0] = (byte)MessageCode.FULL_STATE;
            state[1] = (byte) gameState;
            state[2] = currentPlayer;

            var boardArray = new byte[board.Length];

            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    boardArray[x * y + y] = board[x, y];
                }
            }

            Array.Copy(boardArray, 0, state, 3, board.Length);

            return state;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        public override byte[] NewInstance(byte[] gameId, List<byte[]> players)
        {
            if (gameState != GameState.NONE)
            {
                return GetErrorCode(ErrorCode.WRONG_STATE);
            }

            base.players = players;

            return InitializeGame(gameId, players);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override byte[] SyncronizeState(byte[] gameId, byte[] message)
        {
            if ((MessageCode)message[0] != MessageCode.STATE_DIFF)
            {
                return new byte[] { (byte)MessageCode.FAIL };
            }

            switch ((StateDiffCode)message[1])
            {
                case StateDiffCode.INIT:

                    board = new byte[7, 6] {
                     { 0, 0, 0, 0, 0, 0},
                     { 0, 0, 0, 0, 0, 0},
                     { 0, 0, 0, 0, 0, 0},
                     { 0, 0, 0, 0, 0, 0},
                     { 0, 0, 0, 0, 0, 0},
                     { 0, 0, 0, 0, 0, 0},
                     { 0, 0, 0, 0, 0, 0}
                    };

                    currentPlayer = message[2];
                    var playerCount = message[3];
                    var playerIdLen = message[4];

                    players = new List<byte[]>();

                    for (int i = 0; i < playerCount; i++)
                    {
                        var playerId = new byte[playerIdLen];
                        Array.Copy(message, 5 + i * playerIdLen, playerId, 0, playerIdLen);
                        players.Add(playerId);
                    }

                    gameState = GameState.INITIALIZED;
                    break;

                case StateDiffCode.RUNNING:
                    gameState = GameState.RUNNING;
                    break;

                case StateDiffCode.ACTION:

                    var gameDeltaArray = new byte[5];
                    Array.Copy(message, 2, gameDeltaArray, 0, 5);
                    var gameDelta = GameDelta.Decode(gameDeltaArray);

                    gameState = (GameState) gameDelta.GameState;
                    currentPlayer = gameDelta.CurrentPlayer;
                    board[gameDelta.PosX, gameDelta.PosY] = gameDelta.Stone;
                    break;
            }

            return new byte[] { (byte)MessageCode.OK };
        }

        /// <summary>
        /// Validate action.
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="player"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public override byte[] ValidateAction(byte[] gameId, byte[] player, byte[] action)
        {
            return IsValidAction(player, action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        public override byte[] InitializeGame(byte[] gameId, List<byte[]> players)
        {
            var playerCount = players.Count;
            var playerIdLen = players[0].Length;
            var playerStart = random.Next(playerCount) + 1;


            var stateDiffArray = new byte[playerCount * playerIdLen + 1 + 2];
            stateDiffArray[0] = (byte)playerStart; // starting player
            stateDiffArray[1] = (byte)playerCount; // count players playing
            stateDiffArray[2] = (byte)playerIdLen; // identification length

            for (int i = 0; i < playerCount; i++)
            {
                for (int j = 0; j < playerIdLen; j++)
                {
                    stateDiffArray[(i * playerIdLen) + j + 3] = players[i][j];
                }
            }

            var stateDiff = GenericGameEngine.GetStateDiff(StateDiffCode.INIT, stateDiffArray);

            // using syncronize to initialize, making sure we do the same as diff receivers.
            SyncronizeState(gameId, stateDiff);

            return stateDiff;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public override byte[] IsValidAction(byte[] player, byte[] action)
        {
            if (gameState != GameState.RUNNING)
            {
                return GetErrorCode(ErrorCode.WRONG_STATE);
            }

            var playerId = GetPlayerId(player);

            if (currentPlayer != playerId)
            {
                return GetErrorCode(ErrorCode.WRONG_PLAYER);
            }

            if (!Logic.CanAddStone(board, action[0]))
            {
                return GetErrorCode(ErrorCode.BAD_ACTION);
            }

            return new byte[] { (byte)MessageCode.OK };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        internal int GetPlayerId(byte[] player)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].SequenceEqual(player))
                {
                    return i + 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Set next player
        /// </summary>
        internal void SetNextPlayer()
        {
            currentPlayer = currentPlayer < players.Count ? (byte) (currentPlayer + 1) : (byte) 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldBoard"></param>
        /// <param name="board"></param>
        /// <returns></returns>
        private GameDelta GetDelta(byte[,] oldBoard, byte[,] board)
        {
            var gameDelta = new GameDelta((byte)gameState, currentPlayer);

            for (int y = 0; y < oldBoard.GetLength(1); y++)
            {
                for (int x = 0; x < oldBoard.GetLength(0); x++)
                {
                    if (oldBoard[x, y] != this.board[x, y])
                    {
                        gameDelta.PosX = (byte)x;
                        gameDelta.PosY = (byte)y;
                        gameDelta.Stone = this.board[x, y];
                        return gameDelta;
                    }
                }
            }

            return null;
        }

        public override bool Same(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            // compare specific game engine parameters
            var target = obj as GameEngine;
            
            if (!board.Cast<byte>().SequenceEqual(target.board.Cast<byte>())) {
                return false;
            }

            if (currentPlayer != target.currentPlayer)
            {
                return false;
            }

            return base.Same(obj);
        }
    }
}
