using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public class LockedDoor : InteractableObject
    {
        public string Passcode { get; private set; }
        public bool IsLocked { get; set; } = true;

        public LockedDoor(Rectangle bounds, Color colour, float anchorFrequency, float range, string passcode) : base(bounds, colour, anchorFrequency){
            Bounds = bounds;
            BaseColor = colour;
            Passcode = passcode;
            AnchorFrequency = anchorFrequency;

            Passcode = passcode;
            IsClosed = true;
            Range = range <= 0f ? 0.4f : range;

            Passcode = passcode;
            IsClosed = true;

            // THE FIX: Inflate a local copy first, then assign it.
            // This guarantees the trigger zone reaches 20 pixels outside the solid wall.
            Rectangle interactArea = bounds;
            interactArea.Inflate(20, 20);
            InteractionArea = interactArea;
        }
    }
}