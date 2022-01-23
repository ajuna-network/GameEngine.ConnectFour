using Ajuna.GenericGameEngine;
using Ajuna.GenericGameEngine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.ConnectFour
{
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

            var newBoard = (byte[,])board.Clone();

            var playerId = GetPlayerId(player);

            if (!Logic.AddStone(ref newBoard, action[0], (byte)playerId))
            {
                return Message.Error(ErrorCode.BAD_ACTION);
            }

            // check for winner
            if (Logic.Evaluate(newBoard, (byte)playerId))
            {
                gameState = GameState.FINISHED;
            } 
            else if (Logic.Full(newBoard))
            {
                gameState = GameState.FINISHED;
            } 
            else
            {
                // Set next players turn
                SetNextPlayer();
            }

            GameDeltaAction deltaAction = GetDeltaAction(board, newBoard);

            return Message.StateDiff(StateDiffCode.ACTION, deltaAction.Encode());
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
                return Message.Error(ErrorCode.WRONG_STATE);
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
        public override byte[] NewInstance(byte[] gameId, byte gameEngineId, List<byte[]> players)
        {
            return InitializeGame(gameId, gameEngineId, players);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override byte[] SyncronizeState(byte[] gameId, byte gameEngineId, byte[] message)
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

                    var deltaInit = new byte[message.Length - 2];
                    Array.Copy(message, 2, deltaInit, 0, deltaInit.Length);

                    var gameDeltaInit = GameDeltaInit.Decode(deltaInit);

                    gameState = (GameState) gameDeltaInit.GameState;
                    currentPlayer = gameDeltaInit.CurrentPlayer;
                    players = gameDeltaInit.Players;
                    break;

                case StateDiffCode.RUNNING:
                    gameState = GameState.RUNNING;
                    break;

                case StateDiffCode.ACTION:

                    var deltaAction = new byte[message.Length - 2];
                    Array.Copy(message, 2, deltaAction, 0, deltaAction.Length);

                    var gameDeltaAction = GameDeltaAction.Decode(deltaAction);

                    gameState = (GameState)gameDeltaAction.GameState;
                    currentPlayer = gameDeltaAction.CurrentPlayer;

                    var positions = gameDeltaAction.Positions;
                    positions.ForEach(p => {
                        board[p[0], p[1]] = p[2];
                    });

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
        public override byte[] InitializeGame(byte[] gameId, byte gameEngineId, List<byte[]> players)
        {

            var playerStart = random.Next(players.Count()) + 1;

            GameDeltaInit gameDeltaInit = new((byte)GameState.INITIALIZED, (byte) playerStart, players);

            var stateDiff = Message.StateDiff(StateDiffCode.INIT, gameDeltaInit.Encode());

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
                return Message.Error(ErrorCode.WRONG_STATE);
            }

            var playerId = GetPlayerId(player);

            if (currentPlayer != playerId)
            {
                return Message.Error(ErrorCode.WRONG_PLAYER);
            }

            if (!Logic.CanAddStone(board, action[0]))
            {
                return Message.Error(ErrorCode.BAD_ACTION);
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
        /// <param name="newBoard"></param>
        /// <returns></returns>
        private GameDeltaAction GetDeltaAction(byte[,] oldBoard, byte[,] newBoard)
        {
            List<byte[]> positions = new();

            for (int y = 0; y < oldBoard.GetLength(1); y++)
            {
                for (int x = 0; x < oldBoard.GetLength(0); x++)
                {
                    if (oldBoard[x, y] != newBoard[x, y])
                    {
                        positions.Add(new byte[] { (byte)x , (byte)y, newBoard[x, y] });
                    }
                }
            }

            return new((byte)gameState, currentPlayer, positions);
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
