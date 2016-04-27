using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NeonShooter
{
    public enum ParticleType
    {
        None,
        Enemy,
        Bullet,
        IgnoreGravity
    }

    public class ParticleState
    {
        public Vector2 Velocity;
        public ParticleType Type;
        public float LengthMultiplier;

        public ParticleState(Vector2 velocity, ParticleType type, float length = 1)
        {
            Velocity = velocity;
            Type = type;
        }
        public static void UpdateParticle(ParticleManager<ParticleState>.Particle particle)
        {
            var velocity = particle.State.Velocity;
            particle.Position += velocity;
            particle.Orientation = velocity.ToAngle();
            float speed = velocity.Length();
            float alpha = Math.Min(1, Math.Min(particle.PercentLife*2, speed*1f));
            alpha *= alpha;
            particle.Color.A = (byte) (255*alpha);
            //particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * speed + 0.1f), alpha);
            //particle.Scale.Y = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * speed + 0.1f), alpha);
            //when velocity ~ 0, set it to 0;
            var pos = particle.Position;
            int width = (int)GameRoot.ScreenSize.X;
            int height = (int)GameRoot.ScreenSize.Y;

            // collide with the edges of the screen
            if (pos.X < 0) velocity.X = Math.Abs(velocity.X);
            else if (pos.X > width)
                velocity.X = -Math.Abs(velocity.X);
            if (pos.Y < 0) velocity.Y = Math.Abs(velocity.Y);
            else if (pos.Y > height)
                velocity.Y = -Math.Abs(velocity.Y);
            if (Math.Abs(velocity.X) + Math.Abs(velocity.Y) < 0.00000001f)
            {
                velocity = Vector2.Zero;
            }
            else if (particle.State.Type == ParticleType.Enemy)
                velocity *= 0.94f;
            else
                velocity *= 0.96f + Math.Abs(velocity.X) % 0.04f;
            if (particle.State.Type != ParticleType.IgnoreGravity)
            {
                foreach (var blackHole in EntityManager.BlackHoles)
                {
                    var dPos = blackHole.Position - pos;
                    float distance = dPos.Length();
                    var n = dPos / distance;
                    velocity += 10000 * n / (distance * distance + 10000);

                    // add tangential acceleration for nearby particles
                    if (distance < 400)
                        velocity += 45 * new Vector2(n.Y, -n.X) / (distance + 100);
                }
            }
            particle.State.Velocity = velocity;
        }
    }
    public class ParticleManager<T>
    {
        public class Particle
        {
            public Texture2D Texture;
            public Vector2 Position;
            public float Orientation;
            public Vector2 Scale = Vector2.One;
            public Color Color;
            public float Duration;
            public float PercentLife = 1f;
            public T State;
        }

        private class CircularParticleArray
        {
            private Particle[] listParticles;
            private int start;

            public int Start
            {
                get { return start; }
                set { start = value%listParticles.Length; }
            }
            public int Count { get; set; }
            public int Capacity => listParticles.Length;

            public CircularParticleArray(int capacity)
            {
                listParticles = new Particle[capacity];
            }

            public Particle this[int i]
            {
                get { return listParticles[(start + i)%listParticles.Length]; }
                set { listParticles[(start + i)%listParticles.Length] = value; }
            }
        }

        private Action<Particle> updateParticle;
        private CircularParticleArray particleList;

        public ParticleManager(int capacity, Action<Particle> updateParticle)
        {
            this.updateParticle = updateParticle;
            particleList = new CircularParticleArray(capacity);
            for (var i = 0; i < capacity; i++)
            {
                particleList[i] = new Particle();
            }
        }

        public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale,
            T state, float theta = 0)
        {
            Particle particle;
            if (particleList.Count == particleList.Capacity)
            {
                particle = particleList[0];
                particleList.Start++;
            }
            else
            {
                particle = particleList[particleList.Count];
                particleList.Count++;   
            }
            particle.Texture = texture;
            particle.Position = position;
            particle.Color = tint;
            particle.Duration = duration;
            particle.PercentLife = 1f;
            particle.Scale = scale;
            particle.Orientation = theta;
            particle.State = state;
        }

        public void Update()
        {
            var removalCount = 0;
            for (var i = 0; i < particleList.Count; i++)
            {
                var particle = particleList[i];
                updateParticle(particle);
                particle.PercentLife -= 1f/particle.Duration;
                //shift deleted particle to end of list
                Swap(particleList, i - removalCount, i);
                if (particle.PercentLife < 0)
                    removalCount++;
            }
            particleList.Count -= removalCount;
        }

        private static void Swap(CircularParticleArray list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i < particleList.Count; i++)
            {
                var particle = particleList[i];
                var origin = new Vector2((float)particle.Texture.Width/2,(float)particle.Texture.Height/2);
                spriteBatch.Draw(particle.Texture,particle.Position, null,particle.Color,particle.Orientation,origin,particle.Scale,0,0);
            }
        }
    }
}
