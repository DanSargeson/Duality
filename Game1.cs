using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Duality
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // The core mechanic: 0.0 is Density, 1.0 is Insight.
        public static float CurrentFrequency { get; private set; } = 0.0f;

        // Mock textures for the prototype
        private Texture2D _pixel;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a simple 1x1 white pixel for prototyping shapes
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime) {
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Escape))
                Exit();

            // 1. Update the Polarity Slider
            // Shift towards Insight (Q) or Density (E)
            float shiftSpeed = 1.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kstate.IsKeyDown(Keys.Q))
                CurrentFrequency += shiftSpeed;
            if (kstate.IsKeyDown(Keys.E))
                CurrentFrequency -= shiftSpeed;

            // Clamp the frequency between the two absolutes
            CurrentFrequency = MathHelper.Clamp(CurrentFrequency, 0.0f, 1.0f);

            // TODO: Update Player position (collision logic will check CurrentFrequency)

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // The background color dynamically shifts based on frequency
            // Density = Dark Gray, Insight = Stark White
            Color bgColor = Color.Lerp(new Color(20, 20, 20), Color.White, CurrentFrequency);
            GraphicsDevice.Clear(bgColor);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // ---------------------------------------------------------
            // RENDER DENSITY OBJECTS (Fades out as Frequency approaches 1.0)
            // ---------------------------------------------------------
            float densityAlpha = 1.0f - CurrentFrequency;
            Color densityColor = Color.SteelBlue * densityAlpha;

            // Example: A physical wall
            _spriteBatch.Draw(_pixel, new Rectangle(300, 200, 50, 200), densityColor);

            // ---------------------------------------------------------
            // RENDER INSIGHT OBJECTS (Fades in as Frequency approaches 1.0)
            // ---------------------------------------------------------
            float insightAlpha = CurrentFrequency;
            Color insightColor = Color.HotPink * insightAlpha;

            // Example: A hidden bridge that only appears at high frequency
            _spriteBatch.Draw(_pixel, new Rectangle(350, 250, 200, 50), insightColor);

            // ---------------------------------------------------------
            // RENDER PLAYER (Always visible, but maybe changes color/sprite)
            // ---------------------------------------------------------
            _spriteBatch.Draw(_pixel, new Rectangle(100, 250, 32, 32), Color.LimeGreen);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}