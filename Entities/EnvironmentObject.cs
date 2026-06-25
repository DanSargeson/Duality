using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;
using System;

namespace Duality.Entities
{

    public enum ObjectType { Obstacle, Hazard, Platform }

    public class EnvironmentObject : Entity
    {
        public Rectangle Bounds { get; set; }
      
        // The physical purpose of the object
        public ObjectType Type { get; set; }

        public EnvironmentObject(Rectangle bounds, Color color, float anchorFrequency, ObjectType objectType) {
            Bounds = bounds;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            Type = objectType;
        }

      
        // The physics/collision check
        public bool IsSolid(float currentFrequency) {
            // Objects become passable if their presence drops below a certain threshold
            return GetPresence(currentFrequency) >= 0.8f;
        }
    }
}