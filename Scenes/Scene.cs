using Microsoft.Xna.Framework;
using System;

namespace Duality.Scenes
{
    public abstract class Scene : IDisposable
    {
        protected Game1 Game;

        public Scene(Game1 game) {
            Game = game;
        }

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
        public abstract void Unload();

        public virtual void Dispose() {
            Unload();
        }
    }
}