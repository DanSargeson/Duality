using Microsoft.Xna.Framework;
using System;

namespace Duality.Entities
{
    public class Decal : Entity
    {
        public Vector2 Position { get; private set; }
        public string TrueText { get; private set; }
        public string[] TextVariations { get; private set; }

        public Decal(Vector2 position, string trueText, Color color, float anchorFrequency, float range) {
            Position = position;

            // Catch LDtk's invisible line breaks just like we did for Documents
            TrueText = trueText.Replace("\\n", "\n").Replace("\r\n", "\n");

            BaseColor = color;
            AnchorFrequency = anchorFrequency;
            Range = range <= 0f ? 0.4f : range;

            PrecalculateScrambledText();
        }

        private void PrecalculateScrambledText() {
            TextVariations = new string[10];
            string glyphs = "!<>-_\\\\/[]{}=+*^?#"; // The "alien" syntax from your design doc

            // Seed the randomizer with the text's hash so the same decal 
            // always scrambles the exact same way across playthroughs
            Random rand = new Random(TrueText.GetHashCode());

            for (int i = 0; i < 10; i++) {
                float corruptionLevel = i / 9f; // Scales from 0.0 to 1.0
                char[] chars = TrueText.ToCharArray();

                for (int c = 0; c < chars.Length; c++) {
                    // Don't scramble spaces or line breaks, preserves the "shape" of the sentence
                    if (chars[c] == ' ' || chars[c] == '\n') continue;

                    if (rand.NextDouble() < corruptionLevel) {
                        chars[c] = glyphs[rand.Next(glyphs.Length)];
                    }
                }

                // Store the string in memory once
                TextVariations[i] = new string(chars);
            }
        }

        public string GetCurrentText(float currentFrequency) {
            float distance = Math.Abs(AnchorFrequency - currentFrequency);

            // If we are extremely close to the anchor, show the perfect text
            if (distance < 0.05f) return TextVariations[0];

            int corruptionIndex = Math.Min((int)(distance * 10f), 9);

            return TextVariations[corruptionIndex];
        }
    }
}