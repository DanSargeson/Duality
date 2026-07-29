using Duality.Entities;
using Microsoft.Xna.Framework;

namespace Duality.Data
{
    public class SignObject : InteractableObject
    {


        public string TextContent { get; set; }

        public SignObject(Rectangle bounds, Color color, float anchorFrequency, float range, string textContent) : base(bounds, color, anchorFrequency){

            Bounds = bounds;
            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            Range = range;
            TextContent = textContent;

            // Inflate the interaction area slightly so the player doesn't 
            // have to stand pixel-perfectly on top of the document to read it.
            InteractionArea = bounds;
            InteractionArea.Inflate(15, 15);

        }
    }
}