using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NeonShooter
{
    public class PlayerShip:Entity
    {
        private static PlayerShip instance;
        const int cooldownFrames = 6;
        int cooldownRemaining = 0;
        static Random rand = new Random();
        public static PlayerShip Instance => instance ?? (instance = new PlayerShip());
        int framesUntilRespawn = 0;
        public bool IsDead { get { return framesUntilRespawn > 0; } }
        private PlayerShip()
        {
            image = Art.Player;
            Position = GameRoot.ScreenSize/2;
            Radius = 10;
        }

        public override void Update()
        {
            if (IsDead)
            {
                framesUntilRespawn--;
                return;
            }
            const float speed = 8;
            Velocity = speed*Input.GetMovementDirection();
            Position += Velocity;
            Position = Vector2.Clamp(Position, Size/2, GameRoot.ScreenSize - Size/2);
            if (Velocity.LengthSquared() > 0)
            {
                Orientation = Velocity.ToAngle();
            }
            var aim = Input.GetAimDirection();
            if (aim.LengthSquared() > 0 && cooldownRemaining <= 0)
            {
                cooldownRemaining = cooldownFrames;
                var aimAngle = aim.ToAngle();
                var aimQuaternion = Quaternion.CreateFromYawPitchRoll(0, 0, aimAngle);
                var randomSpread = rand.NextFloat(-0.04f, 0.04f) + rand.NextFloat(-0.04f, 0.04f);
                var vel = MathUtil.FromPolar(aimAngle + randomSpread, 11f);
                var offset = Vector2.Transform(new Vector2(25, -8), aimQuaternion);
                EntityManager.Add(new Bullet(Position + offset,vel));
                Sound.Shot.Play(0.2f, rand.NextFloat(-0.2f, 0.2f), 0);
            }
            if (cooldownRemaining > 0)
            {
                cooldownRemaining--;
            }
            MakeExhaustFire();
        }
        public void Kill()
        {
            EnemySpawner.Reset();
            framesUntilRespawn = 60;
            PlayerStatus.RemoveLife();
            PlayerStatus.ResetMultiplier();
            framesUntilRespawn = PlayerStatus.IsGameOver ? 300 : 120;
            if (PlayerStatus.Lives == 0)
            {
                PlayerStatus.GameOver();
                GameManager.PauseGame(300,true);
            }
            var yellow = new Color(0.8f, 0.8f, 0.4f);

            for (var i = 0; i < 1200; i++)
            {
                var speed = 18f * (1f - 1 / rand.NextFloat(1f, 10f));
                var particleColor = Color.Lerp(Color.White, yellow, rand.NextFloat(0, 1));
                var state = new ParticleState(rand.NextVector2(speed, speed), ParticleType.None);

                GameRoot.ParticleManager.CreateParticle(Art.LineParticle, Position, particleColor, 60, new Vector2(0.7f,0.7f), state);
            }

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsDead)
                base.Draw(spriteBatch);
        }
        private void MakeExhaustFire()
        {
            if (Velocity.LengthSquared() > 0.1f)
            {
                // set up some variables
                Orientation = Velocity.ToAngle();
                var rot = Quaternion.CreateFromYawPitchRoll(0f, 0f, Orientation);
                
                var t = GameRoot.GameTime.TotalGameTime.TotalSeconds;
                // The primary velocity of the particles is 3 pixels/frame in the direction opposite to which the ship is travelling.
                var baseVel = Velocity.ScaleTo(-3);
                // Calculate the sideways velocity for the two side streams. The direction is perpendicular to the ship's velocity and the
                // magnitude varies sinusoidally.
                var perpVel = new Vector2(baseVel.Y, -baseVel.X) * (0.6f * (float)Math.Sin(t * 10));
                var sideColor = new Color(200, 38, 9);    // deep red
                var midColor = new Color(255, 187, 30);   // orange-yellow
                var pos = Position + Vector2.Transform(new Vector2(-25, 0), rot);   // position of the ship's exhaust pipe.
                const float alpha = 0.7f;

                // middle particle stream
                var velMid = baseVel + rand.NextVector2(0, 1);
                GameRoot.ParticleManager.CreateParticle(Art.LineParticle, pos, Color.White * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(velMid, ParticleType.Enemy));
                GameRoot.ParticleManager.CreateParticle(Art.Glow, pos, midColor * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(velMid, ParticleType.Enemy));

                // side particle streams
                var vel1 = baseVel + perpVel + rand.NextVector2(0, 0.3f);
                var vel2 = baseVel - perpVel + rand.NextVector2(0, 0.3f);
                GameRoot.ParticleManager.CreateParticle(Art.LineParticle, pos, Color.White * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel1, ParticleType.Enemy));
                GameRoot.ParticleManager.CreateParticle(Art.LineParticle, pos, Color.White * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel2, ParticleType.Enemy));

                GameRoot.ParticleManager.CreateParticle(Art.Glow, pos, sideColor * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel1, ParticleType.Enemy));
                GameRoot.ParticleManager.CreateParticle(Art.Glow, pos, sideColor * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel2, ParticleType.Enemy));
            }
        }
    }
}
