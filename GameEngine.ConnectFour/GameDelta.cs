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
}
