using Checkers.Models;
using Checkers.MoveHistory;
using Checkers.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public class ReplayGameStrategy : IGameStrategy, IBoardSnapshotHistory
    {

        private bool IsWhitePerspective;
        private BoardViewModel boardVM;


        public BoardSnapshotHistory BoardSnapshotHistory { get; private set; }
        public GameReplay Replay { get; private set; }

        public ReplayGameStrategy(GameReplay replay)
        {
            Replay = replay;
        }

        public Task HandleSquareSelectedAsync(BoardViewModel boardVM, SquareViewModel squareVM)
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync(BoardViewModel VM)
        {
            this.boardVM = VM;

            BoardSnapshotHistory = new BoardSnapshotHistory(boardVM, this.IsWhitePerspective);

            foreach (var state in Replay.BoardStates)
                BoardSnapshotHistory.AddState(state);

            BoardSnapshotHistory.ResetBoardToFirstSnapshot();

        }

        public void SetBoardViewModel(BoardViewModel boardVM)
        {
            this.boardVM = boardVM;
        }
    }
}
