using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace NeonShooter
{
    public static class Sound
    {
        public static Song Music { get; private set; }
        private static readonly Random rand = new Random();
        private static SoundEffect[] explosions;
        public static SoundEffect Explosion => explosions[rand.Next(explosions.Length)];
        private static SoundEffect[] shots;
        public static SoundEffect Shot => shots[rand.Next(shots.Length)];
        private static SoundEffect[] spawns;
        public static SoundEffect Spawn => spawns[rand.Next(spawns.Length)];

        public static void Load(ContentManager content)
        {
            Music = content.Load<Song>("Sound/Music");
            explosions =
                Enumerable.Range(1, 8).Select(x => content.Load<SoundEffect>("Sound/explosion-0" + x)).ToArray();
            shots = Enumerable.Range(1, 4).Select(x => content.Load<SoundEffect>("Sound/shoot-0" + x)).ToArray();
            spawns = Enumerable.Range(1, 8).Select(x => content.Load<SoundEffect>("Sound/spawn-0" + x)).ToArray();
        }
    }
}
