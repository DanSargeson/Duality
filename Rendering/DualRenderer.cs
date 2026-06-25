using Duality.Data;
using Duality.Entities;
using Duality.Mechanics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

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
        private Texture2D _ringTexture;

        private Effect _crtEffect;
        private float _shaderTime;
        private float _shaderIntensity;

        public int VirtualWidth { get; private set; } = 800;
        public int VirtualHeight { get; private set; } = 600;

        public DualRenderer(GraphicsDevice graphicsDevice, SpriteFont font, ContentManager content) {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _font = font;
            _pixel.SetData(new[] { Color.White });
            _random = new Random();
            _renderTarget = new RenderTarget2D(_graphicsDevice, VirtualWidth, VirtualHeight);
            _ringTexture = GenerateRingTexture(100); // 100 pixel base radius
            _crtEffect = content.Load<Effect>("CRT");
        }

        public void Unload() {
            _renderTarget?.Dispose();
            _ringTexture?.Dispose();
        }


        private void DrawHollowRectangle(Rectangle rect, Color color, int thickness) {
            // Top
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Left
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            _spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }


        // Inside DualRenderer.cs
        public void DrawKeypadOverlay(string typedCode, bool isError) {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, GetUIScaleMatrix());

            // Dim the background
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Black * 0.8f);

            int boxWidth = 400;
            int boxHeight = 150;
            Rectangle terminalRect = new Rectangle(
                (VirtualWidth - boxWidth) / 2,
                (VirtualHeight - boxHeight) / 2,
                boxWidth,
                boxHeight
            );

            // Draw the box
            Color borderColor = isError ? Color.Red : Color.DarkGreen;
            _spriteBatch.Draw(_pixel, terminalRect, new Color(5, 5, 5));
            DrawHollowRectangle(terminalRect, borderColor, 2);

            // Draw Header
            string header = "SYSTEM OVERRIDE // MANUAL INPUT";
            Vector2 headSize = _font.MeasureString(header);
            _spriteBatch.DrawString(_font, header, new Vector2(terminalRect.Center.X - headSize.X / 2, terminalRect.Y + 15), borderColor);

            if (isError) {
                // Error Flash
                string errorMsg = "ACCESS DENIED";
                Vector2 errSize = _font.MeasureString(errorMsg);
                _spriteBatch.DrawString(_font, errorMsg, new Vector2(terminalRect.Center.X - errSize.X / 2, terminalRect.Center.Y), Color.Red);
            }
            else {
                // Standard Typing
                // Create a blinking cursor based on the system time
                bool showCursor = DateTime.Now.Millisecond < 500;
                string displayCode = "> " + typedCode + (showCursor ? "_" : "");

                Vector2 codeSize = _font.MeasureString(displayCode);
                _spriteBatch.DrawString(_font, displayCode, new Vector2(terminalRect.Center.X - codeSize.X / 2, terminalRect.Center.Y), Color.LimeGreen);
            }

            // Draw the Anti-Panic Abort Prompt
            string abortText = "[ESC] ABORT";
            Vector2 abortSize = _font.MeasureString(abortText);
            _spriteBatch.DrawString(_font, abortText, new Vector2(terminalRect.X + 15, terminalRect.Bottom - 25), Color.Gray);

            _spriteBatch.End();
        }


        public void DrawLogbookOverlay(List<string> documents, int selectedIndex) {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, GetUIScaleMatrix());

            //Heavy background dim to obscure the paused game
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Black * 0.95f);

            int padding = 50;

            // Draw Header
            _spriteBatch.DrawString(_font, "SYSTEM TERMINAL // ARCHIVED DATA", new Vector2(padding, padding), Color.DarkGreen);
            _spriteBatch.Draw(_pixel, new Rectangle(padding, padding + 25, VirtualWidth - (padding * 2), 2), Color.DarkGreen);

            if (documents.Count == 0) {
                _spriteBatch.DrawString(_font, "NO DATA FOUND IN MEMORY BANKS.", new Vector2(padding, padding + 50), Color.DarkGray);
            }
            else {
                // LEFT PANE: The File List
                int listWidth = 200;
                for (int i = 0; i < documents.Count; i++) {
                    // Highlight the currently selected document
                    Color col = (i == selectedIndex) ? Color.LimeGreen : new Color(20, 80, 20);
                    string prefix = (i == selectedIndex) ? "> " : "  ";
                    string title = $"{prefix}LOG_ENTRY_{i:D2}.sys";

                    _spriteBatch.DrawString(_font, title, new Vector2(padding, padding + 50 + (i * 30)), col);
                }

                // RIGHT PANE: The Document Text
                int textX = padding + listWidth + 20;
                float maxLineWidth = VirtualWidth - textX - padding;

                // Use the WrapText helper we built earlier!
                string wrappedText = WrapText(_font, documents[selectedIndex], maxLineWidth);

                _spriteBatch.DrawString(_font, wrappedText, new Vector2(textX, padding + 50), Color.LimeGreen);
            }

            _spriteBatch.End();
        }


        private Texture2D GenerateRingTexture(int radius) {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(_graphicsDevice, diameter, diameter);
            Color[] colorData = new Color[diameter * diameter];

            float center = radius;
            float thickness = 4f; // 4 pixels thick

            for (int y = 0; y < diameter; y++) {
                for (int x = 0; x < diameter; x++) {
                    float distance = Vector2.Distance(new Vector2(center, center), new Vector2(x, y));

                    // If the pixel falls on the edge of the circle, color it
                    if (distance <= radius && distance >= radius - thickness) {
                        // Soften the edge slightly for a glow effect
                        float alpha = 1f - Math.Abs(distance - (radius - thickness / 2)) / (thickness / 2);
                        colorData[y * diameter + x] = Color.White * alpha;
                    }
                    else {
                        colorData[y * diameter + x] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }


        public void DrawDocumentOverlay(string documentText) {
            // We start a new batch. 
            // IMPORTANT: This must be called BEFORE the final "DRAW TO SCREEN" block where the render target is cleared.
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, GetUIScaleMatrix());

            //Dim the gameplay in the background
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, VirtualWidth, VirtualHeight), Color.Black * 0.8f);

            //Draw the document background (Brutalist Terminal Aesthetic)
            int docWidth = 600;
            int docHeight = 400;
            Rectangle docRect = new Rectangle(
                (VirtualWidth - docWidth) / 2,
                (VirtualHeight - docHeight) / 2,
                docWidth,
                docHeight
            );

            // Dark terminal background with a slight green border
            _spriteBatch.Draw(_pixel, docRect, new Color(5, 10, 5));
            _spriteBatch.Draw(_pixel, new Rectangle(docRect.X, docRect.Y, docRect.Width, 2), Color.DarkGreen);
            _spriteBatch.Draw(_pixel, new Rectangle(docRect.X, docRect.Bottom, docRect.Width, 2), Color.DarkGreen);

            //Wrap and draw the text
            string wrappedText = WrapText(_font, documentText, docWidth - 40);
            _spriteBatch.DrawString(_font, wrappedText, new Vector2(docRect.X + 20, docRect.Y + 20), Color.LimeGreen);

            _spriteBatch.End();
        }

        // Reusable text wrapping system
        private string WrapText(SpriteFont spriteFont, string text, float maxLineWidth) {
            if (string.IsNullOrEmpty(text)) return string.Empty;

         
            // This catches both manual "\n" typing and LDtk's 'Enter' key presses.
            text = text.Replace("\\n", "\n").Replace("\r\n", "\n");

            //Split into distinct paragraphs
            string[] paragraphs = text.Split('\n');
            StringBuilder sb = new StringBuilder();
            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string paragraph in paragraphs) {
                string[] words = paragraph.Split(' ');
                float lineWidth = 0f; // Reset line width at the start of every paragraph

                foreach (string word in words) {
                    Vector2 size = spriteFont.MeasureString(word);

                    if (lineWidth + size.X < maxLineWidth) {
                        sb.Append(word + " ");
                        lineWidth += size.X + spaceWidth;
                    }
                    else {
                        sb.Append("\n" + word + " ");
                        lineWidth = size.X + spaceWidth;
                    }
                }
                sb.Append("\n"); // Add the paragraph break back in
            }

            // Clean up the trailing whitespace and newlines
            return sb.ToString().TrimEnd('\n', ' ');
        }



        public void Draw(GameTime gameTime, PolarityManager polarityManager, Level level, Player player, Camera camera, List<GlitchParticle> particles, BlastEffect blast = null) {
            float currentFrequency = polarityManager.CurrentFrequency;

            //Calculate Unified Stress
            // Frequency builds base tension. Strain builds the violent peak before burnout.
            float baseTension = (float)Math.Pow(currentFrequency, 3);
            float totalStress = polarityManager.TotalStress;

            //Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, currentFrequency);
            Color bgColor = Color.Lerp(new Color(10, 12, 15), new Color(30, 2, 2), currentFrequency);

            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(bgColor);

            //The Shake Effect (Driven by totalStress)
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

            foreach (var particle in particles) {
                Rectangle rect = new Rectangle((int)particle.Position.X, (int)particle.Position.Y, particle.Size, particle.Size);
                _spriteBatch.Draw(_pixel, rect, particle.BaseColor * particle.Alpha);
            }

            //The Entity Drawing
            System.Action<Entities.Entity, Rectangle> drawBlock = (entity, bounds) => {
                float presence = entity.GetPresence(currentFrequency);
                if (presence <= 0f) return;

                // We no longer guess the math. We ask the Entity directly.
                if (!entity.IsSolid(currentFrequency)) {
                    // FADING / GHOST PHASE
                    Color shadowColor = Color.Lerp(Color.Gray, entity.BaseColor, 0.3f);
                    _spriteBatch.Draw(_pixel, bounds, shadowColor * presence);
                }
                else {
                    // FULLY SOLID PHASE
                    _spriteBatch.Draw(_pixel, bounds, entity.BaseColor * 0.9f);
                    DrawHollowRectangle(bounds, Color.White, 2);
                }
            };

            if (level.ExitZone != Rectangle.Empty) {
                float pulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5) * 0.25f + 0.5f;
                _spriteBatch.Draw(_pixel, level.ExitZone, Color.Gold * pulse);
            }

            // Render Physical Blocks
            foreach (var obj in level.EnvironmentObjects) drawBlock(obj, obj.Bounds);
            foreach (var interactable in level.Interactables) drawBlock(interactable, interactable.Bounds);
            foreach (var enemy in level.Enemies) drawBlock(enemy, enemy.Bounds);
            foreach (var document in level.Documents) {
                if (!document.IsCollected) {

                    drawBlock(document, document.Bounds);
                }
            }
            foreach (var upgrade in level.UpgradeNodes) {
                if (!upgrade.IsCollected) {

                    drawBlock(upgrade, upgrade.Bounds);
                }
            }

            if (level.LockedDoors != null) {
                foreach (var door in level.LockedDoors) {
                    // Only draw the door if it hasn't been hacked yet
                    if (door.IsLocked) {
                        drawBlock(door, door.Bounds);
                    }
                }
            }

            // Render Player
            Color playerColor = Color.Lerp(Color.LimeGreen, Color.White, currentFrequency);
            if (totalStress > 0.2f) {
                Rectangle ghostBounds = player.Bounds;
                ghostBounds.Inflate((int)(10 * totalStress), (int)(10 * totalStress));
                _spriteBatch.Draw(_pixel, ghostBounds, Color.Gray * (0.3f * totalStress));
            }
            _spriteBatch.Draw(_pixel, player.Bounds, playerColor);

            // Draw the expanding Blast Radius
            if (blast != null && blast.IsActive) {
                // The origin point is the exact center of our generated texture
                Vector2 origin = new Vector2(_ringTexture.Width / 2f, _ringTexture.Height / 2f);

                // Scale the base 100px texture up to whatever the current radius of the blast is
                float scale = blast.CurrentRadius / (_ringTexture.Width / 2f);

                // Draw it. The color scales its alpha value to fade out.
                _spriteBatch.Draw(
                    _ringTexture,
                    blast.Position,
                    null,
                    Color.Cyan * blast.Alpha, //
                    0f,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }

            // Render Clues (Decals)
            foreach (var decal in level.Decals) {
                float presence = decal.GetPresence(currentFrequency);
                if (presence <= 0f) continue;

                // Grab the pre-calculated string for our current frequency distance
                string textToDraw = decal.GetCurrentText(currentFrequency);

                _spriteBatch.DrawString(_font, textToDraw, decal.Position, decal.BaseColor * presence);
            }


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

            //if (polarityManager.Strain > 0) {
            //    // Start a new batch for UI so it ignores the camera transform and sticks to the screen
            //    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            //    Rectangle strainBg = new Rectangle(VirtualWidth / 2 - 100, 20, 200, 15);
            //    Rectangle strainFg = new Rectangle(VirtualWidth / 2 - 100, 20, (int)(200 * polarityManager.Strain), 15);

            //    _spriteBatch.Draw(_pixel, strainBg, Color.DarkRed * 0.5f);
            //    _spriteBatch.Draw(_pixel, strainFg, polarityManager.IsBurntOut ? Color.White : Color.Red);

            //    string txt = polarityManager.IsBurntOut ? "SYSTEM BURNOUT" : "SYSTEM STRAIN";
            //    Vector2 size = _font.MeasureString(txt);
            //    _spriteBatch.DrawString(_font, txt, new Vector2(VirtualWidth / 2 - size.X / 2, 40), polarityManager.IsBurntOut ? Color.Red : Color.White);

            //    _spriteBatch.End();
            //}


            _shaderTime = (float)gameTime.TotalGameTime.TotalSeconds;
            // _shaderIntensity = 0.2f + (currentFrequency * 8.0f) + (totalStress * 25.0f);
            _shaderIntensity = 0.2f + (currentFrequency * 0.4f) + (totalStress * 1.5f);

            // Unbind the render target so it's ready to be read as a texture
            _graphicsDevice.SetRenderTarget(null);

            // =============================================================
            // PASS 2: DRAW THE CANVAS TO THE SCREEN WITH THE SHADER
            // =============================================================

            //// 1. Unbind the RenderTarget so we draw to the physical monitor
            //_graphicsDevice.SetRenderTarget(null);
            //_graphicsDevice.Clear(Color.Black);

            //// 2. Inject the dynamic math into the HLSL Shader
            //float time = (float)gameTime.TotalGameTime.TotalSeconds;
            //_crtEffect.Parameters["Time"]?.SetValue(time);

            //// Make the glitch intensity react to both the dimension AND the system stress
            //float intensity = 0.2f + (currentFrequency * 8.0f) + (totalStress * 25.0f);
            //_crtEffect.Parameters["Intensity"]?.SetValue(intensity);

            //// 3. Draw the canvas using the CRT Shader
            //// Note: We use SpriteSortMode.Immediate so the shader applies immediately to this batch
            //_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null, _crtEffect);

            //// Stretch the render target to perfectly fit the player's window
            //Rectangle screenRect = new Rectangle(0, 0, _graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

            //_spriteBatch.Draw(_renderTarget, screenRect, Color.White);

            //_spriteBatch.End();

        }


        public void DrawHUD(PolarityManager polarityManager) {
            if (polarityManager.Strain > 0) {
                // We inject the UI matrix here so it scales perfectly but ignores the camera and shader!
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, GetUIScaleMatrix());

                Rectangle strainBg = new Rectangle(VirtualWidth / 2 - 100, 20, 200, 15);
                Rectangle strainFg = new Rectangle(VirtualWidth / 2 - 100, 20, (int)(200 * polarityManager.Strain), 15);

                _spriteBatch.Draw(_pixel, strainBg, Color.DarkRed * 0.5f);
                _spriteBatch.Draw(_pixel, strainFg, polarityManager.IsBurntOut ? Color.White : Color.Red);

                string txt = polarityManager.IsBurntOut ? "SYSTEM BURNOUT" : "SYSTEM STRAIN";
                Vector2 size = _font.MeasureString(txt);
                _spriteBatch.DrawString(_font, txt, new Vector2(VirtualWidth / 2 - size.X / 2, 40), polarityManager.IsBurntOut ? Color.Red : Color.White);

                _spriteBatch.End();
            }
        }


        private Matrix GetUIScaleMatrix() {
            Rectangle dest = CalculateDestinationRectangle();
            float scaleX = (float)dest.Width / VirtualWidth;
            float scaleY = (float)dest.Height / VirtualHeight;

            // This perfectly aligns your UI to the letterboxed game screen
            return Matrix.CreateScale(scaleX, scaleY, 1f) * Matrix.CreateTranslation(dest.X, dest.Y, 0);
        }


        public void PresentToScreen() {
            // --- DRAW TO SCREEN ---
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Black);

            // Pass the cached parameters into the HLSL effect
            _crtEffect.Parameters["Time"]?.SetValue(_shaderTime);
            _crtEffect.Parameters["Intensity"]?.SetValue(_shaderIntensity);

            Rectangle destinationRect = CalculateDestinationRectangle();

            // Use SpriteSortMode.Immediate and pass the effect to apply the shader to the scaling pass
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null, _crtEffect);

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