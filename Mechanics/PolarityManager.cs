using Microsoft.Xna.Framework;

namespace Duality.Mechanics
{
    public class PolarityManager
    {
        // 0.0 is pure Density, 1.0 is pure Insight
        public float CurrentFrequency { get; private set; } = 0.0f;
        public float ShiftSpeed { get; set; } = 1.5f;

        public void Update(GameTime gameTime, bool shiftingToInsight, bool shiftingToDensity) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (shiftingToInsight)
                CurrentFrequency += ShiftSpeed * deltaTime;
            if (shiftingToDensity)
                CurrentFrequency -= ShiftSpeed * deltaTime;

            CurrentFrequency = MathHelper.Clamp(CurrentFrequency, 0.0f, 1.0f);
        }
    }
}