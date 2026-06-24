using Microsoft.Xna.Framework;
using System;

namespace Duality.Mechanics
{
    public class PolarityManager
    {
        // 0.0 is pure Density, 1.0 is pure Insight
        public float CurrentFrequency { get; private set; } = 0.0f;
        public float ShiftSpeed { get; set; } = 0.5f;

        // Overload & Burnout Mechanics
        public float Strain { get; private set; } = 0.0f; // 0.0 to 1.0
        public bool IsBurntOut { get; private set; } = false;

        public float TotalStress {
            get {
                float baseTension = (float)System.Math.Pow(CurrentFrequency, 3);
                return System.Math.Max(baseTension, Strain) * CurrentFrequency;
            }
        }

        // Configuration for future upgrades
        public float MaxInsightTime { get; set; } = 3.0f; // Seconds before overload
        public float RecoveryTime { get; set; } = 4.0f;   // Seconds to recover from burnout

        // Threshold at which Strain starts building
        private const float StrainThreshold = 0.8f;

        public void TriggerDischarge() {
            if (IsBurntOut) return; // Prevent spamming

            // Forcibly push the system to the breaking point
            Strain = 1.0f;
            IsBurntOut = true;

            // Note: Your existing Update loop in PolarityManager should 
            // catch this IsBurntOut state on the next frame and naturally 
            // handle the snap back to Density and the 4-second cooldown.
        }


        public void Update(GameTime gameTime, bool shiftingToInsight, bool shiftingToDensity) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Handle Shifting
            // We ignore Insight input if the system is burnt out, and forcefully pull to Density
            if (shiftingToInsight && !IsBurntOut)
                CurrentFrequency += ShiftSpeed * deltaTime;
            if (shiftingToDensity || IsBurntOut)
                CurrentFrequency -= ShiftSpeed * deltaTime;

            CurrentFrequency = MathHelper.Clamp(CurrentFrequency, 0.0f, 1.0f);

            // 2. Handle Strain and Burnout Logic
            if (IsBurntOut) {
                // Recover over time
                Strain -= (1.0f / RecoveryTime) * deltaTime;
                if (Strain <= 0.0f) {
                    Strain = 0.0f;
                    IsBurntOut = false; // Recovered!
                }
            }
            else {
                // Build strain if in the high-frequency danger zone
                if (CurrentFrequency > StrainThreshold) {
                    Strain += (1.0f / MaxInsightTime) * deltaTime;
                    if (Strain >= 1.0f) {
                        Strain = 1.0f;
                        IsBurntOut = true; // System snaps!
                    }
                }
                else {
                    // Slowly cool down if we are below the threshold and not burnt out
                    Strain -= (1.0f / RecoveryTime) * deltaTime;
                    Strain = MathHelper.Clamp(Strain, 0.0f, 1.0f);
                }
            }
        }

        public void SetFrequency(float frequency) {
            CurrentFrequency = MathHelper.Clamp(frequency, 0.0f, 1.0f);
        }
}
}