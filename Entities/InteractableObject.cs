using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public class InteractableObject
    {
        public Rectangle Bounds { get; set; }
        public Color BaseColor { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; } = 0.4f;

        public bool IsClosed { get; set; } = true;

        public InteractableObject(Rectangle bounds, Color color, float anchorFrequency) {
            Bounds = bounds;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
        }

        public float GetPresence(float currentFrequency) {
            if (!IsClosed) return 0f; // Disappears entirely when opened
            float distance = Math.Abs(AnchorFrequency - currentFrequency);
            if (distance > Range) return 0f;
            return 1f - (distance / Range);
        }

        public bool IsSolid(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }

        // Inflate bounds slightly to create a "trigger zone" where the player can press Space
        public Rectangle InteractionArea => new Rectangle(Bounds.X - 10, Bounds.Y - 10, Bounds.Width + 20, Bounds.Height + 20);
    }
}