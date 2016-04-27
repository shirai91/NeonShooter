using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace NeonShooter
{
    public class Bullet : Entity
    {
        private static Random rand = new Random();

        public Bullet(Vector2 position, Vector2 velocity)
        {
            image = Art.Bullet;
            Position = position;
            Velocity = velocity;
            Orientation = Velocity.ToAngle();
            Radius = 8;
        }

        public override void Update()
        {
            if (Velocity.LengthSquared() > 0)
            {
                Orientation = Velocity.ToAngle();
            }
            Position += Velocity;
            if (!GameRoot.Viewport.Bounds.Contains(Position.ToPoint()))
            {
                IsExpired = true;
                for (var i = 0; i < 30; i++)
                    GameRoot.ParticleManager.CreateParticle(Art.LineParticle, Position, Color.LightBlue, 50,
                        new Vector2(0.5f, 0.5f),
                        new ParticleState(Velocity = rand.NextVector2(0, 9), ParticleType.Bullet));
            }

        }
    }
}
