using System.Collections.Generic;

namespace Duality.Data
{
    public class LevelData
    {
        public string LevelName { get; set; }
        public float PlayerStartX { get; set; }
        public float PlayerStartY { get; set; }
        public string NextLevel { get; set; }
        public InteractableDTO ExitZone { get; set; }

        public List<EnvironmentObjectData> EnvironmentObjects { get; set; }
    }

    public class EnemyDTO
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string ColorHex { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; }

        public string Behaviour { get; set; } // "Patrol" or "Hunter"

        public List<WaypointDTO> Waypoints { get; set; }
    }

    public class WaypointDTO { public float X { get; set; } public float Y { get; set; } }
    public class EnvironmentObjectData { /* Existing properties */ public int X { get; set; } public int Y { get; set; } public int Width { get; set; } public int Height { get; set; } public string ColorHex { get; set; } public float AnchorFrequency { get; set; } public string Type { get; set; } public float Range { get; set; } = 0.4f; }
    public class DecalDTO { public float X { get; set; } public float Y { get; set; } public string Text { get; set; } public string ColorHex { get; set; } public float AnchorFrequency { get; set; } public float Range { get; set; } }
    public class InteractableDTO { public int X { get; set; } public int Y { get; set; } public int Width { get; set; } public int Height { get; set; } public string ColorHex { get; set; } public float AnchorFrequency { get; set; } public float Range { get; set; } }
}