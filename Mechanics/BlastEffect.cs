using Microsoft.Xna.Framework;

namespace Duality.Rendering // Or Duality.Mechanics
{
    public class BlastEffect
    {
        public Vector2 Position { get; private set; }
        public float MaxRadius { get; private set; }
        public float CurrentRadius { get; private set; }
        public float Alpha { get; private set; }

        public bool IsActive => Alpha > 0f;

        private float _expansionSpeed;

        public BlastEffect(Vector2 position, float maxRadius) {
            Position = position;
            MaxRadius = maxRadius;
            CurrentRadius = 0f;
            Alpha = 1f;

            // Reaches max radius in exactly 0.25 seconds for a punchy, violent snap
            _expansionSpeed = maxRadius * 4f;
        }

        public void Update(float deltaTime) {
            if (!IsActive) return;

            CurrentRadius += _expansionSpeed * deltaTime;

            // Fade out exponentially as it gets larger
            Alpha = 1f - (CurrentRadius / MaxRadius);

            if (CurrentRadius >= MaxRadius) {
                Alpha = 0f;
            }
        }
    }
}