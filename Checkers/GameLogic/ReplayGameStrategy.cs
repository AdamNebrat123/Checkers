using Checkers.Model;
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
    public class ReplayGameStrategy : IGameStrategy, IBoardSnapshotHistory, IGameNames
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

        public async Task<(string playerName, string opponentName)> GetGameNames()
        {
            string playerName = string.Empty;
            string opponentName = string.Empty;
            if (IsWhitePerspective)
            {
                if (Replay.GuestColor == PieceColor.White.ToString())
                {
                    playerName = Replay.Guest;
                    opponentName = Replay.Host;
                }
                else
                {
                    playerName = Replay.Host;
                    opponentName = Replay.Guest;
                }
            }
            else
            {
                if (Replay.GuestColor == PieceColor.Black.ToString())
                {
                    playerName = Replay.Guest;
                    opponentName = Replay.Host;
                }
                else
                {
                    playerName = Replay.Host;
                    opponentName = Replay.Guest;
                }
            }

            return (playerName, opponentName);
        }
    }
}
