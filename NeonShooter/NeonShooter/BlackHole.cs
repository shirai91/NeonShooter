using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NeonShooter
{
    public class BlackHole :Entity
    {
        private static Random rand = new Random();
        private float sprayAngle = 0;
        private int hitpoints = 10;
        const int frameBeforeDie = 31;
        private int _frameRemainBeforeDie;
        private bool _isDying;
        public BlackHole(Vector2 position)
        {
            image = Art.BlackHole;
            Position = position;
            Radius = image.Width / 2f;
            _isDying = false;
        }

        public void WasShot()
        {
            hitpoints--;
            if (hitpoints <= 0)
            {
                _frameRemainBeforeDie = frameBeforeDie;
                _isDying = true;
            }

        }

        public void Kill()
        {
            hitpoints = 0;
            WasShot();
        }

        public override void Update()
        {
            var entities = EntityManager.GetNearbyEntities(Position, 250);
            // The black holes spray some orbiting particles. The spray toggles on and off every quarter second.
            if ((GameRoot.GameTime.TotalGameTime.Milliseconds / 250) % 2 == 0)
            {
                var sprayVel = MathUtil.FromPolar(sprayAngle, rand.NextFloat(12, 15));
                var color = ColorUtil.HSVToColor(5, 0.5f, 0.8f);  // light purple
                var pos = Position + 2f * new Vector2(sprayVel.Y, -sprayVel.X) + rand.NextVector2(4, 8);
                var state = new ParticleState(sprayVel, ParticleType.Enemy);

                GameRoot.ParticleManager.CreateParticle(Art.LineParticle, pos, color, 60, new Vector2(0.7f,0.7f), state);
            }
            GameRoot.WarpingGrid.ApplyImplosiveForce(100f, new Vector3(Position.X, Position.Y, 0f), 20f);
            // rotate the spray direction
            sprayAngle -= MathHelper.TwoPi / 50f;
            foreach (var entity in entities)
            {
                if (entity is Enemy && !(entity as Enemy).IsActive)
                    continue;

                // bullets are repelled by black holes and everything else is attracted
                if (entity is Bullet)
                    entity.Velocity += (entity.Position - Position).ScaleTo(0.3f);
                else
                {
                    var dPos = Position - entity.Position;
                    var length = dPos.Length();

                    entity.Velocity += dPos.ScaleTo(MathHelper.Lerp(2, 0, length / 250f));
                }
            }
            if (_isDying)
            {
                _frameRemainBeforeDie--;
                if (_frameRemainBeforeDie % 6 == 0)
                    CreateExplosiveParticle();
                if (_frameRemainBeforeDie <= 0)
                    IsExpired = true;
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // make the size of the black hole pulsate
            float scale = 1 + 0.1f * (float)Math.Sin(10 * GameRoot.GameTime.TotalGameTime.TotalSeconds);
            spriteBatch.Draw(image, Position, null, color, Orientation, Size / 2f, scale, 0, 0);
        }

        private void CreateExplosiveParticle()
        {
            var hue = (float)((3 * GameRoot.GameTime.TotalGameTime.TotalSeconds) % 6);
            var particleColor = ColorUtil.HSVToColor(hue, 0.25f, 1);
            const int numParticles = 150;
            var startOffset = rand.NextFloat(0, MathHelper.TwoPi / numParticles);
            Sound.Explosion.Play(0.5f, rand.NextFloat(-0.2f, 0.2f), 0);
            for (var i = 0; i < numParticles; i++)
            {
                var sprayVel = MathUtil.FromPolar(MathHelper.TwoPi * i / numParticles + startOffset, rand.NextFloat(8, 16));
                var pos = Position + 2f * sprayVel;
                var state = new ParticleState(sprayVel, ParticleType.Bullet);

                GameRoot.ParticleManager.CreateParticle(Art.LineParticle, pos, particleColor, 180, new Vector2(0.7f, 0.7f), state);
            }
        }
    }
}
