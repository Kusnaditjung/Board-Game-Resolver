using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameResolver
{
    internal class Game
    {
        private const int boardSize = 5;                
        public ConcurrentDictionary<int, List<List<string>>> Solutions { get; private set; }
        public bool[,] InitialBoard { get; private set; }
        public Game(bool[,] board)
        {
            InitialBoard = board;
        }

        public Game Solve()
        {
            Solutions = new ConcurrentDictionary<int, List<List<string>>>();
            MakePossibleMovement(InitialBoard, new List<string>());
            return this;
        }

        //Play always start from initial board to the last move and return the last state of the board
        public static bool[,] Play(bool[,] board, IEnumerable<string> moves, int lastMoveIndex)
        {
            ValidateArgument(board, moves, lastMoveIndex);

            return PlayBoard(board, moves.ToList(), lastMoveIndex);
        }

        private static bool[,] PlayBoard(bool[,] board, List<string> moves, int lastMoveIndex)
        {
            bool[,] playBoard = (bool[,])board.Clone();

            for (int index = 0; index <= lastMoveIndex; index++)
            {
                if (moves[index] == "Start")
                    continue;

                PlaySingleMove(playBoard, moves[index]);
            }

            return playBoard;
        }

        private static void ValidateArgument(bool[,] board, IEnumerable<string> moves, int lastMoveIndex)
        {
            if (board == null)
                throw new ArgumentNullException("board");

            if (moves == null)
                throw new ArgumentNullException("moves");

            if (lastMoveIndex >= moves.Count())
                throw new ArgumentOutOfRangeException("lastMoveIndex");
        }

        private void MakePossibleMovement(bool[,] board, List<string> moves)
        {
            List<string> possibleMoves = GetPossibleMoves(board).ToList();

            if (possibleMoves.Any())
            {
                ProcessPossibleMovesInParalel(board, moves, possibleMoves);
            }
            else
            {
                PopulateSolutions(board, moves);
            }
        }

        private void PopulateSolutions(bool[,] board, List<string> moves)
        {
            int leftCount = PieceLeft(board);
            
            if (Solutions.ContainsKey(leftCount))
            {
                Solutions[leftCount].Add(moves);
            }
            else
            {
                Solutions.TryAdd(leftCount, new List<List<string>>() { moves });
            }
        }

        private void ProcessPossibleMovesInParalel(bool[,] board, List<string> moves, List<string> possibleMoves)
        {
            Parallel.ForEach(
                possibleMoves,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (move) =>
                {
                    bool[,] cloneBoard = (bool[,])board.Clone();
                    PlaySingleMove(cloneBoard, move);
                    List<string> newMoves = moves.ToList();
                    newMoves.Add(move);
                    MakePossibleMovement(cloneBoard, newMoves);
                });
        }

        private IEnumerable<string> GetPossibleMoves(bool[,] board)
        {
            foreach (string destination in GetDestination(board))
            {
                foreach (string source in GetSourceForSinglDestination(board, destination))
                {
                    yield return source + "-" + destination;
                }
            }
        }

        private IEnumerable<string> GetSourceForSinglDestination(bool[,] board, string destination)
        {
            int yDest = int.Parse(destination.Substring(0, 1));
            int xDest = int.Parse(destination.Substring(1, 1));

            //horizontal move to right
            if (CanMoveToRight(xDest))
            {
                if (board[yDest, xDest - 2] && board[yDest, xDest - 1])
                    yield return yDest.ToString() + (xDest - 2).ToString();
            }

            //horizontal move to left
            if (CanMoveToLeft(yDest, xDest))
            {
                if (board[yDest, xDest + 2] && board[yDest, xDest + 1])
                    yield return yDest.ToString() + (xDest + 2).ToString();
            }

            //vertical move to down
            if (CanMoveToDown(yDest))
            {
                if (board[yDest - 2, xDest] && board[yDest - 1, xDest])
                    yield return (yDest - 2).ToString() + xDest.ToString();
            }

            //vertical move to up
            if (CanMoveToUp(yDest, xDest))
            {
                if (board[yDest + 2, xDest] && board[yDest + 1, xDest])
                    yield return (yDest + 2).ToString() + xDest.ToString();
            }

            //diagonal move, move down left
            if (CanMoveDownLeft(yDest, xDest))
            {
                if (board[yDest - 2, xDest + 2] && board[yDest - 1, xDest + 1])
                    yield return (yDest - 2).ToString() + (xDest + 2).ToString();
            }

            //diagnoal move, move up right
            if (CanMoveUpRight(yDest, xDest))
            {
                if (board[yDest + 2, xDest - 2] && board[yDest + 1, xDest - 1])
                    yield return (yDest + 2).ToString() + (xDest - 2).ToString();
            }
        }

        private bool CanMoveUpRight(int yDest, int xDest)
        {
            return xDest - 2 >= 0 && (yDest + 2 <= MaxIndex(xDest - 2));
        }

        private bool CanMoveDownLeft(int yDest, int xDest)
        {
            return yDest - 2 >= 0 && (xDest + 2 <= MaxIndex(yDest - 2));
        }

        private bool CanMoveToUp(int yDest, int xDest)
        {
            return yDest + 2 <= MaxIndex(xDest);
        }

        private static bool CanMoveToRight(int xDest)
        {
            return xDest - 2 >= 0;
        }

        private static bool CanMoveToDown(int yDest)
        {
            return yDest - 2 >= 0;
        }

        private bool CanMoveToLeft(int yDest, int xDest)
        {
            return xDest + 2 <= MaxIndex(yDest);
        }

        private int MaxIndex(int position)
        {
            return (boardSize - 1) - position;
        }

        private IEnumerable<string> GetDestination(bool[,] board)
        {
            for (int row = 0; row < boardSize; row++)
            {
                int rowColumnCount = boardSize - row;
                for (int column = 0; column < rowColumnCount; column++)
                {
                    if (board[row, column] == false)
                    {
                        yield return row.ToString() + column.ToString();
                    }
                }
            }
        }

        private int PieceLeft(bool[,] board)
        {
            int count = 0;

            for (int row = 0; row < boardSize; row++)
            {
                int rowColumnCount = boardSize - row;
                for (int column = 0; column < rowColumnCount; column++)
                {
                    if (board[row, column] == true)
                    {
                        count++;
                    }
                }
            }

            return count;
        }        

        private static void PlaySingleMove(bool[,] playBoard, string move)
        {
            string[] moveArray = move.Split(new[] { "-" }, StringSplitOptions.None);

            string startPosition = moveArray[0];
            string endPosition = moveArray[1];
            string skipPosition = GetSkipCell(startPosition, endPosition);

            EmptyCell(playBoard, startPosition);
            EmptyCell(playBoard, skipPosition);
            FillCell(playBoard, endPosition);
        }

        private static string GetSkipCell(string startPosition, string endPosition)
        {
            int row = (int.Parse(startPosition.Substring(0, 1)) + int.Parse(endPosition.Substring(0, 1))) / 2;
            int column = (int.Parse(startPosition.Substring(1, 1)) + int.Parse(endPosition.Substring(1, 1))) / 2;

            return row.ToString() + column.ToString();
        }

        private static void EmptyCell(bool[,] board, string position)
        {
            int row = int.Parse(position.Substring(0, 1));
            int column = int.Parse(position.Substring(1, 1));

            bool isPiecePresent = board[row, column];

            if (!isPiecePresent)
                throw new InvalidOperationException(string.Format("The cell {0} is currently empty, cannot be emptied", position));

            board[row, column] = false;
        }

        private static void FillCell(bool[,] board, string position)
        {
            int row = int.Parse(position.Substring(0, 1));
            int column = int.Parse(position.Substring(1, 1));
            bool isPiecePresent = board[row, column];

            if (isPiecePresent)
                throw new InvalidOperationException(string.Format("The cell {0} is currently filled, cannot be filled", position));

            board[row, column] = true;
        }         
    }
}
