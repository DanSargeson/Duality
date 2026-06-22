using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Duality.Entities;

namespace Duality.Rendering
{
    public class DualRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel; // Prototyping texture

        public DualRenderer(GraphicsDevice graphicsDevice) {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);

            // Generate the 1x1 white pixel for shape drawing
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(float currentFrequency, List<EnvironmentObject> envObjects, Player player) {
            // 1. Dynamic Background
            // Density = Dark Gray (20,20,20), Insight = Stark White
            Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, currentFrequency);
            _graphicsDevice.Clear(bgColor);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // 2. Render Environment Objects
            foreach (var obj in envObjects) {
                float presence = obj.GetPresence(currentFrequency);

                // Skip drawing entirely if the object is fully phased out
                if (presence <= 0f) continue;

                // Apply the presence value to the alpha channel
                Color drawColor = obj.BaseColor * presence;
                _spriteBatch.Draw(_pixel, obj.Bounds, drawColor);
            }

            // 3. Render Player
            // The player remains visually solid regardless of frequency, but might shift color eventually.
            _spriteBatch.Draw(_pixel, player.Bounds, Color.LimeGreen);

            _spriteBatch.End();
        }
    }
}