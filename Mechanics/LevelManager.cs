using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Duality.Entities;
using Duality.Data;

namespace Duality.Mechanics
{
    public class LevelManager
    {
        public Vector2 PlayerStart { get; private set; }
        public List<EnvironmentObject> EnvironmentObjects { get; private set; }

        public void LoadLevel(string filePath) {
            // 1. Read and Deserialize
            string jsonString = File.ReadAllText(filePath);
            LevelData data = JsonSerializer.Deserialize<LevelData>(jsonString);

            // 2. Set Player State
            PlayerStart = new Vector2(data.PlayerStartX, data.PlayerStartY);

            // 3. Construct Game Entities
            EnvironmentObjects = new List<EnvironmentObject>();

            foreach (var objData in data.EnvironmentObjects) {
                Rectangle bounds = new Rectangle(objData.X, objData.Y, objData.Width, objData.Height);
                Color color = ParseHexColor(objData.ColorHex);

                // Safely parse the enum string from the JSON into the actual ObjectType enum
                if (!Enum.TryParse(objData.Type, out ObjectType objType)) {
                    objType = ObjectType.Obstacle; // Fallback default
                }

                var envObj = new EnvironmentObject(bounds, color, objData.AnchorFrequency, objType) {
                    Range = objData.Range
                };

                EnvironmentObjects.Add(envObj);
            }
        }

        // Utility to convert "#FF0000" to a MonoGame Color object
        private Color ParseHexColor(string hex) {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6) return Color.White;

            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);

            return new Color(r, g, b);
        }
    }
}