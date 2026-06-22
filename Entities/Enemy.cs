using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Duality.Entities
{
    public class Enemy
    {
        public Vector2 Position { get; set; }
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public Color BaseColor { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; } = 0.4f;
        public float Speed { get; set; } = 100f;

        public List<Vector2> Waypoints { get; set; }
        private int _currentWaypointIndex = 0;

        public Enemy(Vector2 startPos, Color color, float anchorFrequency, List<Vector2> waypoints) {
            Position = startPos;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            Waypoints = waypoints;

            if (Waypoints == null || Waypoints.Count == 0) {
                Waypoints = new List<Vector2> { startPos };
            }
        }

        public void Update(GameTime gameTime) {
            if (Waypoints.Count <= 1) return;

            // Move towards the current waypoint
            Vector2 target = Waypoints[_currentWaypointIndex];
            Vector2 direction = target - Position;

            if (direction.Length() < 5f) {
                // We reached the waypoint, target the next one (looping around)
                _currentWaypointIndex = (_currentWaypointIndex + 1) % Waypoints.Count;
            }
            else {
                direction.Normalize();
                Position += direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public float GetPresence(float currentFrequency) {
            float distance = Math.Abs(AnchorFrequency - currentFrequency);
            if (distance > Range) return 0f;
            return 1f - (distance / Range);
        }

        // Enemies are only dangerous if they are materialized enough to touch you
        public bool IsDangerous(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }
    }
}