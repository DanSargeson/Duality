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
        private SpriteFont _font;

        public DualRenderer(GraphicsDevice graphicsDevice, SpriteFont font) {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _font = font;
            _pixel.SetData(new[] { Color.White });
            _random = new Random();
        }

        // ADDED: List<Enemy> enemies
        public void Draw(float currentFrequency, List<EnvironmentObject> envObjects, List<Enemy> enemies, List<InteractableObject> interactables, List<Decal> decals, Player player, Camera camera) {

            float frictionIntensity = (float)Math.Pow(currentFrequency, 3);
            Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, currentFrequency);
            _graphicsDevice.Clear(bgColor);

            float maxShakePixels = 6.0f;
            float currentShake = maxShakePixels * frictionIntensity;

            Vector2 shakeOffset = Vector2.Zero;
            if (currentShake > 0.1f) {
                shakeOffset = new Vector2(
                    ((float)_random.NextDouble() * 2 - 1) * currentShake,
                    ((float)_random.NextDouble() * 2 - 1) * currentShake
                );
            }

            Matrix cameraTransform = camera.GetTransform(shakeOffset);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: cameraTransform);



            // Render Clues (Decals)
            foreach (var decal in decals) {
                float presence = decal.GetPresence(currentFrequency);
                if (presence <= 0f) continue;

                // Draw the text
                _spriteBatch.DrawString(_font, decal.Text, decal.Position, decal.BaseColor * presence);
            }

            // Render Interactables
            foreach (var interactable in interactables) {
                float presence = interactable.GetPresence(currentFrequency);
                if (presence <= 0f) continue;

                Color drawColor = interactable.BaseColor * presence;
                if (frictionIntensity > 0.5f && _random.NextDouble() > 0.8) {
                    drawColor = _random.Next(2) == 0 ? Color.Red * presence : Color.Cyan * presence;
                }
                _spriteBatch.Draw(_pixel, interactable.Bounds, drawColor);
            }


            // Render Environment
            foreach (var obj in envObjects) {
                float presence = obj.GetPresence(currentFrequency);
                if (presence <= 0f) continue;

                Color drawColor = obj.BaseColor * presence;
                if (frictionIntensity > 0.5f && _random.NextDouble() > 0.8) {
                    drawColor = _random.Next(2) == 0 ? Color.Red * presence : Color.Cyan * presence;
                }
                _spriteBatch.Draw(_pixel, obj.Bounds, drawColor);
            }

            // Render Enemies
            foreach (var enemy in enemies) {
                float presence = enemy.GetPresence(currentFrequency);

                // ADDED: Skip drawing the enemy entirely if its presence is 0
                if (presence <= 0f) continue;

                // Enemies pulse slightly to make them look alive/dangerous
                Color drawColor = enemy.BaseColor * presence;
                _spriteBatch.Draw(_pixel, enemy.Bounds, drawColor);
            }

            // Render Player
            Color playerColor = Color.Lerp(Color.LimeGreen, Color.White, currentFrequency);

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