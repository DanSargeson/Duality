using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public class InteractableObject : Entity
    {
        public Rectangle Bounds { get; set; }
        public bool IsClosed { get; set; } = true;

        public Rectangle InteractionArea { get; set; }

        public InteractableObject(Rectangle bounds, Color color, float anchorFrequency) {
            Bounds = bounds;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            InteractionArea = new Rectangle(Bounds.X - 10, Bounds.Y - 10, Bounds.Width + 20, Bounds.Height + 20);
        }

        public bool IsSolid(float currentFrequency) {
            return GetPresence(currentFrequency) > 0.5f;
        }
    }
}