﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    
    public class GameModel
    {
        public string GameId { get; set; } = Guid.NewGuid().ToString();

        //  יוצר המשחק
        public string Host { get; set; }
        public string HostColor { get; set; }

        // האורח
        public string Guest { get; set; }
        public string GuestColor { get; set; }

        // מי בתור עכשיו
        public bool IsWhiteTurn { get; set; }

        // מצב הלוח: 0=ריק, 1=לבן, 2=שחור, 3=לבן מלך, 4=שחור מלך
        public int[,] BoardState { get; set; } = new int[8, 8];

        // רשימת המהלכים שבוצעו
        public List<GameMove> Moves { get; set; } = new();

        // תאריך יצירה
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
