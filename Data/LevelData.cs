using System.Collections.Generic;

namespace Duality.Data
{
    // These classes exist purely to hold data during the JSON deserialization process
    public class LevelData
    {
        public string LevelName { get; set; }
        public float PlayerStartX { get; set; }
        public float PlayerStartY { get; set; }
        public List<EnvironmentObjectData> EnvironmentObjects { get; set; }
    }

    public class EnvironmentObjectData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ColorHex { get; set; }
        public float AnchorFrequency { get; set; }
        public string Type { get; set; }
        public float Range { get; set; } = 0.4f;
    }


    public class DecalDTO
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Text { get; set; }
        public string ColorHex { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; }
    }

    public class InteractableDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ColorHex { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; }
    }

}