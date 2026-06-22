using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Duality.Entities;

namespace Duality.Mechanics
{
    // Simplified DTOs to keep LevelManager clean
    public class LevelDTO
    {
        public string LevelName { get; set; }
        public float PlayerStartX { get; set; }
        public float PlayerStartY { get; set; }
        public List<EnvironmentObjectDTO> EnvironmentObjects { get; set; }
    }

    public class EnvironmentObjectDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ColorHex { get; set; }
        public float AnchorFrequency { get; set; }
        public string Type { get; set; }
        public float Range { get; set; }
    }

    public class LevelManager
    {
        public Vector2 PlayerStart { get; private set; }
        public List<EnvironmentObject> EnvironmentObjects { get; private set; } = new();

        public void LoadLevel(string path) {
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<LevelDTO>(json);

            // Construct the Vector2 from the flat floats
            PlayerStart = new Vector2(data.PlayerStartX, data.PlayerStartY);

            EnvironmentObjects.Clear();

            foreach (var obj in data.EnvironmentObjects) {
                // Construct the Rectangle from the flat ints
                Rectangle bounds = new Rectangle(obj.X, obj.Y, obj.Width, obj.Height);
                var color = ParseHex(obj.ColorHex);

                Enum.TryParse(obj.Type, out ObjectType type);

                EnvironmentObjects.Add(new EnvironmentObject(bounds, color, obj.AnchorFrequency, type) {
                    Range = obj.Range
                });
            }
        }

        private Color ParseHex(string hex) {
            // Basic hex parser
            var r = Convert.ToByte(hex.Substring(1, 2), 16);
            var g = Convert.ToByte(hex.Substring(3, 2), 16);
            var b = Convert.ToByte(hex.Substring(5, 2), 16);
            return new Color(r, g, b);
        }
    }
}