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
                LastSafePosition = new Vector2(100, 250)
            };

            // Initialize Environment (This would eventually be loaded from a level file)
            _environmentObjects = new List<EnvironmentObject>
            {
                // 1. DENSITY WALL (Anchor 0.0) 
                // Blocks you early on. You must shift towards Insight to make it fade so you can walk through.
                new EnvironmentObject(new Rectangle(200, 150, 50, 250), Color.SteelBlue, 0.0f, ObjectType.Obstacle),

                // 2. THE CHASM (Hazard)
                // A massive permanent pit. Range is 100f so it never fades. 
                // If you step here without a bridge, you get bounced back.
                new EnvironmentObject(new Rectangle(350, 150, 150, 250), new Color(10, 10, 10), 0.0f, ObjectType.Hazard) { Range = 100f },

                // 3. INSIGHT BRIDGE (Anchor 1.0)
                // Appears over the chasm when in Insight so you can safely cross.
                new EnvironmentObject(new Rectangle(350, 200, 150, 50), Color.HotPink, 1.0f, ObjectType.Platform),

                // 4. INSIGHT WALL (Anchor 1.0)
                // Blocks you right after the bridge. If you stay in Insight, you can't pass.
                // You must drop your frequency back to Density to make this wall fade!
                new EnvironmentObject(new Rectangle(550, 150, 50, 250), Color.HotPink, 1.0f, ObjectType.Obstacle)
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