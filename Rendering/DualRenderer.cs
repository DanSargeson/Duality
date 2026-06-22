using System;
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
        private Texture2D _pixel;
        private Random _random;

        public DualRenderer(GraphicsDevice graphicsDevice) {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _random = new Random();
        }

        public void Draw(float currentFrequency, List<EnvironmentObject> envObjects, Player player) {

            // 1. Calculate Friction Intensity (Exponential Curve)
            // Cubing the frequency means 0.5 frequency only applies 12% friction. 
            // But 0.9 frequency applies 72% friction. It ramps up violently at the end.
            float frictionIntensity = (float)Math.Pow(currentFrequency, 3);

            // 2. Dynamic Background
            Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, currentFrequency);
            _graphicsDevice.Clear(bgColor);

            // 3. Screen Shake Matrix
            float maxShakePixels = 6.0f;
            float currentShake = maxShakePixels * frictionIntensity;

            Vector2 shakeOffset = Vector2.Zero;
            if (currentShake > 0.1f) {
                shakeOffset = new Vector2(
                    ((float)_random.NextDouble() * 2 - 1) * currentShake,
                    ((float)_random.NextDouble() * 2 - 1) * currentShake
                );
            }

            // A Matrix allows us to offset everything drawn in this SpriteBatch at once
            Matrix cameraTransform = Matrix.CreateTranslation(new Vector3(shakeOffset, 0));

            // Start drawing, passing in the Matrix
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: cameraTransform);

            // 4. Render Environment
            foreach (var obj in envObjects) {
                float presence = obj.GetPresence(currentFrequency);
                if (presence <= 0f) continue;

                Color drawColor = obj.BaseColor * presence;

                // Glitch Effect: If friction is high, randomly shift the color to pure red or blue for a single frame
                if (frictionIntensity > 0.5f && _random.NextDouble() > 0.8) {
                    drawColor = _random.Next(2) == 0 ? Color.Red * presence : Color.Cyan * presence;
                }

                // Render the main object
                _spriteBatch.Draw(_pixel, obj.Bounds, drawColor);
            }

            // 5. Render Player
            // The player color interpolates from a solid green to a harsh, unstable white
            Color playerColor = Color.Lerp(Color.LimeGreen, Color.White, currentFrequency);

            // Player Shadow/Ghosting (draws a larger, fainter box behind the player at high friction)
            if (frictionIntensity > 0.2f) {
                Rectangle ghostBounds = player.Bounds;
                ghostBounds.Inflate((int)(10 * frictionIntensity), (int)(10 * frictionIntensity));
                _spriteBatch.Draw(_pixel, ghostBounds, Color.Black * (0.3f * frictionIntensity));
            }

            _spriteBatch.Draw(_pixel, player.Bounds, playerColor);

            _spriteBatch.End();
        }
    }
}