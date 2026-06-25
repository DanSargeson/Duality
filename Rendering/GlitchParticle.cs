using Microsoft.Xna.Framework;

namespace Duality.Rendering
{
    public class GlitchParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color BaseColor;
        public float Life;
        public float MaxLife;
        public int Size;

        public void Update(float deltaTime) {
            Position += Velocity * deltaTime;
            Life -= deltaTime;
        }

        // Fades out exponentially as it dies
        public float Alpha => MathHelper.Clamp(Life / MaxLife, 0f, 1f);
    }
}