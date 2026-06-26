using Duality.Data;
using Duality.Mechanics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Duality.Entities
{
    public class Player
    {
        public Vector2 Position { get; set; }

        public Vector2 StartPosition { get; set; }

        // Tracks the last place the player stood that wasn't a hazard
        public Vector2 LastSafePosition { get; set; }

        public float Speed { get; set; } = 200f;

        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;

        // The full rendering bounds
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public bool IsInsideAnyObstacle(Data.Level level) {
            // Check normal walls
            foreach (var obj in level.EnvironmentObjects) {
                if (obj.Type == ObjectType.Obstacle && Bounds.Intersects(obj.Bounds)) {
                    return true;
                }
            }

            // Check standard interactables (if they act as walls when closed)
            foreach (var interactable in level.Interactables) {
                if (interactable.IsClosed && Bounds.Intersects(interactable.Bounds)) {
                    return true;
                }
            }

            if (level.LockedDoors != null) {
                foreach (var door in level.LockedDoors) {
                    if (door.IsLocked && Bounds.Intersects(door.Bounds)) {
                        return true;
                    }
                }
            }

            return false;
        }


        public void Update(GameTime gameTime, Vector2 movementDirection, PolarityManager polarityManager, Data.Level level) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 velocity = movementDirection * Speed * deltaTime;
    
            // 1. X-Axis Movement & Collision
            Position += new Vector2(velocity.X, 0);
            if (IsCollidingWithObstacle(polarityManager.CurrentFrequency, level)) {
                Position -= new Vector2(velocity.X, 0); // Revert X if we hit a wall
            }

            // 2. Y-Axis Movement & Collision
            Position += new Vector2(0, velocity.Y);
            if (IsCollidingWithObstacle(polarityManager.CurrentFrequency, level)) {
                Position -= new Vector2(0, velocity.Y); // Revert Y if we hit a wall
            }

            // 3. Top-Down Hazard Check
            CheckGround(polarityManager.CurrentFrequency, level, out bool isFalling, out bool isCompletelySafe);

            // Are we currently stuck inside a solid wall because of a frequency shift?
            bool isStuckInWall = IsCollidingWithObstacle(polarityManager.CurrentFrequency, level);

            if (isFalling || isStuckInWall) {
                // The player fell! (e.g. they shifted to Density while standing on the Insight bridge)
                // Snap them back to the last truly safe ground they were standing on.
                Position = LastSafePosition;
            }
            else if (isCompletelySafe && !IsInsideAnyObstacle(level)) {
                // Only update our safety anchor if we are on solid ground, NOT over a pit at all.
                LastSafePosition = Position;
                 
            }

            if (IsCollidingWithEnemy(polarityManager.CurrentFrequency, level)) {
                // Caught! Bounce back to the last safe spot
                Position = StartPosition;
                polarityManager.SetFrequency(0f);

                // Hard reset EVERY enemy in the room so the Stalker doesn't spawn-camp you
                foreach (var enemy in level.Enemies) {
                    enemy.ResetToSpawn();
                }
            }

            // 5. Clamp Player to Level Bounds
            // Prevent the player from walking off the edge of the world map
            Position = new Vector2(
                MathHelper.Clamp(Position.X, level.Bounds.Left, level.Bounds.Right - Width),
                MathHelper.Clamp(Position.Y, level.Bounds.Top, level.Bounds.Bottom - Height)
            );
        }
        

        private bool IsCollidingWithObstacle(float currentFrequency, Data.Level level) {
            foreach (var obj in level.EnvironmentObjects) {
                // For walls, we still check the full Bounds so you stop right at the edge
                if (obj.Type == ObjectType.Obstacle && obj.IsSolid(currentFrequency) && Bounds.Intersects(obj.Bounds)) {
                    return true;
                }
            }

            foreach (var interactable in level.Interactables) {
                if (interactable.IsSolid(currentFrequency) && Bounds.Intersects(interactable.Bounds)) {
                    return true;
                }
            }

            if (level.LockedDoors != null) {
                foreach (var door in level.LockedDoors) {
                    if (door.IsLocked && door.IsSolid(currentFrequency) && Bounds.Intersects(door.Bounds)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsCollidingWithEnemy(float currentFrequency, Data.Level level) {
            foreach (var enemy in level.Enemies) {
                // If the enemy exists on this frequency and touches you, you're caught
                if (enemy.IsDangerous(currentFrequency) && Bounds.Intersects(enemy.Bounds)) {
                    return true;
                }
            }
            return false;
        }

        private void CheckGround(float currentFrequency, Data.Level level, out bool isFalling, out bool isCompletelySafe) {
            bool overHazard = false;
            bool overPlatform = false;

            // In Top-Down, we only care if the absolute CENTER of the player is over the pit.
            Point playerCenter = Bounds.Center;

            foreach (var obj in level.EnvironmentObjects) {
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