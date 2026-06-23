using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Duality.Entities;
using Duality.Mechanics;
using Duality.Rendering;
using Duality.Data;

namespace Duality
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;

        // Systems
        private InputManager _inputManager;
        private PolarityManager _polarityManager;
        private DualRenderer _renderer;
        private Camera _camera;

        private LevelManager _levelManager;

        // State
        private Player _player;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.AllowUserResizing = true;
        }

        protected override void Initialize() {
            _inputManager = new InputManager();
            _polarityManager = new PolarityManager();
            _levelManager = new LevelManager();
            _levelManager.LoadLevel("Content/Levels/Level_Tutorial.json");
            // Initialize Player
            _player = new Player {
                Position = new Vector2(100, 250),
                LastSafePosition = new Vector2(100, 250),
                StartPosition = new Vector2(100, 250)
            };

            base.Initialize();
        }

        protected override void LoadContent() {
            // Initialize renderer here because it requires the GraphicsDevice to be ready
            SpriteFont font = Content.Load<SpriteFont>("Font");
            _renderer = new DualRenderer(GraphicsDevice, font);
            _camera = new Camera(_renderer.VirtualWidth, _renderer.VirtualHeight);
        }

        protected override void Update(GameTime gameTime) {
            _inputManager.Update();

            if (_inputManager.IsPausePressed)
                Exit();

            // Shift polarity based on input
            _polarityManager.Update(
                gameTime,
                _inputManager.IsShiftingToInsight,
                _inputManager.IsShiftingToDensity
            );

           
            var currentLevel = _levelManager.CurrentLevel;


            foreach (var enemy in currentLevel.Enemies) {
                enemy.Update(gameTime);
            }

            if (_inputManager.IsInteractPressed) {
                foreach (var interactable in currentLevel.Interactables) {
                    // If the door is closed and the player is standing next to it
                    if (interactable.IsClosed && interactable.InteractionArea.Intersects(_player.Bounds)) {
                        interactable.IsClosed = false; // Open the door!
                    }
                }
            }


            // Move player
            _player.Update(gameTime, _inputManager.GetMovementDirection(), _polarityManager, currentLevel);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // TODO: Collision resolution between _player and _environmentObjects 
            // relying on EnvironmentObject.IsSolid(_polarityManager.CurrentFrequency)
            _camera.Follow(_player.Position, deltaTime, currentLevel.Bounds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // Pass the current state to the renderer
            _renderer.Draw(_polarityManager.CurrentFrequency, _levelManager.CurrentLevel, _player, _camera);

            base.Draw(gameTime);
        }
    }
}