using Microsoft.Xna.Framework;

namespace Duality.Entities
{
    public class DocumentObject : InteractableObject
    {
        public string TextContent { get; private set; }

        public DocumentObject(Rectangle bounds, Color color, float anchorFrequency, float range, string textContent) : base(bounds, color, anchorFrequency) {
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