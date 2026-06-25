using Microsoft.Xna.Framework;
using System;

namespace Duality.Mechanics
{
    public class PolarityManager
    {
        // 0.0 is pure Density, 1.0 is pure Insight
        public float CurrentFrequency { get; set; } = 0.0f;


        // Pushing into Insight takes 4 full seconds. 
        // This gives the heartbeat a massive runway to accelerate.
        public float ShiftInSpeed { get; set; } = 0.20f;

        // Escaping back to Density takes 1.5 seconds. 
        // Fast enough to feel like a panic reflex.
        public float ShiftOutSpeed { get; set; } = 0.66f;

        // Overload & Burnout Mechanics
        public float Strain { get; set; } = 0.0f; // 0.0 to 1.0
        public bool IsBurntOut { get; private set; } = false;

        public float TotalStress {
            get {
                // Shifting into Insight provides the first 40% of the panic
                float baseTension = CurrentFrequency * 0.4f;

                // The actual system melting down provides the remaining 60%
                float strainTension = Strain * 0.6f;

                return baseTension + strainTension;
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
                CurrentFrequency += ShiftInSpeed * deltaTime;
            if (shiftingToDensity || IsBurntOut)
                CurrentFrequency -= ShiftOutSpeed * deltaTime;

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