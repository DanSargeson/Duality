using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Duality.Rendering
{
    public class FloatingTextManager
    {
        private List<FloatingText> _activeTexts = new List<FloatingText>();

        public void Add(string text, Vector2 position, Color color) {
            _activeTexts.Add(new FloatingText(text, position, color));
        }

        public void Update(float deltaTime) {
            // Iterate backwards so we can safely remove dead text without breaking the loop
            for (int i = _activeTexts.Count - 1; i >= 0; i--) {
                _activeTexts[i].Update(deltaTime);

                if (_activeTexts[i].IsDead) {
                    _activeTexts.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font) {
            foreach (var floatingText in _activeTexts) {
                // Draw a harsh black shadow behind the text for readability
                Vector2 shadowOffset = new Vector2(1, 1);
                spriteBatch.DrawString(font, floatingText.Text, floatingText.Position + shadowOffset, Color.Black * (floatingText.GetFadedColor().A / 255f));

                // Draw the actual text
                spriteBatch.DrawString(font, floatingText.Text, floatingText.Position, floatingText.GetFadedColor());
            }
        }
    }
}