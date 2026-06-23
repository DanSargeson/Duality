using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Duality.Entities;

namespace Duality.Data
{
    // Need to use the DTO container mapped from JSON
    public class LevelDTOContainer
    {
        public string LevelName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float PlayerStartX { get; set; }
        public float PlayerStartY { get; set; }
        public string NextLevel { get; set; }
        public InteractableDTO ExitZone { get; set; }

        public List<EnvironmentObjectData> EnvironmentObjects { get; set; }
        public List<EnemyDTO> Enemies { get; set; }
        public List<DecalDTO> Decals { get; set; }
        public List<InteractableDTO> Interactables { get; set; }
    }

    public class LevelManager
    {
        public Level CurrentLevel { get; private set; }

        public void LoadLevel(string path) {
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<LevelDTOContainer>(json);

            CurrentLevel = new Level {
                Name = data.LevelName,
                PlayerStart = new Vector2(data.PlayerStartX, data.PlayerStartY),
                Bounds = new Rectangle(0, 0, data.Width, data.Height),
                NextLevelPath = data.NextLevel
            };

            if (data.ExitZone != null) {
                CurrentLevel.ExitZone = new Rectangle(data.ExitZone.X, data.ExitZone.Y, data.ExitZone.Width, data.ExitZone.Height);
            }

            if (data.EnvironmentObjects != null) {
                foreach (var obj in data.EnvironmentObjects) {
                    Enum.TryParse(obj.Type, out ObjectType type);
                    CurrentLevel.EnvironmentObjects.Add(new EnvironmentObject(new Rectangle(obj.X, obj.Y, obj.Width, obj.Height), ParseHex(obj.ColorHex), obj.AnchorFrequency, type) { Range = obj.Range });
                }
            }

            if (data.Enemies != null) {
                foreach (var e in data.Enemies) {
                    var waypoints = new List<Vector2>();
                    if (e.Waypoints != null) foreach (var wp in e.Waypoints) waypoints.Add(new Vector2(wp.X, wp.Y));

                    Enum.TryParse(e.Behaviour ?? "Patrol", out EnemyBehaviour behaviour);

                    CurrentLevel.Enemies.Add(new Enemy(new Vector2(e.X, e.Y), ParseHex(e.ColorHex), e.AnchorFrequency, waypoints) {
                        Range = e.Range,
                        Behaviour = behaviour
                    });
                }
            }

            if (data.Decals != null) {
                foreach (var d in data.Decals) CurrentLevel.Decals.Add(new Decal(new Vector2(d.X, d.Y), d.Text, ParseHex(d.ColorHex), d.AnchorFrequency) { Range = d.Range });
            }

            if (data.Interactables != null) {
                foreach (var i in data.Interactables) CurrentLevel.Interactables.Add(new InteractableObject(new Rectangle(i.X, i.Y, i.Width, i.Height), ParseHex(i.ColorHex), i.AnchorFrequency) { Range = i.Range });
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