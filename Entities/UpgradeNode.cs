using Duality.Entities;
using Microsoft.Xna.Framework;

public class UpgradeNode : InteractableObject
{
    public string UpgradeId { get; private set; }
    public bool IsCollected { get; set; } = false;

    public UpgradeNode(Rectangle bounds, Color color, float AnchorFrequency, float Range, string upgradeId) : base(bounds, color, AnchorFrequency) {

        base.Range = Range;
        UpgradeId = upgradeId;
        InteractionArea = bounds;
        InteractionArea.Inflate(15, 15);
    }
}