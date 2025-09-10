using Checkers.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                    OnPropertyChanged(nameof(PieceImage));
                }
            }
        }

        public string PieceImage =>
            Piece == null ? string.Empty :
            Piece.Color == PieceColor.White ? "white_piece.png" : "black_piece.png";

        public ICommand SquareClickedCommand { get; }

        public SquareViewModel(Square square)
        {
            this.square = square;
            SquareClickedCommand = new Command(OnSquareClicked);
        }

        private void OnSquareClicked()
        {
            if (Piece != null)
            {
                Application.Current.MainPage.DisplayAlert("לחיצה", $"נבחרה חתיכה {Piece.Color}", "אישור");
            }
            else
            {
                // ריק – אולי בעתיד נוסיף פונקציונליות
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
