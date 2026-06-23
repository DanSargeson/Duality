using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Duality.Entities
{
    public enum EnemyBehaviour { Patrol, Hunter }

    public class Enemy : Entity
    {
        public Vector2 Position { get; set; }
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public float Speed { get; set; } = 100f;
        public EnemyBehaviour Behaviour { get; set; } = EnemyBehaviour.Patrol;

        public List<Vector2> Waypoints { get; set; }
        private int _currentWaypointIndex = 0;

        public Enemy(Vector2 startPos, Color color, float anchorFrequency, List<Vector2> waypoints) {
            Position = startPos;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            Waypoints = waypoints ?? new List<Vector2> { startPos };
        }

        // Updated signature to take the player and frequency for Hunter AI
        public void Update(GameTime gameTime, Player targetPlayer, float currentFrequency) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Behaviour == EnemyBehaviour.Hunter) {
                // Hunters only chase if they exist enough to be dangerous
                if (IsDangerous(currentFrequency)) {
                    Vector2 direction = targetPlayer.Position - Position;
                    if (direction.Length() > 0) {
                        direction.Normalize();
                        Position += direction * Speed * deltaTime;
                    }
                }
            }
            else if (Behaviour == EnemyBehaviour.Patrol) {
                if (Waypoints.Count <= 1) return;

                Vector2 target = Waypoints[_currentWaypointIndex];
                Vector2 direction = target - Position;

                if (direction.Length() < 5f) {
                    _currentWaypointIndex = (_currentWaypointIndex + 1) % Waypoints.Count;
                }
                else {
                    direction.Normalize();
                    Position += direction * Speed * deltaTime;
                }
            }
        }

        public bool IsDangerous(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }
    }
}