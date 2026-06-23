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

        public void Draw(GameTime gameTime, PolarityManager polarity, Level level, Player player, Camera camera) {

            float currentFrequency = polarity.CurrentFrequency;
            float frictionIntensity = (float)Math.Pow(currentFrequency, 3);
            Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, currentFrequency);

            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(bgColor);

            // Epilepsy-friendly update: Removed violent position shake.
            Matrix cameraTransform = camera.GetTransform(Vector2.Zero);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: cameraTransform);

            Action<Entity, Rectangle> drawBlock = (entity, bounds) => {
                float presence = entity.GetPresence(currentFrequency);
                if (presence <= 0f) return;

                Color drawColor = entity.BaseColor * presence;
                if (frictionIntensity > 0.5f && _random.NextDouble() > 0.8) {
                    drawColor = _random.Next(2) == 0 ? Color.Red * presence : Color.Cyan * presence;
                }
                _spriteBatch.Draw(_pixel, bounds, drawColor);
            };

            // Draw Exit Zone (Pulsing Gold)
            if (level.ExitZone != Rectangle.Empty) {
                float pulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5) * 0.25f + 0.5f;
                _spriteBatch.Draw(_pixel, level.ExitZone, Color.Gold * pulse);
            }

            foreach (var decal in level.Decals) {
                float presence = decal.GetPresence(currentFrequency);
                if (presence > 0f) _spriteBatch.DrawString(_font, decal.Text, decal.Position, decal.BaseColor * presence);
            }

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

            // Draw Full Screen Static/Distortion Effect
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (polarity.Strain > 0.1f) {
                int staticLines = (int)(100 * polarity.Strain); // More strain = more static
                for (int i = 0; i < staticLines; i++) {
                    int y = _random.Next(VirtualHeight);
                    int h = _random.Next(1, 4);
                    Color staticColor = _random.Next(2) == 0 ? Color.Black : Color.White;
                    _spriteBatch.Draw(_pixel, new Rectangle(0, y, VirtualWidth, h), staticColor * (0.15f * polarity.Strain));
                }
            }

            // Draw UI: Strain Bar
            if (polarity.Strain > 0) {
                Rectangle strainBg = new Rectangle(VirtualWidth / 2 - 100, 20, 200, 15);
                Rectangle strainFg = new Rectangle(VirtualWidth / 2 - 100, 20, (int)(200 * polarity.Strain), 15);
                _spriteBatch.Draw(_pixel, strainBg, Color.DarkRed * 0.5f);
                _spriteBatch.Draw(_pixel, strainFg, polarity.IsBurntOut ? Color.White : Color.Red);

                string txt = polarity.IsBurntOut ? "SYSTEM BURNOUT" : "SYSTEM STRAIN";
                Vector2 size = _font.MeasureString(txt);
                _spriteBatch.DrawString(_font, txt, new Vector2(VirtualWidth / 2 - size.X / 2, 40), polarity.IsBurntOut ? Color.Red : Color.White);
            }
            _spriteBatch.End();

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