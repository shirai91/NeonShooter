using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NeonShooter
{
    public static class Input
    {
        private static KeyboardState keyboardState, lastKeyboardState;
        private static MouseState mouseState, lastMouseState;
        private static bool isAimingWithMouse = false;
        public static Vector2 MousePosition=> new Vector2(mouseState.X,mouseState.Y);

        public static void Update()
        {
            lastKeyboardState = keyboardState;
            lastMouseState = mouseState;
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            isAimingWithMouse = !new[] {Keys.Left, Keys.Right, Keys.Up, Keys.Down}.Any(x => keyboardState.IsKeyDown(x));
        }

        public static bool WasKeyPressed(Keys key)
        {
            return lastKeyboardState.IsKeyDown(key) && keyboardState.IsKeyDown(key);
        }

        public static Vector2 GetMovementDirection()
        {
            var direction = new Vector2();
            direction.Y *= -1;
            if (keyboardState.IsKeyDown(Keys.W))
            {
                direction.Y -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                direction.Y += 1;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                direction.X -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                direction.X += 1;
            }
            return direction;
        }

        public static Vector2 GetAimDirection()
        {
            if (isAimingWithMouse)
                return GetMouseAimDirection();
            var direction = new Vector2();
            direction.Y *= -1;
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                direction.X -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                direction.X += 1;
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                direction.Y -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                direction.Y += 1;
            }
            return direction == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(direction);
        }

        public static Vector2 GetMouseAimDirection()
        {
            var direction = MousePosition - PlayerShip.Instance.Position;
            return direction == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(direction);
        }

        public static bool WasBombButtonPressed()
        {
            return WasKeyPressed(Keys.Space);
        }
    }
}
