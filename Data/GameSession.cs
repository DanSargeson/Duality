using System.Collections.Generic;

namespace Duality.Data
{
    public class GameSession
    {
        // Stores all collected CCTV logs, reports, and lore entries
        public HashSet<string> CollectedDocuments { get; private set; }

        // Keeping the player's system strain between rooms maintains tension.
        // Walking through a door shouldn't magically cure burnout.
        public float CarriedStrain { get; set; } = 0f;
        public float CarriedFrequency { get; set; } = 0f;

        public HashSet<string> UnlockedUpgrades { get; private set; }

        public GameSession() {
            CollectedDocuments = new HashSet<string>();
            UnlockedUpgrades = new HashSet<string>();
        }

        public void CollectDocument(string textContent) {
            // HashSet automatically ignores duplicates, but we check anyway for safety
            if (!string.IsNullOrWhiteSpace(textContent)) {
                CollectedDocuments.Add(textContent);
            }
        }


        public void UnlockUpgrade(string upgradeId) {
            if (!string.IsNullOrEmpty(upgradeId)) {
                UnlockedUpgrades.Add(upgradeId);
            }
        }
    }
}