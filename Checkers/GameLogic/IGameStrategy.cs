using Checkers.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public interface IGameStrategy
    {
        Task HandleSquareSelectedAsync(BoardViewModel boardVM, SquareViewModel squareVM);
        void SetBoardViewModel(BoardViewModel boardVM);
        Task InitializeAsync(BoardViewModel boardVM);
    }
}
