using Checkers.Model;
using Checkers.Utils;
using Checkers.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.MoveHistory
{
    public class BoardSnapshotHistory
    {
        private readonly BoardViewModel boardVM;
        private readonly bool isWhitePerspective;

        private readonly List<int[][]> states = new();
        private int currentIndex = -1;

        public bool CanGoBack => CurrentIndex > 0;
        public bool CanGoForward => CurrentIndex < states.Count - 1;

        public int CurrentIndex { 
            get => currentIndex;
            set{
                if (value < 0 )
                    currentIndex = 0;
                else currentIndex = value;
            }
        }

        public BoardSnapshotHistory(BoardViewModel boardVM, bool isWhitePerspective)
        {
            this.boardVM = boardVM;
            this.isWhitePerspective = isWhitePerspective;

            this.states.Add(BoardHelper.InitialBoardState());
        }

        public void AddState(int[][] boardState)
        {
            states.Add(Clone(boardState));
            CurrentIndex = states.Count - 1;
        }

        public void GoBack()
        {
            if (!CanGoBack) return;
            CurrentIndex--;
            ApplyCurrentState();
        }

        public void GoForward()
        {
            if (!CanGoForward) return;
            CurrentIndex++;
            ApplyCurrentState();
        }

        private void ApplyCurrentState()
        {
            if (!states.Any())
                return;

            var state = states[CurrentIndex];

            boardVM.Board = BoardHelper.ConvertStateToBoard(state, isWhitePerspective);

            // סנכרון UI
            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    int index = row * Board.Size + col;
                    var squareVM = boardVM.Squares[index];

                    squareVM.Piece = boardVM.Board.Squares[row, col].Piece;
                }
            }
        }

        private static int[][] Clone(int[][] original)
        {
            return original.Select(row => row.ToArray()).ToArray();
        }

        public void ResetBoardToMostUpdatedSnapshot()
        {
            if (!states.Any())
                return;

            // Reset Board To Most Updated Snapshot
            CurrentIndex = states.Count - 1;
            ApplyCurrentState();
        }
    }

}
