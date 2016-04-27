using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeonShooter
{
    public static class GameManager
    {
        public static bool IsPaused { get; private set; }
        public static int PauseFrame { get; private set; }
        public static bool IsPausedWhenGameOver { get; private set; }
        public static void PauseGame(int frame,bool isPausedWhenGameOver)
        {
            IsPaused = true;
            PauseFrame = frame;
            IsPausedWhenGameOver = isPausedWhenGameOver;
        }
        public static void Update()
        {
            if (!IsPaused) return;
            PauseFrame--;
            if (PauseFrame == 0)
            {
                IsPaused = false;
                if (IsPausedWhenGameOver)
                {
                    IsPausedWhenGameOver = true;
                    PlayerStatus.Reset();
                }
            }
        }
    }
}
