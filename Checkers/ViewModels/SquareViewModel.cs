using Checkers.Model;
using Checkers.Models;
using Checkers.ViewModels;
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
    public class SquareViewModel : ViewModelBase
    {
        private readonly Square square;
        private readonly Action<SquareViewModel> onSelected;
        private bool hasMoveMarker;

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

        public string PieceImage
        {
            get
            {
                if (Piece == null) return string.Empty;

                return Piece switch
                {
                    Man m when m.Color == PieceColor.White => "white_piece.png",
                    Man m when m.Color == PieceColor.Black => "black_piece.png",
                    King k when k.Color == PieceColor.White => "white_piece_king.png",
                    King k when k.Color == PieceColor.Black => "black_piece_king.png",
                    _ => string.Empty
                };
            }
        }

        public bool HasMoveMarker
        {
            get => hasMoveMarker;
            set
            {
                if (hasMoveMarker != value)
                {
                    hasMoveMarker = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SquareClickedCommand { get; }

        public SquareViewModel(Square square, Action<SquareViewModel> onSelected)
        {
            this.square = square;
            this.onSelected = onSelected;
            SquareClickedCommand = new Command(OnSquareClicked);
        }

        private void OnSquareClicked()
        {
            onSelected?.Invoke(this);
        }

        public void RaisePieceImageChanged()
        {
            OnPropertyChanged(nameof(PieceImage));
        }

        public void UpdateProperty([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(nameof(propertyName));
        }

    }
}
