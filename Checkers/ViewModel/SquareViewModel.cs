using Checkers.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.ViewModel
{
    public class SquareViewModel : INotifyPropertyChanged
    {
        private readonly Square square;

        public int Row => square.Row;
        public int Column => square.Column;
        public bool IsDark => square.IsDark;

        public Piece? Piece
        {
            get => square.Piece;
            set
            {
                if (square.Piece != value)
                {
                    square.Piece = value;
                    OnPropertyChanged();
                }
            }
        }

        public SquareViewModel(Square square)
        {
            this.square = square;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
