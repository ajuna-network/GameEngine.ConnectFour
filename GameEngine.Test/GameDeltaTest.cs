using Ajuna.GenericGameEngine;
using Ajuna.GenericGameEngine.Enums;
using GameEngine.ConnectFour;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Test
{

    public class GameDeltaTest
    {
        public byte[] GAME_ID = new byte[] { 1, 2, 3, 4 };

        public byte[] PLAYER_1 = new byte[] { 11, 12, 13, 14 };

        public byte[] PLAYER_2 = new byte[] { 21, 22, 23, 24 };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GameDeltaInitTest()
        {
            var players = new List<byte[]>() { PLAYER_1, PLAYER_2 };
            GameDeltaInit oldDelta = new(1, 2, players);
            var oldDeltaEncoded = oldDelta.Encode();
            var newDeltaEncoded = GameDeltaInit.Decode(oldDeltaEncoded).Encode();
            Assert.IsTrue(oldDeltaEncoded.SequenceEqual(newDeltaEncoded), "Encoding and decoding missmatch!");
        }

        [Test]
        public void GameDeltaActionTest()
        {
            GameDeltaAction oldDelta = new(1, 2) { 
                PosX = 3,
                PosY = 4,
                Stone = 2
            };
            var oldDeltaEncoded = oldDelta.Encode();
            var newDeltaEncoded = GameDeltaAction.Decode(oldDeltaEncoded).Encode();
            Assert.IsTrue(oldDeltaEncoded.SequenceEqual(newDeltaEncoded), "Encoding and decoding missmatch!");
        }

    }
}