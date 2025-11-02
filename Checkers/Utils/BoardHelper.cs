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
        public static int[][] ConvertBoardToState(Board board)
        {
            int[][] state = new int[Board.Size][];
            for (int row = 0; row < Board.Size; row++)
            {
                state[row] = new int[Board.Size];
                for (int col = 0; col < Board.Size; col++)
                {
                    var piece = board.Squares[row, col].Piece;

                    if (piece == null)
                        state[row][col] = 0;
                    else if (piece.Color == PieceColor.White)
                        state[row][col] = piece is King ? 3 : 1;
                    else
                        state[row][col] = piece is King ? 4 : 2;
                }
            }

            return state;
        }

        public static void ConvertStateToBoard(int[][] state, Board board)
        {
            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    switch (state[row][col])
                    {
                        case 0:
                            board.Squares[row, col].Piece = null;
                            break;
                        case 1:
                            board.Squares[row, col].Piece = new Man(PieceColor.White);
                            break;
                        case 2:
                            board.Squares[row, col].Piece = new Man(PieceColor.Black);
                            break;
                        case 3:
                            board.Squares[row, col].Piece = new King(PieceColor.White);
                            break;
                        case 4:
                            board.Squares[row, col].Piece = new King(PieceColor.Black);
                            break;
                    }
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
