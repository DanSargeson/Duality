using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public class Decal : Entity
    {
        public Vector2 Position { get; set; }
        public string Text { get; set; }
        
        public Decal(Vector2 position, string text, Color color, float anchorFrequency) {
            Position = position;
            Text = text;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
        }
    }
}