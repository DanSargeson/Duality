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
        private Enemy _enemy;
        private List<EnvironmentObject> _environmentObjects;
        private List<Enemy> _enemies;


        private List<Decal> _decals;
        private List<InteractableObject> _interactables;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            _inputManager = new InputManager();
            _polarityManager = new PolarityManager();
            _levelManager = new LevelManager();
            _camera = new Camera(GraphicsDevice.Viewport);
            _levelManager.LoadLevel("Content/Levels/Level_Tutorial.json");
            // Initialize Player
            _player = new Player {
                Position = new Vector2(100, 250),
                LastSafePosition = new Vector2(100, 250),
                StartPosition = new Vector2(100, 250)
            };

            // Initialize Environment (This would eventually be loaded from a level file)
            _environmentObjects = _levelManager.EnvironmentObjects;

            _enemies = _levelManager.Enemies;

            _decals = _levelManager.Decals;
            _interactables = _levelManager.Interactables;

            base.Initialize();
        }

        protected override void LoadContent() {
            // Initialize renderer here because it requires the GraphicsDevice to be ready
            SpriteFont font = Content.Load<SpriteFont>("Font");
            _renderer = new DualRenderer(GraphicsDevice, font);
            _camera = new Camera(GraphicsDevice.Viewport);
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

            if (_inputManager.IsInteractPressed) {
                foreach (var interactable in _interactables) {
                    // If the door is closed and the player is standing next to it
                    if (interactable.IsClosed && interactable.InteractionArea.Intersects(_player.Bounds)) {
                        interactable.IsClosed = false; // Open the door!
                    }
                }
            }

            // Move player
            _player.Update(gameTime, _inputManager.GetMovementDirection(), _polarityManager, _environmentObjects, _enemies, _interactables, _levelManager.LevelBounds);

            foreach (var enemy in _enemies) {
                enemy.Update(gameTime);
            }

            // TODO: Collision resolution between _player and _environmentObjects 
            // relying on EnvironmentObject.IsSolid(_polarityManager.CurrentFrequency)
            _camera.Follow(_player.Position, (float)gameTime.ElapsedGameTime.TotalSeconds, _levelManager.LevelBounds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // Pass the current state to the renderer
            _renderer.Draw(_polarityManager.CurrentFrequency, _environmentObjects, _enemies, _interactables, _decals, _player, _camera);

            base.Draw(gameTime);
        }
    }
}