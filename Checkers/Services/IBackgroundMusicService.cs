using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Services
{
    public interface IBackgroundMusicService
    {
        void Play(string musicName, bool isLooping = true);
        void Pause();
        void Unpause();
        void Stop();
        bool IsPlaying { get; }
        string CurrentMusic { get; }
    }
}
