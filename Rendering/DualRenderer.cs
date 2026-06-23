using Duality.Data;
using Duality.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Duality.Rendering
{
    public class DualRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        private Random _random;
        private SpriteFont _font;

        private RenderTarget2D _renderTarget;
        public int VirtualWidth { get; private set; } = 800;
        public int VirtualHeight { get; private set; } = 600;

        public DualRenderer(GraphicsDevice graphicsDevice, SpriteFont font) {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _font = font;
            _pixel.SetData(new[] { Color.White });
            _random = new Random();

            _renderTarget = new RenderTarget2D(_graphicsDevice, VirtualWidth, VirtualHeight);
        }

        // ADDED: List<Enemy> enemies
        public void Draw(float currentFrequency, Level level, Player player, Camera camera) {

            float frictionIntensity = (float)Math.Pow(currentFrequency, 3);
            Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, currentFrequency);
           
            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(bgColor);

            float maxShakePixels = 5.0f;
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


            // A clean, localized helper function to draw any physical block!
            // Because everything is an 'Entity', we can reuse this glitch math for all of them.
            System.Action<Entity, Rectangle> drawBlock = (entity, bounds) => {
                float presence = entity.GetPresence(currentFrequency);
                if (presence <= 0f) return;

                Color drawColor = entity.BaseColor * presence;
                if (frictionIntensity > 0.5f && _random.NextDouble() > 0.8) {
                    drawColor = _random.Next(2) == 0 ? Color.Red * presence : Color.Cyan * presence;
                }
                _spriteBatch.Draw(_pixel, bounds, drawColor);
            };


            // Render Clues (Decals)
            foreach (var decal in level.Decals) {
                float presence = decal.GetPresence(currentFrequency);
                if (presence <= 0f) continue;
                _spriteBatch.DrawString(_font, decal.Text, decal.Position, decal.BaseColor * presence);
            }

            // Render all physical blocks in 3 clean lines
            foreach (var obj in level.EnvironmentObjects) drawBlock(obj, obj.Bounds);
            foreach (var interactable in level.Interactables) drawBlock(interactable, interactable.Bounds);
            foreach (var enemy in level.Enemies) drawBlock(enemy, enemy.Bounds);

            // Render Player
            Color playerColor = Color.Lerp(Color.LimeGreen, Color.White, currentFrequency);

            if (frictionIntensity > 0.2f) {
                Rectangle ghostBounds = player.Bounds;
                ghostBounds.Inflate((int)(10 * frictionIntensity), (int)(10 * frictionIntensity));
                _spriteBatch.Draw(_pixel, ghostBounds, Color.Black * (0.3f * frictionIntensity));
            }

            _spriteBatch.Draw(_pixel, player.Bounds, playerColor);

            _spriteBatch.End();


            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Black); // The color of the letterbox bars

            // Calculate how to scale the 800x600 target to fit the current window size
            Rectangle destinationRect = CalculateDestinationRectangle();

            // PointClamp prevents blurry edges when the image scales up!
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(_renderTarget, destinationRect, Color.White);
            _spriteBatch.End();
        }


        private Rectangle CalculateDestinationRectangle() {
            Rectangle screenRect = _graphicsDevice.PresentationParameters.Bounds;

            float screenAspect = (float)screenRect.Width / screenRect.Height;
            float virtualAspect = (float)VirtualWidth / VirtualHeight;

            int width, height;
            if (screenAspect > virtualAspect) {
                // Screen is wider than virtual (pillarbox)
                height = screenRect.Height;
                width = (int)(height * virtualAspect);
            }
            else {
                // Screen is taller than virtual (letterbox)
                width = screenRect.Width;
                height = (int)(width / virtualAspect);
            }

            int x = (screenRect.Width - width) / 2;
            int y = (screenRect.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }

    }
}