using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Duality.Entities;

namespace Duality.Data
{
    public class WaypointDTO
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class EnemyDTO
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string ColorHex { get; set; }
        public float AnchorFrequency { get; set; }
        public float Range { get; set; }
        public List<WaypointDTO> Waypoints { get; set; }
    }

    public class LevelDTO
    {
        public string LevelName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public float PlayerStartX { get; set; }
        public float PlayerStartY { get; set; }
        public List<EnvironmentObjectDTO> EnvironmentObjects { get; set; }
        public List<EnemyDTO> Enemies { get; set; } // Added Enemies List

        public List<DecalDTO> Decals { get; set; }
        public List<InteractableDTO> Interactables { get; set; }
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
        public List<Enemy> Enemies { get; private set; } = new(); // Exposed to Game1

        public Rectangle LevelBounds { get; private set; }

        public List<Decal> Decals { get; private set; } = new();
        public List<InteractableObject> Interactables { get; private set; } = new();

        public void LoadLevel(string path) {
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<LevelDTO>(json);

            PlayerStart = new Vector2(data.PlayerStartX, data.PlayerStartY);

            LevelBounds = new Rectangle(0, 0, data.Width, data.Height);

            EnvironmentObjects.Clear();
            foreach (var obj in data.EnvironmentObjects) {
                Rectangle bounds = new Rectangle(obj.X, obj.Y, obj.Width, obj.Height);
                var color = ParseHex(obj.ColorHex);
                Enum.TryParse(obj.Type, out ObjectType type);

                EnvironmentObjects.Add(new EnvironmentObject(bounds, color, obj.AnchorFrequency, type) {
                    Range = obj.Range
                });
            }

            Enemies.Clear();
            if (data.Enemies != null) {
                foreach (var e in data.Enemies) {
                    var color = ParseHex(e.ColorHex);
                    var startPos = new Vector2(e.X, e.Y);

                    var waypoints = new List<Vector2>();
                    if (e.Waypoints != null) {
                        foreach (var wp in e.Waypoints) {
                            waypoints.Add(new Vector2(wp.X, wp.Y));
                        }
                    }

                    Enemies.Add(new Enemy(startPos, color, e.AnchorFrequency, waypoints) {
                        Range = e.Range
                    });
                }
            }

            Decals.Clear();
if (data.Decals != null) {
    foreach (var d in data.Decals) {
        Decals.Add(new Decal(new Vector2(d.X, d.Y), d.Text, ParseHex(d.ColorHex), d.AnchorFrequency) { Range = d.Range });
    }
}

Interactables.Clear();
if (data.Interactables != null) {
    foreach (var i in data.Interactables) {
        Interactables.Add(new InteractableObject(new Rectangle(i.X, i.Y, i.Width, i.Height), ParseHex(i.ColorHex), i.AnchorFrequency) { Range = i.Range });
    }
}
        }

        private Color ParseHex(string hex) {
            var r = Convert.ToByte(hex.Substring(1, 2), 16);
            var g = Convert.ToByte(hex.Substring(3, 2), 16);
            var b = Convert.ToByte(hex.Substring(5, 2), 16);
            return new Color(r, g, b);
        }
    }
}