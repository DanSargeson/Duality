using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public class Decal
    {
        public Vector2 Position { get; set; }
        public string Text { get; set; }
        public Color BaseColor { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; } = 0.4f;

        public Decal(Vector2 position, string text, Color color, float anchorFrequency) {
            Position = position;
            Text = text;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
        }

        public float GetPresence(float currentFrequency) {
            float distance = Math.Abs(AnchorFrequency - currentFrequency);
            if (distance > Range) return 0f;
            return 1f - (distance / Range);
        }
    }
}