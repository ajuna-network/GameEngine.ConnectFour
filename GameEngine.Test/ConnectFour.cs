using Ajuna.GenericGameEngine;
using Ajuna.GenericGameEngine.Enums;
using GameEngine.ConnectFour;
using NUnit.Framework;
using System.Collections.Generic;

namespace GameEngine.Test
{

    public class ConnectFourTest
    {
        public byte GAME_ENGINE_ID = 0x00;

        public byte[] GAME_ID = new byte[] { 1, 2, 3, 4 };

        public byte[] PLAYER_1 = new byte[] { 1 };

        public byte[] PLAYER_2 = new byte[] { 2 };

        public byte[] DELTA_RUN = new GameDeltaRunning((byte)GameState.INITIALIZED).Encode();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestRandomSeed()
        {
            var _gameEngine1 = new ConnectFour.GameEngine(GAME_ID);
            // set random seed for deterministic results
            _gameEngine1.SetRandomSeed(new byte[] { 0, 0, 0, 1 });
            var diff1 = _gameEngine1.NewInstance(GAME_ID, GAME_ENGINE_ID, new List<byte[]> { PLAYER_1, PLAYER_2 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff1);

            // launch game
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, Message.StateDiff(StateDiffCode.RUNNING, DELTA_RUN));

            Assert.AreEqual(MessageCode.OK, (MessageCode)_gameEngine1.ValidateAction(GAME_ID, PLAYER_1, new byte[] { 0 })[0], "correct starting player");
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)_gameEngine1.ValidateAction(GAME_ID, PLAYER_2, new byte[] { 0 })[0], "wrong starting player");

            var _gameEngine2 = new ConnectFour.GameEngine(GAME_ID);
            // set random seed for deterministic results
            _gameEngine2.SetRandomSeed(new byte[] { 0, 0, 0, 0 });
            var diff2 = _gameEngine2.NewInstance(GAME_ID, GAME_ENGINE_ID, new List<byte[]> { PLAYER_1, PLAYER_2 });
            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff2);

            // launch game
            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, Message.StateDiff(StateDiffCode.RUNNING, DELTA_RUN));

            Assert.AreEqual(MessageCode.ERROR, (MessageCode)_gameEngine2.ValidateAction(GAME_ID, PLAYER_1, new byte[] { 0 })[0], "wrong starting player");
            Assert.AreEqual(MessageCode.OK, (MessageCode)_gameEngine2.ValidateAction(GAME_ID, PLAYER_2, new byte[] { 0 })[0], "correct starting player");
        }

        [Test]
        public void CreationAndEqualsTest()
        {
            var _gameEngine1 = new ConnectFour.GameEngine(GAME_ID);

            Assert.AreEqual(GameState.NONE, _gameEngine1.GameState);
            Assert.AreEqual(GAME_ID, _gameEngine1.GameId);

            var message = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_1, new byte[] { 0 });

            Assert.AreEqual(MessageCode.ERROR, (MessageCode)message[0]);
            Assert.AreEqual(ErrorCode.WRONG_STATE, (ErrorCode)message[1]);

            // set random seed for deterministic results
            _gameEngine1.SetRandomSeed(new byte[] { 0, 0, 0, 1 });

            var diff1 = _gameEngine1.NewInstance(GAME_ID, GAME_ENGINE_ID, new List<byte[]> { PLAYER_1, PLAYER_2 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff1);

            Assert.AreEqual(GameState.INITIALIZED, _gameEngine1.GameState);

            var _gameEngine2 = new ConnectFour.GameEngine(GAME_ID);
            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff1);

            Assert.AreEqual(GameState.INITIALIZED, _gameEngine2.GameState);

            Assert.True(_gameEngine1.Same(_gameEngine2));

            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, Message.StateDiff(StateDiffCode.RUNNING, DELTA_RUN));
            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, Message.StateDiff(StateDiffCode.RUNNING, DELTA_RUN));

            Assert.AreEqual(GameState.RUNNING, _gameEngine2.GameState);

            Assert.True(_gameEngine1.Same(_gameEngine2));

            // valid move
            var valide_flag1 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_1, new byte[] { 0 });
            Assert.AreEqual(MessageCode.OK, (MessageCode)valide_flag1[0]);

            // bad move
            var valide_flag2 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_1, new byte[] { 7 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)valide_flag2[0]);

            // wrong player
            var valide_flag3 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_2, new byte[] { 0 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)valide_flag3[0]);

        }

        [Test]
        public void FullGamePlay()
        {
            var _gameEngine1 = new ConnectFour.GameEngine(GAME_ID);
            var _gameEngine2 = new ConnectFour.GameEngine(GAME_ID);

            Assert.AreEqual(GameState.NONE, _gameEngine1.GameState);
            Assert.AreEqual(GAME_ID, _gameEngine1.GameId);

            // set random seed for deterministic results
            _gameEngine1.SetRandomSeed(new byte[] { 0, 0, 0, 1 });

            var diff1 = _gameEngine1.NewInstance(GAME_ID, GAME_ENGINE_ID, new List<byte[]> { PLAYER_1, PLAYER_2 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff1);
            Assert.AreEqual(GameState.INITIALIZED, _gameEngine1.GameState, "Not correct state after, new instances!");

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff1);
            Assert.AreEqual(GameState.INITIALIZED, _gameEngine2.GameState,"Not same state after syncronizing, second node!");

            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            var diff_running = Message.StateDiff(StateDiffCode.RUNNING, Message.StateDiff(StateDiffCode.RUNNING, DELTA_RUN));

            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff_running);
            Assert.AreEqual(GameState.RUNNING, _gameEngine1.GameState);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff_running);
            Assert.AreEqual(GameState.RUNNING, _gameEngine2.GameState);

            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // get final state
            var full_state1 = _gameEngine1.GetState(GAME_ID);
            Assert.AreEqual(MessageCode.FULL_STATE, (MessageCode)full_state1[0]);
            Assert.AreEqual(GameState.RUNNING, (GameState)full_state1[1]);

            // validated moves

            // valid move
            var valide_flag1 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_1, new byte[] { 0 });
            Assert.AreEqual(MessageCode.OK, (MessageCode)valide_flag1[0]);

            // bad move
            var valide_flag2 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_1, new byte[] { 7 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)valide_flag2[0]);

            // wrong player
            var valide_flag3 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_2, new byte[] { 0 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)valide_flag3[0]);

            // execute actions

            // bad move
            var exec_move1 = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_1, new byte[] { 7 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)exec_move1[0]);

            // wrong player
            var exec_move2 = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_2, new byte[] { 0 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)exec_move2[0]);

            // player 1 move 1
            var diff = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_1, new byte[] { 0 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.AreEqual(MessageCode.STATE_DIFF, (MessageCode)diff[0]);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // player 2 move 1
            diff = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_2, new byte[] { 1 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.AreEqual(MessageCode.STATE_DIFF, (MessageCode)diff[0]);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // player 1 move 2
            diff = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_1, new byte[] { 0 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.AreEqual(MessageCode.STATE_DIFF, (MessageCode)diff[0]);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // player 2 move 2
            diff = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_2, new byte[] { 1 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.AreEqual(MessageCode.STATE_DIFF, (MessageCode)diff[0]);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // player 1 move 3
            diff = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_1, new byte[] { 0 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.AreEqual(MessageCode.STATE_DIFF, (MessageCode)diff[0]);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // player 2 move 3
            diff = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_2, new byte[] { 1 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.AreEqual(MessageCode.STATE_DIFF, (MessageCode)diff[0]);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // player 1 move 4
            diff = _gameEngine1.ExecuteAction(GAME_ID, PLAYER_1, new byte[] { 0 });
            _gameEngine1.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.AreEqual(MessageCode.STATE_DIFF, (MessageCode)diff[0]);

            _gameEngine2.SyncronizeState(GAME_ID, GAME_ENGINE_ID, diff);
            Assert.True(_gameEngine1.Same(_gameEngine2), "Check of having both nodes equal failed.");

            // valid move
            var valide_flag4 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_2, new byte[] { 0 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)valide_flag4[0]);

            // valid move
            var valide_flag5 = _gameEngine1.ValidateAction(GAME_ID, PLAYER_1, new byte[] { 0 });
            Assert.AreEqual(MessageCode.ERROR, (MessageCode)valide_flag5[0]);

            // get final state
            var full_state2 = _gameEngine1.GetState(GAME_ID);
            Assert.AreEqual(MessageCode.FULL_STATE, (MessageCode)full_state2[0]);
            Assert.AreEqual(GameState.FINISHED, (GameState)full_state2[1]);

            // get final state
            var full_state3 = _gameEngine2.GetState(GAME_ID);
            Assert.AreEqual(MessageCode.FULL_STATE, (MessageCode)full_state3[0]);
            Assert.AreEqual(GameState.FINISHED, (GameState)full_state3[1]);

        }
    }
}