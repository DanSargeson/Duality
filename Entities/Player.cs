using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Duality.Entities
{
    public class Player
    {
        public Vector2 Position { get; set; }
        public Vector2 StartPosition { get; set; } // Used to reset the player if they fall
        public float Speed { get; set; } = 200f;

        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public void Update(GameTime gameTime, Vector2 movementDirection, float currentFrequency, List<EnvironmentObject> envObjects) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 velocity = movementDirection * Speed * deltaTime;

            // 1. X-Axis Movement & Collision
            Position += new Vector2(velocity.X, 0);
            if (IsCollidingWithObstacle(currentFrequency, envObjects)) {
                Position -= new Vector2(velocity.X, 0); // Revert X movement if hit
            }

            // 2. Y-Axis Movement & Collision
            Position += new Vector2(0, velocity.Y);
            if (IsCollidingWithObstacle(currentFrequency, envObjects)) {
                Position -= new Vector2(0, velocity.Y); // Revert Y movement if hit
            }

            // 3. Hazard Check (Falling through the gap)
            if (IsFalling(currentFrequency, envObjects)) {
                Position = StartPosition; // Reset the player
            }
        }

        private bool IsCollidingWithObstacle(float currentFrequency, List<EnvironmentObject> envObjects) {
            foreach (var obj in envObjects) {
                if (obj.Type == ObjectType.Obstacle && obj.IsSolid(currentFrequency) && Bounds.Intersects(obj.Bounds)) {
                    return true;
                }
            }
            return false;
        }

        private bool IsFalling(float currentFrequency, List<EnvironmentObject> envObjects) {
            bool overHazard = false;
            bool overPlatform = false;

            foreach (var obj in envObjects) {
                if (Bounds.Intersects(obj.Bounds)) {
                    if (obj.Type == ObjectType.Hazard && obj.IsSolid(currentFrequency)) overHazard = true;
                    if (obj.Type == ObjectType.Platform && obj.IsSolid(currentFrequency)) overPlatform = true;
                }
            }

            // You only fall if you are over a hazard AND the bridge above it isn't solid yet
            return overHazard && !overPlatform;
        }
    }
}