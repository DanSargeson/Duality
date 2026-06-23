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

        public float WakeDelay { get; set; } = 0f;
        private float _wakeTimer = 0f;
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

            if (base.GetPresence(currentFrequency) > 0f) {
                _wakeTimer += deltaTime;
                if (_wakeTimer > WakeDelay) _wakeTimer = WakeDelay;
            }
            else {
                // Player left the sweet spot! Fade back into the static twice as fast.
                _wakeTimer -= deltaTime * 2f;
                if (_wakeTimer < 0f) _wakeTimer = 0f;
            }



            if (Behaviour == EnemyBehaviour.Hunter) {
                // Hunters only chase if they exist enough to be dangerous
                if (IsDangerous(currentFrequency)) {
                    Vector2 direction = targetPlayer.Bounds.Center.ToVector2() - Bounds.Center.ToVector2();
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

        public override float GetPresence(float currentFrequency) {
            float basePresence = base.GetPresence(currentFrequency);

            // Normal enemies behave normally
            if (WakeDelay <= 0f) return basePresence;

            // Stalkers scale their visibility/solidity by how "awake" they are!
            return basePresence * (_wakeTimer / WakeDelay);
        }

        public bool IsDangerous(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }
    }
}