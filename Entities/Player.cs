using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Duality.Entities
{
    public class Player
    {
        public Vector2 Position { get; set; }

        // Tracks the last place the player stood that wasn't a hazard
        public Vector2 LastSafePosition { get; set; }

        public float Speed { get; set; } = 200f;

        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;

        // The full rendering bounds
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public void Update(GameTime gameTime, Vector2 movementDirection, float currentFrequency, List<EnvironmentObject> envObjects) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 velocity = movementDirection * Speed * deltaTime;

            // 1. X-Axis Movement & Collision
            Position += new Vector2(velocity.X, 0);
            if (IsCollidingWithObstacle(currentFrequency, envObjects)) {
                Position -= new Vector2(velocity.X, 0); // Revert X if we hit a wall
            }

            // 2. Y-Axis Movement & Collision
            Position += new Vector2(0, velocity.Y);
            if (IsCollidingWithObstacle(currentFrequency, envObjects)) {
                Position -= new Vector2(0, velocity.Y); // Revert Y if we hit a wall
            }

            // 3. Top-Down Hazard Check
            CheckGround(currentFrequency, envObjects, out bool isFalling, out bool isCompletelySafe);

            if (isFalling) {
                // The player fell! (e.g. they shifted to Density while standing on the Insight bridge)
                // Snap them back to the last truly safe ground they were standing on.
                Position = LastSafePosition;
            }
            else if (isCompletelySafe) {
                // Only update our safety anchor if we are on solid ground, NOT over a pit at all.
                LastSafePosition = Position;
            }
        }

        private bool IsCollidingWithObstacle(float currentFrequency, List<EnvironmentObject> envObjects) {
            foreach (var obj in envObjects) {
                // For walls, we still check the full Bounds so you stop right at the edge
                if (obj.Type == ObjectType.Obstacle && obj.IsSolid(currentFrequency) && Bounds.Intersects(obj.Bounds)) {
                    return true;
                }
            }
            return false;
        }

        private void CheckGround(float currentFrequency, List<EnvironmentObject> envObjects, out bool isFalling, out bool isCompletelySafe) {
            bool overHazard = false;
            bool overPlatform = false;

            // In Top-Down, we only care if the absolute CENTER of the player is over the pit.
            Point playerCenter = Bounds.Center;

            foreach (var obj in envObjects) {
                // Note we use 'Contains' instead of 'Intersects' here
                if (obj.Bounds.Contains(playerCenter)) {
                    if (obj.Type == ObjectType.Hazard && obj.IsSolid(currentFrequency)) overHazard = true;
                    if (obj.Type == ObjectType.Platform && obj.IsSolid(currentFrequency)) overPlatform = true;
                }
            }

            // You fall if your center is over a hazard, AND there's no platform to hold you up
            isFalling = overHazard && !overPlatform;

            // You are completely safe only if you are not over a hazard at all
            isCompletelySafe = !overHazard;
        }
    }
}