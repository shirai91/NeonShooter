using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace NeonShooter
{
    public static class Extensions
    {
        public static float ToAngle(this Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }
        public static float NextFloat(this Random rand, float minValue, float maxValue)
        {
            return (float)rand.NextDouble() * (maxValue - minValue) + minValue;
        }
        public static Vector2 ScaleTo(this Vector2 vector, float length)
        {
            return vector * (length / vector.Length());
        }
    }
}
