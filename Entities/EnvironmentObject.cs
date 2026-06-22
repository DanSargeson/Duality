using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;
using System;

namespace Duality.Entities
{

    public enum ObjectType { Obstacle, Hazard, Platform }

    public class EnvironmentObject
    {
        public Rectangle Bounds { get; set; }
        public Color BaseColor { get; set; }

        // Where does this object perfectly exist? (0.0 = Density, 1.0 = Insight)
        public float AnchorFrequency { get; set; }

        // How far from the anchor can the frequency drift before the object vanishes/loses solidity?
        public float Range { get; set; } = 0.4f;

        // The physical purpose of the object
        public ObjectType Type { get; set; }

        public EnvironmentObject(Rectangle bounds, Color color, float anchorFrequency, ObjectType objectType) {
            Bounds = bounds;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            Type = objectType;
        }

        // Calculates how "real" the object currently is based on the global manager
        public float GetPresence(float currentFrequency) {
            float distance = Math.Abs(AnchorFrequency - currentFrequency);
            if (distance > Range) return 0f;

            // Returns a value between 0.0 (invisible) and 1.0 (fully solid)
            return 1f - (distance / Range);
        }

        // The physics/collision check
        public bool IsSolid(float currentFrequency) {
            // Objects become passable if their presence drops below a certain threshold
            return GetPresence(currentFrequency) > 0.5f;
        }
    }
}