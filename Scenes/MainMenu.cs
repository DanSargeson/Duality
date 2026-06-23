using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Duality.Scenes
{
    public class MainMenuScene : Scene
    {
        public MainMenuScene(Game1 game) : base(game) { }

        public override void Initialize() { }
        public override void LoadContent() { }

        public override void Unload() { }

        public override void Update(GameTime gameTime) {
            // Enter to start, Escape to quit
            if (Game._inputManager.WasActionPressed(Keys.Enter))
                Game.ChangeScene(new GameplayScene(Game, "Content/Levels/Project.ldtk", "TUTORIAL"));

            if (Game._inputManager.WasActionPressed(Keys.Escape))
                Game.Exit();
        }

        public override void Draw(GameTime gameTime) {
            Game.GraphicsDevice.Clear(new Color(15, 15, 15));
            Game._spriteBatch.Begin();

            int screenWidth = Game.GraphicsDevice.Viewport.Width;
            int screenHeight = Game.GraphicsDevice.Viewport.Height;

            // Center Title
            string title = "D U A L I T Y";
            Vector2 titleSize = Game._font.MeasureString(title);
            Game._spriteBatch.DrawString(Game._font, title, new Vector2((screenWidth - titleSize.X) / 2, screenHeight * 0.3f), Color.White);

            // Center Start Text
            string startText = "Press ENTER to Start";
            Vector2 startSize = Game._font.MeasureString(startText);
            Game._spriteBatch.DrawString(Game._font, startText, new Vector2((screenWidth - startSize.X) / 2, screenHeight * 0.5f), Color.LightGray);

            // Center Quit Text
            string quitText = "Press ESC to Quit";
            Vector2 quitSize = Game._font.MeasureString(quitText);
            Game._spriteBatch.DrawString(Game._font, quitText, new Vector2((screenWidth - quitSize.X) / 2, screenHeight * 0.6f), Color.DarkGray);

            Game._spriteBatch.End();
        }
    }
}