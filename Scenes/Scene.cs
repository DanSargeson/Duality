using Microsoft.Xna.Framework;

namespace Duality.Scenes
{
    public abstract class Scene
    {
        protected Game1 Game;

        public Scene(Game1 game) {
            Game = game;
        }

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
}