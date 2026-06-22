using Microsoft.Xna.Framework;

namespace Duality.Entities
{
    public class Player
    {
        // Floating point position for smooth math
        public Vector2 Position { get; set; }
        public float Speed { get; set; } = 200f;

        // Player dimensions
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;

        // Automatically generates the integer-based Rectangle for rendering/collision
        // by casting the floating-point position coordinates.
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public void Update(GameTime gameTime, Vector2 movementDirection) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Apply movement
            Position += movementDirection * Speed * deltaTime;

            // TODO: Pass in current frequency and check collisions against EnvironmentObjects
        }
    }
}