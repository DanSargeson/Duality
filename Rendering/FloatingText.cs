using Microsoft.Xna.Framework;

namespace Duality.Rendering
{
    public class FloatingText
    {
        public string Text { get; private set; }
        public Vector2 Position { get; private set; }
        public Color TextColor { get; private set; }

        private float _lifetime;
        private float _maxLifetime;
        private Vector2 _velocity;

        public bool IsDead => _lifetime <= 0;

        public FloatingText(string text, Vector2 startPosition, Color color, float lifetime = 1.5f) {
            Text = text;
            Position = startPosition;
            TextColor = color;
            _lifetime = lifetime;
            _maxLifetime = lifetime;

            // Drift slowly upwards
            _velocity = new Vector2(0, -20f);
        }

        public void Update(float deltaTime) {
            _lifetime -= deltaTime;
            Position += _velocity * deltaTime;
        }

        public Color GetFadedColor() {
            // Calculate a ratio from 1.0 down to 0.0
            float alphaRatio = MathHelper.Clamp(_lifetime / _maxLifetime, 0f, 1f);
            return TextColor * alphaRatio;
        }
    }
}