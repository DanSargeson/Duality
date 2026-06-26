using Duality.Data;
using Duality.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Duality.Entities
{
    public enum EnemyBehaviour { Patrol, Hunter, Stalker }

    public class Enemy : Entity
    {
        public Vector2 Position { get; set; }
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;

        private float _trailTimer = 0f;
        private float _trailSpawnRate = 0.1f; // Drops a data footprint every 0.1 seconds
        private Random _random = new Random();

        public Vector2 StartPosition { get; private set; }

        public float WakeDelay { get; set; } = 0f;
        private float _wakeTimer = 0f;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public float Speed { get; set; } = 100f;
        public EnemyBehaviour Behaviour { get; set; } = EnemyBehaviour.Patrol;

        public List<Vector2> Waypoints { get; set; }
        private int _currentWaypointIndex = 0;

        public Enemy(Vector2 startPos, Color color, float anchorFrequency, List<Vector2> waypoints) {
            StartPosition = startPos;
            Position = startPos;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            Waypoints = waypoints ?? new List<Vector2> { startPos };
        }

        public void ResetToSpawn() {
            Position = StartPosition;
            _currentWaypointIndex = 0;

            // Force them back to sleep so the Stalker isn't instantly active
            _wakeTimer = 0f;
        }


        private bool IsCollidingWithObstacle(Data.Level level) {
            // The Apex Predator does not care about geometry.
            if (Behaviour == EnemyBehaviour.Stalker) return false;

            foreach (var obj in level.EnvironmentObjects) {
                // The enemy checks solidity against its OWN dimension, not the player's
                if (obj.Type == ObjectType.Obstacle && obj.IsSolid(AnchorFrequency) && Bounds.Intersects(obj.Bounds)) {
                    return true;
                }
            }

            foreach (var interactable in level.Interactables) {
                if (interactable.IsSolid(AnchorFrequency) && Bounds.Intersects(interactable.Bounds)) {
                    return true;
                }
            }

            if (level.LockedDoors != null) {
                foreach (var door in level.LockedDoors) {
                    if (door.IsLocked && door.IsSolid(AnchorFrequency) && Bounds.Intersects(door.Bounds)) {
                        return true;
                    }
                }
            }

            return false;
        }


        public void Update(GameTime gameTime, Player targetPlayer, float currentFrequency, List<GlitchParticle> particles, Level level) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Determine if the enemy is in its active frequency phase
            bool isActivePhase = false;

            if (Behaviour == EnemyBehaviour.Stalker) {
                // Stalkers wake up at Anchor and persist infinitely into Insight
                isActivePhase = currentFrequency >= AnchorFrequency;
            }
            else {
                // Standard enemies use the original bidirectional check
                isActivePhase = base.GetPresence(currentFrequency) > 0.5f;
            }

            // 2. Process Wake Timer
            if (isActivePhase) {
                _wakeTimer += deltaTime;
                if (_wakeTimer > WakeDelay) _wakeTimer = WakeDelay;
            }
            else {
                // Player left the sweet spot! Fade back into the static.
                _wakeTimer -= deltaTime * 2f;
                if (_wakeTimer < 0f) _wakeTimer = 0f;
            }

            

            Vector2 oldPosition = Position;

            // 4. Execution Logic
            if (Behaviour == EnemyBehaviour.Hunter || Behaviour == EnemyBehaviour.Stalker) {
                if (IsDangerous(currentFrequency)) {
                    Vector2 direction = targetPlayer.Bounds.Center.ToVector2() - Bounds.Center.ToVector2();
                    if (direction.Length() > 0) {
                        direction.Normalize();
                        ApplyMovement(direction * Speed * deltaTime, level);
                    }
                }
            }
            else if (Behaviour == EnemyBehaviour.Patrol) {
                if (Waypoints.Count > 1) {
                    Vector2 target = Waypoints[_currentWaypointIndex];
                    Vector2 direction = target - Position;

                    if (direction.Length() < 5f) {
                        _currentWaypointIndex = (_currentWaypointIndex + 1) % Waypoints.Count;
                    }
                    else {
                        direction.Normalize();
                        ApplyMovement(direction * Speed * deltaTime, level);
                    }
                }
            }


            // If the enemy took a step...
            if (Position != oldPosition) {

                // Only shed the data wake if the player is peeking into Insight
                // We ignore Stalkers because they are already natively visible in Insight
                if (currentFrequency > 0.4f && Behaviour != EnemyBehaviour.Stalker) {

                    _trailTimer -= deltaTime;
                    if (_trailTimer <= 0) {
                        _trailTimer = _trailSpawnRate;

                        particles.Add(new GlitchParticle {
                            // Spawn slightly randomized around the enemy's feet
                            Position = new Vector2(Bounds.Center.X + _random.Next(-6, 6), Bounds.Bottom + _random.Next(-4, 4)),
                            // Drift slowly upwards like evaporating heat or corrupt data
                            Velocity = new Vector2(0, -15f),
                            // Set to a ghostly cyan or white, at half opacity
                            BaseColor = BaseColor * 0.5f,
                            MaxLife = 1.2f, // Lingers just long enough to show a path
                            Life = 1.2f,
                            Size = _random.Next(2, 4)
                        });
                    }
                }
            }
        }


        
        private void ApplyMovement(Vector2 velocity, Data.Level level) {
            // Move X
            Position += new Vector2(velocity.X, 0);
            if (IsCollidingWithObstacle(level)) {
                Position -= new Vector2(velocity.X, 0); // Revert if hit
            }

            // Move Y
            Position += new Vector2(0, velocity.Y);
            if (IsCollidingWithObstacle(level)) {
                Position -= new Vector2(0, velocity.Y); // Revert if hit
            }
        }

        public override float GetPresence(float currentFrequency) {
            float basePresence = 0f;

            if (Behaviour == EnemyBehaviour.Stalker) {
                // The Stalker ignores the upper-bound fade out.
                // If past the anchor, they are 100% physically present.
                // If below the anchor, we fall back to the base curve so they still fade in gracefully as you approach.
                if (currentFrequency >= AnchorFrequency) {
                    basePresence = 1f;
                }
                else {
                    basePresence = base.GetPresence(currentFrequency);
                }
            }
            else {
                basePresence = base.GetPresence(currentFrequency);
            }

            if (WakeDelay <= 0f) return basePresence;

            // Scale visibility/solidity by how "awake" they are
            return basePresence * (_wakeTimer / WakeDelay);
        }

        public bool IsDangerous(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }
    }
}