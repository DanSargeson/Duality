using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Duality.Entities;

namespace Duality.Data
{
    public class LevelManager
    {
        public Level CurrentLevel { get; private set; }

        public void LoadLDtkLevel(string path, string levelName) {
            string json = File.ReadAllText(path);

            CurrentLevel = new Level {
                Name = levelName,
                EnvironmentObjects = new List<EnvironmentObject>(),
                Enemies = new List<Enemy>(),
                Decals = new List<Decal>(),
                Interactables = new List<InteractableObject>(),
                Documents = new List<DocumentObject>(),
                LockedDoors = new List<LockedDoor>(),
                UpgradeNodes = new List<UpgradeNode>()
            };

            // Parse the LDtk JSON dynamically
            using (JsonDocument doc = JsonDocument.Parse(json)) {
                JsonElement root = doc.RootElement;

                // 1. Find the correct level
                JsonElement targetLevel = default;
                bool levelFound = false;

                foreach (var level in root.GetProperty("levels").EnumerateArray()) {
                    if (level.GetProperty("identifier").GetString() == levelName) {
                        targetLevel = level;
                        levelFound = true;  
                        break;
                    }
                }

                if (!levelFound) throw new Exception($"Level '{levelName}' not found in LDtk file.");

                if (targetLevel.TryGetProperty("fieldInstances", out JsonElement fi)) {
                    System.Diagnostics.Debug.WriteLine($"--- CHECKING LEVEL: {levelName} ---");
                    foreach (var f in fi.EnumerateArray()) {
                        string foundName = f.GetProperty("__identifier").GetString();
                        System.Diagnostics.Debug.WriteLine($"FOUND FIELD: '{foundName}'");
                    }
                }
                else {
                    System.Diagnostics.Debug.WriteLine($"--- NO FIELDS FOUND ON LEVEL: {levelName} ---");
                }

                CurrentLevel.OnLoadLog = GetStringField(targetLevel, "OnLoadLog");
                string safePrint = CurrentLevel.OnLoadLog.Replace("\n", "\\n").Replace("\r", "");
                System.Diagnostics.Debug.WriteLine($"Content of CurrentLevel.OnLoadLog: [{safePrint}]");

                // LDtk's intrinsic instance ID is always strictly lowercase "iid"
                CurrentLevel.LevelId = targetLevel.GetProperty("iid").GetString();

                // Set level bounds
                CurrentLevel.Bounds = new Rectangle(0, 0,
                    targetLevel.GetProperty("pxWid").GetInt32(),
                    targetLevel.GetProperty("pxHei").GetInt32());

                // 2. Find the "Entities" layer
                foreach (var layer in targetLevel.GetProperty("layerInstances").EnumerateArray()) {
                    if (layer.GetProperty("__identifier").GetString() == "Entities") {

                        // 3. Loop through all entities placed in this layer
                        foreach (var entity in layer.GetProperty("entityInstances").EnumerateArray()) {
                            ParseEntity(entity);
                        }
                    }
                }
            }
        }

        private void ParseEntity(JsonElement entity) {
            string type = entity.GetProperty("__identifier").GetString();

            // LDtk stores pixel coordinates in an array: [x, y]
            int x = entity.GetProperty("px")[0].GetInt32();
            int y = entity.GetProperty("px")[1].GetInt32();
            int width = entity.GetProperty("width").GetInt32();
            int height = entity.GetProperty("height").GetInt32();
            Rectangle bounds = new Rectangle(x, y, width, height);


            var fields = new Dictionary<string, JsonElement>();
            if (entity.TryGetProperty("fieldInstances", out JsonElement fieldInstances)) {
                foreach (var field in fieldInstances.EnumerateArray()) {
                    fields[field.GetProperty("__identifier").GetString()] = field.GetProperty("__value");
                }
            }


            switch (type) {
                case "PlayerStart":
                    CurrentLevel.PlayerStart = new Vector2(x, y);
                    break;

                case "ExitZone":
                    CurrentLevel.ExitZone = bounds;
                    CurrentLevel.NextLevelPath = GetStringField(entity, "NextLevel");
                    break;

                // Group all custom LDtk obstacle types into the same EnvironmentObject list
                case "InsightObstacle":
                case "DensityObstacle":
                case "Hazard":
                case "Platform":
                    Enum.TryParse(GetStringField(entity, "ObjectType"), out ObjectType objType);
                    CurrentLevel.EnvironmentObjects.Add(new EnvironmentObject(
                        bounds,
                        ParseHex(GetStringField(entity, "ColourHex")), // Updated to your spelling
                        GetFloatField(entity, "AnchorFrequency"),
                        objType) { Range = GetFloatField(entity, "Range") }
                    );
                    break;
                case "PatrolEnemy":
                case "HunterEnemy":
                case "StalkerEnemy":
                case "Enemy":
                    Enum.TryParse(GetStringField(entity, "EnemyBehaviour"), out EnemyBehaviour behaviour);

                    // Extract the Waypoints! (16 is your default LDtk grid size)
                    List<Vector2> waypoints = GetWaypoints(entity, "Point", 16);

                    CurrentLevel.Enemies.Add(new Enemy(
                        new Vector2(x, y),
                        ParseHex(GetStringField(entity, "ColourHex")),
                        GetFloatField(entity, "AnchorFrequency"),
                        waypoints) { Range = GetFloatField(entity, "Range"), Behaviour = behaviour, WakeDelay = GetFloatField(entity, "WakeDelay" )}
                    );
                    break;
                case "Document":
                    CurrentLevel.Documents.Add(new DocumentObject(
                        bounds,
                        ParseHex(GetStringField(entity, "ColourHex")),
                        GetFloatField(entity, "AnchorFrequency"),
                        GetFloatField(entity, "Range"),
                        GetStringField(entity, "TextContent")
                    ));
                    break;
                case "Decal":
                    CurrentLevel.Decals.Add(new Decal(
                        new Vector2(x, y), // Position directly from LDtk
                        GetStringField(entity, "TextContent"),
                        ParseHex(GetStringField(entity, "ColourHex")),
                        GetFloatField(entity, "AnchorFrequency"),
                        GetFloatField(entity, "Range")
                    ));
                    break;
                case "Upgrade":
                    CurrentLevel.UpgradeNodes.Add(new UpgradeNode(
                        bounds,
                        ParseHex(GetStringField(entity, "ColourHex")),
                        GetFloatField(entity, "AnchorFrequency"),
                        GetFloatField(entity, "Range"),
                        GetStringField(entity, "UpgradeId"), 
                        GetStringField(entity, "TextContext") 
                    ));
                    break;
                case "LockedDoor":
                    CurrentLevel.LockedDoors.Add(new LockedDoor(
                        bounds,
                        ParseHex(GetStringField(entity, "ColourHex")),
                        GetFloatField(entity, "AnchorFrequency"),
                        GetFloatField(entity, "Range"),
                        GetStringField(entity, "Passcode") // The multi-line LDtk field
                    ));
                    break;
            }
        }

        // HELPER METHOD
        private List<Vector2> GetWaypoints(JsonElement entity, string fieldName, int gridSize) {
            var waypoints = new List<Vector2>();
            foreach (var field in entity.GetProperty("fieldInstances").EnumerateArray()) {
                if (field.GetProperty("__identifier").GetString() == fieldName) {
                    var val = field.GetProperty("__value");
                    if (val.ValueKind == JsonValueKind.Array) {
                        foreach (var pt in val.EnumerateArray()) {
                            // LDtk stores points in Grid coordinates (cx, cy). Multiply by grid size for world pixels!
                            float pxX = pt.GetProperty("cx").GetInt32() * gridSize;
                            float pxY = pt.GetProperty("cy").GetInt32() * gridSize;
                            waypoints.Add(new Vector2(pxX, pxY));
                        }
                    }
                }
            }
            return waypoints;
        }

        // --- Helper methods to extract Custom Fields from LDtk Entities ---

        private string GetStringField(JsonElement entity, string fieldName) {
            if (entity.TryGetProperty("fieldInstances", out JsonElement fieldInstances)) {
                foreach (var field in fieldInstances.EnumerateArray()) {
                    if (field.GetProperty("__identifier").GetString() == fieldName) {
                        var value = field.GetProperty("__value");
                        // Safety Check: If the field is empty in LDtk, return an empty string, don't crash!
                        return value.ValueKind == JsonValueKind.Null ? "" : value.GetString();
                    }
                }
            }
            return ""; // Default if the field doesn't exist at all
        }

        private float GetFloatField(JsonElement entity, string fieldName, float defaultValue = 0f) {
            if (entity.TryGetProperty("fieldInstances", out JsonElement fieldInstances)) {
                foreach (var field in fieldInstances.EnumerateArray()) {
                    if (field.GetProperty("__identifier").GetString() == fieldName) {
                        var value = field.GetProperty("__value");
                        return value.ValueKind == JsonValueKind.Null ? defaultValue : value.GetSingle();
                    }
                }
            }

            // THE TRACKER ALARM: Tells you exactly WHERE the failing entity is in the world
            string entityType = entity.GetProperty("__identifier").GetString();
            int x = entity.GetProperty("px")[0].GetInt32();
            int y = entity.GetProperty("px")[1].GetInt32();

            System.Diagnostics.Debug.WriteLine($"[WARNING] {entityType} at X:{x}, Y:{y} is missing '{fieldName}'. Using default: {defaultValue}");

            return defaultValue;
        }

        private Color ParseHex(string hex) {
            if (string.IsNullOrEmpty(hex) || hex.Length < 7) return Color.Magenta; // Fallback error color
            var r = Convert.ToByte(hex.Substring(1, 2), 16);
            var g = Convert.ToByte(hex.Substring(3, 2), 16);
            var b = Convert.ToByte(hex.Substring(5, 2), 16);
            return new Color(r, g, b);
        }
    }
}