using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Duality.Scenes
{
    public class MainMenuScene : Scene
    {
        public MainMenuScene(Game1 game) : base(game) { }

        public override void Initialize() { }
        public override void LoadContent() { }

        public override void Update(GameTime gameTime) {
            // Enter to start, Escape to quit
            if (Game._inputManager.WasActionPressed(Keys.Enter))
                Game.ChangeScene(new GameplayScene(Game, "Content/Levels/Level_Tutorial.json"));

            if (Game._inputManager.WasActionPressed(Keys.Escape))
                Game.Exit();
        }

        public override void Draw(GameTime gameTime) {
            Game.GraphicsDevice.Clear(new Color(15, 15, 15));
            Game._spriteBatch.Begin();

            // Simple centered text for the menu
            Game._spriteBatch.DrawString(Game._font, "D U A L I T Y", new Vector2(330, 200), Color.White);
            Game._spriteBatch.DrawString(Game._font, "Press ENTER to Start", new Vector2(300, 300), Color.LightGray);
            Game._spriteBatch.DrawString(Game._font, "Press ESC to Quit", new Vector2(315, 350), Color.DarkGray);

            Game._spriteBatch.End();
        }
    }
}