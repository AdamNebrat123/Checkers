using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Services
{
    public interface IMusicService
    {
        void Play(string musicName, bool isLooping = true);
        void Pause();
        void Stop();
        bool IsPlaying { get; }
        string CurrentMusic { get; }
    }
}
