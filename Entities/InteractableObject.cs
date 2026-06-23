using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public class InteractableObject : Entity
    {
        public Rectangle Bounds { get; set; }
        public bool IsClosed { get; set; } = true;

        public InteractableObject(Rectangle bounds, Color color, float anchorFrequency) {
            Bounds = bounds;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
        }

        public bool IsSolid(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }

        // Inflate bounds slightly to create a "trigger zone" where the player can press Space
        public Rectangle InteractionArea => new Rectangle(Bounds.X - 10, Bounds.Y - 10, Bounds.Width + 20, Bounds.Height + 20);
    }
}