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


        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsDead)
                base.Draw(spriteBatch);
        }
    }
}
