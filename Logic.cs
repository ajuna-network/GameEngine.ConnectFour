using System;

namespace ConnectFourEngine
{
    public static class Logic
    {
        public static bool Full(byte[,] board)
        {
            var yPos = board.GetLength(1) - 1;
            for (int xPos = 0; xPos < board.GetLength(0); xPos++)
            {
                if (board[yPos, xPos] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Evaluate(byte[,] board, byte player)
        {

            // horizontalCheck 
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int x = 0; x < board.GetLength(0) - 3; x++)
                {
                    if (board[x, y] == player
                    && board[x + 1, y] == player
                    && board[x + 2, y] == player
                    && board[x + 3, y] == player)
                    {
                        return true;
                    }
                }
            }

            // verticalCheck
            for (int y = 0; y < board.GetLength(1) - 3; y++)
            {
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    if (board[x, y] == player
                    && board[x, y + 1] == player
                    && board[x, y + 2] == player
                    && board[x, y + 3] == player)
                    {
                        return true;
                    }
                }
            }

            // ascendingDiagonalCheck 
            for (int y = 0; y < board.GetLength(1) - 3; y++)
            {
                for (int x = 3; x < board.GetLength(0); x++)
                {
                    if (board[x,y] == player
                    && board[x - 1, y + 1] == player
                    && board[x - 2, y + 2] == player
                    && board[x - 3, y + 3] == player)
                    {
                        return true;
                    }
                }
            }

            // descendingDiagonalCheck 
            for (int y = 3; y < board.GetLength(1); y++)
            {
                for (int x = 3; x < board.GetLength(0); x++)
                {
                    if (board[x,y] == player
                    && board[x - 1,y - 1] == player
                    && board[x - 2,y - 2] == player
                    && board[x - 3,y - 3] == player)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AddStone(ref byte[,] board, byte column, byte player)
        {
            if (!CanAddStone(board, column))
            {
                return false;
            }

            var boardRows = board.GetLength(1);

            for (int y = 0; y < boardRows; y++)
            {
                var yPos = boardRows - y - 1;

                if (board[column, yPos] > 0)
                {
                    continue;
                }

                board[column, yPos] = player;
                break;
            }

            return true;
        }

        public static bool CanAddStone(byte[,] board, byte column)
        {
            if (board.GetLength(0) <= column)
            {
                return false;
            }

            if (board[column, 0] > 0)
            {
                return false;
            }

            return true;
        }
    }

}