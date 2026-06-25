using Duality.Data;
using Duality.Entities;
using Duality.Mechanics;
using Duality.Rendering;
using Duality.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Duality
{
    public class Game1 : Game
    {
        public GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;

        // Systems
        public InputManager _inputManager { get; private set; }
        public SpriteFont _font { get; private set; }

        public Scene _currentScene { get; private set; }
        public GameSession Session { get; private set; }


        // State
        // private Player _player;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        public void ChangeScene(Scene newScene) {
            _currentScene?.Dispose();
            _currentScene = newScene;
            _currentScene.Initialize();
            _currentScene.LoadContent();
        }

        protected override void Initialize() {
            _inputManager = new InputManager();
            Session = new GameSession();
            base.Initialize();
        }

        protected override void LoadContent() {
            // Initialize renderer here because it requires the GraphicsDevice to be ready
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Font");
  
            ChangeScene(new MainMenuScene(this));
        }

        protected override void Update(GameTime gameTime) {
            _inputManager.Update();
            _currentScene?.Update(gameTime);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            _currentScene?.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}