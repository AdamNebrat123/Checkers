using Checkers.Models;
using System.Threading.Tasks;

namespace Checkers.GameLogic
{
    public interface IGameStrategy
    {
        /// <summary>
        /// מופעל כאשר השחקן ביצע מהלך.
        /// </summary>
        Task OnPlayerMoveAsync(GameMove move);

        /// <summary>
        /// מאתחל את המשחק (אם צריך לטעון מהלכים קודמים או להזניק תור ראשון).
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// מנקה משאבים, מאזינים וכו'.
        /// </summary>
        void Dispose();
    }
}
