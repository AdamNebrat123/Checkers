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
        public static int[,] ConvertBoardToState(Board board)
        {
            int[,] state = new int[Board.Size, Board.Size];

            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    var piece = board.Squares[row, col].Piece;

                    if (piece == null)
                        state[row, col] = 0;
                    else if (piece.Color == PieceColor.White)
                        state[row, col] = piece is King ? 3 : 1;
                    else
                        state[row, col] = piece is King ? 4 : 2;
                }
            }

            return state;
        }

        public static void ConvertStateToBoard(int[,] state, Board board)
        {
            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    switch (state[row, col])
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

    }
}
