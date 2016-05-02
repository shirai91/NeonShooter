using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NeonShooter
{
    public abstract class Entity
    {
        protected Texture2D image;
        protected Color color = Color.White;
        public Vector2 Position, Velocity;
        public float Orientation;
        public float Radius = 20;
        public bool IsExpired;
        private Vector2 _size;
        public Vector2 Size
        {
            get
            {
                return image == null ? _size= Vector2.Zero : _size= new Vector2(image.Width, image.Height);
            }
            set { _size = value; }
        } 
        public abstract void Update();

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image,Position, null,color,Orientation,Size/2f,1f,0,0);
        }

    }
}
