using Duality.Data;
using Duality.Entities;
using Duality.Mechanics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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

        public void Unload() {
            _renderTarget?.Dispose();
        }

        // MODIFIED: We pass in the PolarityManager instead of just the float
        public void Draw(GameTime gameTime, Mechanics.PolarityManager polarityManager, Data.Level level, Entities.Player player, Camera camera) {
            float currentFrequency = polarityManager.CurrentFrequency;

            // 1. Calculate Unified Stress
            // Frequency builds base tension. Strain builds the violent peak before burnout.
            float baseTension = (float)Math.Pow(currentFrequency, 3);
            float totalStress = polarityManager.TotalStress;

            Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, currentFrequency);

            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(bgColor);

            // 2. The Shake Effect (Driven by totalStress)
            float maxShakePixels = 4.0f;
            float currentShake = maxShakePixels * totalStress;

            Vector2 shakeOffset = Vector2.Zero;
            if (currentShake > 0.1f) {
                shakeOffset = new Vector2(
                    ((float)_random.NextDouble() * 2 - 1) * currentShake,
                    ((float)_random.NextDouble() * 2 - 1) * currentShake
                );
            }

            Matrix cameraTransform = camera.GetTransform(shakeOffset);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: cameraTransform);

            // 3. The Entity Drawing (Flashing REMOVED)
            System.Action<Entities.Entity, Rectangle> drawBlock = (entity, bounds) => {
                float presence = entity.GetPresence(currentFrequency);
                if (presence <= 0f) return;

                // The Red/Cyan flashing logic has been completely removed!
                // It now just smoothly scales the alpha of the object's base color.
                _spriteBatch.Draw(_pixel, bounds, entity.BaseColor * presence);
            };

            if (level.ExitZone != Rectangle.Empty) {
                float pulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5) * 0.25f + 0.5f;
                _spriteBatch.Draw(_pixel, level.ExitZone, Color.Gold * pulse);
            }

            // Render Clues (Decals)
            foreach (var decal in level.Decals) {
                float presence = decal.GetPresence(currentFrequency);
                if (presence <= 0f) continue;
                _spriteBatch.DrawString(_font, decal.Text, decal.Position, decal.BaseColor * presence);
            }

            // Render Physical Blocks
            foreach (var obj in level.EnvironmentObjects) drawBlock(obj, obj.Bounds);
            foreach (var interactable in level.Interactables) drawBlock(interactable, interactable.Bounds);
            foreach (var enemy in level.Enemies) drawBlock(enemy, enemy.Bounds);

            // Render Player
            Color playerColor = Color.Lerp(Color.LimeGreen, Color.White, currentFrequency);
            if (totalStress > 0.2f) {
                Rectangle ghostBounds = player.Bounds;
                ghostBounds.Inflate((int)(10 * totalStress), (int)(10 * totalStress));
                _spriteBatch.Draw(_pixel, ghostBounds, Color.Gray * (0.3f * totalStress));
            }
            _spriteBatch.Draw(_pixel, player.Bounds, playerColor);

            _spriteBatch.End();

            // 4. Procedural TV Static Overlay
            if (totalStress > 0.1f) {
                // We begin a NEW batch without the camera transform so the static sticks to the "glass" of the screen
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

                // Generate hundreds of tiny rectangles to simulate film grain/static.
                // The amount of static scales up perfectly with the system stress.
                int staticParticles = (int)(1000 * totalStress);

                for (int i = 0; i < staticParticles; i++) {
                    int x = _random.Next(VirtualWidth);
                    int y = _random.Next(VirtualHeight);
                    int size = _random.Next(1, 4);

                    // Opacity peaks just before burnout
                    float alpha = (float)_random.NextDouble() * totalStress * 0.35f;
                    Color noiseColor = _random.Next(2) == 0 ? Color.Green : Color.Red;

                    _spriteBatch.Draw(_pixel, new Rectangle(x, y, size, size), noiseColor * alpha);
                }
                _spriteBatch.End();
            }

            if (polarityManager.Strain > 0) {
                // Start a new batch for UI so it ignores the camera transform and sticks to the screen
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                Rectangle strainBg = new Rectangle(VirtualWidth / 2 - 100, 20, 200, 15);
                Rectangle strainFg = new Rectangle(VirtualWidth / 2 - 100, 20, (int)(200 * polarityManager.Strain), 15);

                _spriteBatch.Draw(_pixel, strainBg, Color.DarkRed * 0.5f);
                _spriteBatch.Draw(_pixel, strainFg, polarityManager.IsBurntOut ? Color.White : Color.Red);

                string txt = polarityManager.IsBurntOut ? "SYSTEM BURNOUT" : "SYSTEM STRAIN";
                Vector2 size = _font.MeasureString(txt);
                _spriteBatch.DrawString(_font, txt, new Vector2(VirtualWidth / 2 - size.X / 2, 40), polarityManager.IsBurntOut ? Color.Red : Color.White);

                _spriteBatch.End();
            }


            // --- DRAW TO SCREEN ---
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Black);

            Rectangle destinationRect = CalculateDestinationRectangle();
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
                height = screenRect.Height; width = (int)(height * virtualAspect);
            }
            else {
                width = screenRect.Width; height = (int)(width / virtualAspect);
            }
            return new Rectangle((screenRect.Width - width) / 2, (screenRect.Height - height) / 2, width, height);
        }
    }
}