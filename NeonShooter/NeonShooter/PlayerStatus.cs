using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeonShooter
{
    public static class PlayerStatus
    {
        private const float multiplierExpiryTime = 0.8f;
        private const int maxMultiplier = 20;

        public static int Lives { get; private set; }
        public static int Score { get; private set; }
        public static int Multiplier { get; private set; }

        private static float multiplierTimeLeft;    // time until the current multiplier expires
        private static int scoreForExtraLife;       // score required to gain an extra life

        // Static constructor
        static PlayerStatus()
        {
            Reset();
        }

        public static void Reset()
        {
            Score = 0;
            Multiplier = 1;
            Lives = 4;
            scoreForExtraLife = 2000;
            multiplierTimeLeft = 0;
            ResetMultiplier();
        }

        public static void Update()
        {
            if (Multiplier > 1)
            {
                // update the multiplier timer
                if ((multiplierTimeLeft -= (float)GameRoot.GameTime.ElapsedGameTime.TotalSeconds) <= 0)
                {
                    multiplierTimeLeft = multiplierExpiryTime;
                    ResetMultiplier();
                }
            }
        }

        public static void AddPoints(int basePoints)
        {
            if (PlayerShip.Instance.IsDead)
                return;

            Score += basePoints * Multiplier;
            while (Score >= scoreForExtraLife)
            {
                scoreForExtraLife += 2000;
                Lives++;
            }
        }

        public static void IncreaseMultiplier()
        {
            if (PlayerShip.Instance.IsDead)
                return;

            multiplierTimeLeft = multiplierExpiryTime;
            if (Multiplier < maxMultiplier)
                Multiplier++;
        }

        public static void ResetMultiplier()
        {
            Multiplier = 1;
        }

        public static void RemoveLife()
        {
            Lives--;
        }
    }
}
