using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Duality.Entities
{
    public class Anomaly
    {
        public Vector2 Position { get; set; }
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public Color BaseColor { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; } = 0.4f;

        // Patrol Logic
        public List<Vector2> PatrolPoints { get; set; } = new List<Vector2>();
        private int _currentPatrolIndex = 0;
        public float Speed { get; set; } = 100f;

        // Calculates how "real" the entity currently is
        public float GetPresence(float currentFrequency) {
            float distance = Math.Abs(AnchorFrequency - currentFrequency);
            if (distance > Range) return 0f;
            return 1f - (distance / Range);
        }

        // An anomaly can only hurt the player if it is solid enough in the current reality
        public bool IsDangerous(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }

        public void Update(GameTime gameTime) {
            if (PatrolPoints == null || PatrolPoints.Count == 0) return;

            Vector2 target = PatrolPoints[_currentPatrolIndex];
            Vector2 direction = target - Position;
            float distance = direction.Length();

            // If we reached the target point, cycle to the next one
            if (distance < 5f) {
                _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Count;
            }
            else {
                direction.Normalize();
                Position += direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
    }
}