using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public abstract class Entity
    {
        public Color BaseColor { get; set; }

        // Where does this object perfectly exist? (0.0 = Density, 1.0 = Insight)
        public float AnchorFrequency { get; set; }

        // How far from the anchor can the frequency drift before the object vanishes/loses solidity?
        public float Range { get; set; } = 0.4f;

        // Calculates how "real" the object currently is based on the global manager.
        // We make it 'virtual' so specific items (like locked doors) can override it if they need custom fade logic.
        public virtual float GetPresence(float currentFrequency) {
            float distance = Math.Abs(AnchorFrequency - currentFrequency);
            if (distance > Range) return 0f;

            // Returns a value between 0.0 (invisible) and 1.0 (fully solid)
            return 1f - (distance / Range);
        }
    }
}