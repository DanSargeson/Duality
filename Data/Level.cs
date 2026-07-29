using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Duality.Entities;

namespace Duality.Data
{
    public class Level
    {
        public string Name { get; set; }
        public Rectangle Bounds { get; set; }
        public Vector2 PlayerStart { get; set; }
        public string NextLevelPath { get; set; }
        public Rectangle ExitZone { get; set; }

        public string OnLoadLog { get; set; }

        public string LevelId { get; set; }

        public List<DocumentObject> Documents { get; set; } = new List<DocumentObject>();

        public List<SignObject> Signs { get; set; } = new List<SignObject>();

        public List<UpgradeNode> UpgradeNodes { get; set; } = new List<UpgradeNode>();

        public List<EnvironmentObject> EnvironmentObjects { get; set; } = new List<EnvironmentObject>();
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public List<InteractableObject> Interactables { get; set; } = new List<InteractableObject>();

        public List<LockedDoor> LockedDoors { get; set; } = new List<LockedDoor>();
        public List<Decal> Decals { get; set; } = new List<Decal>();
    }
}