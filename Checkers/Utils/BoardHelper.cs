using Checkers.Model;
using Checkers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Utils
{
    public static class BoardHelper
    {
        public static int[][] ConvertBoardToState(Board board, bool isWhitePerspective)
        {
            if (isWhitePerspective)
                return ConvertBoardToStateWhitePerspective(board);
            return ConvertBoardToStateBlackPerspective(board);
        }

        public static int[][] ConvertBoardToStateWhitePerspective(Board board)
        {
            int[][] state = new int[Board.Size][];
            for (int row = 0; row < Board.Size; row++)
            {
                state[row] = new int[Board.Size];
                for (int col = 0; col < Board.Size; col++)
                {
                    var piece = board.Squares[row, col].Piece;
                    state[row][col] = piece switch
                    {
                        null => 0,
                        Man m when m.Color == PieceColor.White => 1,
                        Man m when m.Color == PieceColor.Black => 2,
                        King k when k.Color == PieceColor.White => 3,
                        King k when k.Color == PieceColor.Black => 4,
                        _ => 0
                    };
                }
            }
            return state;
        }

        public static int[][] ConvertBoardToStateBlackPerspective(Board board)
        {
            int[][] state = new int[Board.Size][];
            for (int row = 0; row < Board.Size; row++)
            {
                state[row] = new int[Board.Size];
                for (int col = 0; col < Board.Size; col++)
                {
                    // הפיכה של השורה והעמודה
                    int targetRow = Board.Size - 1 - row;
                    int targetCol = Board.Size - 1 - col;

                    var piece = board.Squares[targetRow, targetCol].Piece;
                    state[row][col] = piece switch
                    {
                        null => 0,
                        Man m when m.Color == PieceColor.White => 1,
                        Man m when m.Color == PieceColor.Black => 2,
                        King k when k.Color == PieceColor.White => 3,
                        King k when k.Color == PieceColor.Black => 4,
                        _ => 0
                    };
                }
            }
            return state;
        }

        public static void ConvertStateToBoard(int[][] state, Board board, bool isWhitePerspective)
        {
            bool invert = !isWhitePerspective;

            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    int targetRow = invert ? Board.Size - 1 - row : row;
                    int targetCol = invert ? Board.Size - 1 - col : col;

                    board.Squares[targetRow, targetCol].Piece = state[row][col] switch
                    {
                        0 => null,
                        1 => new Man(PieceColor.White),
                        2 => new Man(PieceColor.Black),
                        3 => new King(PieceColor.White),
                        4 => new King(PieceColor.Black),
                        _ => null
                    };
                }
            }
        }

        public static int[][] InitialBoardState()
        {
            int size = Board.Size;
            int[][] state = new int[size][];
            for (int row = 0; row < size; row++)
            {
                state[row] = new int[size];
                for (int col = 0; col < size; col++)
                {
                    if ((row + col) % 2 == 1) // only dark squares
                    {
                        if (row < 3) // top 3 rows -> Black pieces
                            state[row][col] = 2;
                        else if (row > size - 4) // bottom 3 rows -> White pieces
                            state[row][col] = 1;
                        else
                            state[row][col] = 0; // empty middle rows
                    }
                    else
                    {
                        state[row][col] = 0; // light squares are always empty
                    }
                }
            }
            return state;
        }


        // אתחול הלוח
        public static int[][] InitializeEmptyBoard()
        {
            var board = new int[8][];
            for (int i = 0; i < 8; i++)
                board[i] = new int[8]; // כל שורה באורך 8
            return board;
        }

    }
}
