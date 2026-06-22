using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Duality.Entities;
using Duality.Mechanics;
using Duality.Rendering;

namespace Duality
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;

        // Systems
        private InputManager _inputManager;
        private PolarityManager _polarityManager;
        private DualRenderer _renderer;

        // State
        private Player _player;
        private List<EnvironmentObject> _environmentObjects;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            _inputManager = new InputManager();
            _polarityManager = new PolarityManager();

            // Initialize Player
            _player = new Player {
                Position = new Vector2(100, 250),
                StartPosition = new Vector2(100, 250)
            };

            // Initialize Environment (This would eventually be loaded from a level file)
            _environmentObjects = new List<EnvironmentObject>
            {
                // A Density Wall that blocks you
                new EnvironmentObject(new Rectangle(300, 200, 50, 200), Color.SteelBlue, 0.0f, ObjectType.Obstacle),
    
                // A permanent physical gap in the floor. 
                // We give it a massive Range (100f) so it never fades regardless of frequency.
                new EnvironmentObject(new Rectangle(350, 250, 200, 50), new Color(10, 10, 10), 0.0f, ObjectType.Hazard) { Range = 100f },

                // An Insight Bridge layered directly over the gap
                new EnvironmentObject(new Rectangle(350, 250, 200, 50), Color.HotPink, 1.0f, ObjectType.Platform)
            };

            base.Initialize();
        }

        protected override void LoadContent() {
            // Initialize renderer here because it requires the GraphicsDevice to be ready
            _renderer = new DualRenderer(GraphicsDevice);
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

            // Move player
            _player.Update(gameTime, _inputManager.GetMovementDirection(), _polarityManager.CurrentFrequency, _environmentObjects);

            // TODO: Collision resolution between _player and _environmentObjects 
            // relying on EnvironmentObject.IsSolid(_polarityManager.CurrentFrequency)

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // Pass the current state to the renderer
            _renderer.Draw(_polarityManager.CurrentFrequency, _environmentObjects, _player);

            base.Draw(gameTime);
        }
    }
}