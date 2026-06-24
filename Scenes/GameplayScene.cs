using Microsoft.Xna.Framework;
using Duality.Entities;
using Duality.Mechanics;
using Duality.Rendering;
using Duality.Data;
using Duality.Audio;

namespace Duality.Scenes
{
    public class GameplayScene : Scene
    {
        private PolarityManager _polarityManager;
        private DualRenderer _renderer;
        private Camera _camera;
        private LevelManager _levelManager;
        private Player _player;
        private string _ldtkFilePath;
        private string _levelName;
        private AudioManager _audioManager;

        public GameplayScene(Game1 game, string ldtkFilepath, string levelName) : base(game) {
            _ldtkFilePath = ldtkFilepath;
            _levelName = levelName;
        }

        public override void Initialize() {
            _polarityManager = new PolarityManager();
            _levelManager = new LevelManager();
            _levelManager.LoadLDtkLevel(_ldtkFilePath, _levelName);

            _player = new Player {
                Position = _levelManager.CurrentLevel.PlayerStart,
                LastSafePosition = _levelManager.CurrentLevel.PlayerStart,
                StartPosition = _levelManager.CurrentLevel.PlayerStart
            };

            _audioManager = new AudioManager();
        }

        public override void LoadContent() {
            _renderer = new DualRenderer(Game.GraphicsDevice, Game._font);
            _camera = new Camera(_renderer.VirtualWidth, _renderer.VirtualHeight);
            _audioManager.LoadContent(Game.Content);
        }

        public override void Update(GameTime gameTime) {
            // Pause Game
            if (Game._inputManager.IsPausePressed) {
                Game.ChangeScene(new MainMenuScene(Game)); // Later, change this to a PauseScene
                return;
            }

            _polarityManager.Update(gameTime, Game._inputManager.IsShiftingToInsight, Game._inputManager.IsShiftingToDensity);
            _audioManager.Update(_polarityManager.CurrentFrequency, _polarityManager.TotalStress);

            var currentLevel = _levelManager.CurrentLevel;

            // Enemies now require the player reference for Hunter behaviour
            foreach (var enemy in currentLevel.Enemies) {
                enemy.Update(gameTime, _player, _polarityManager.CurrentFrequency);
            }

            if (Game._inputManager.IsInteractPressed) {
                foreach (var interactable in currentLevel.Interactables) {
                    if (interactable.IsClosed && interactable.InteractionArea.Intersects(_player.Bounds)) {
                        interactable.IsClosed = false;
                    }
                }
            }

            _player.Update(gameTime, Game._inputManager.GetMovementDirection(), _polarityManager, currentLevel);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _camera.Follow(_player.Bounds.Center.ToVector2(), deltaTime, currentLevel.Bounds);

            // Level Transition Logic
            if (currentLevel.ExitZone != Rectangle.Empty && _player.Bounds.Intersects(currentLevel.ExitZone)) {
                if (!string.IsNullOrEmpty(currentLevel.NextLevelPath))
                    Game.ChangeScene(new GameplayScene(Game, _ldtkFilePath, currentLevel.NextLevelPath));
                else
                    Game.ChangeScene(new MainMenuScene(Game)); // Back to menu if game is over
            }
        }

        public override void Unload() {
            _renderer?.Unload(); // Explicitly clear the RenderTarget2D from VRAM
            _audioManager.Unload(); // Stop and dispose audio instances
        }

        public override void Draw(GameTime gameTime) {
            _renderer.Draw(gameTime, _polarityManager, _levelManager.CurrentLevel, _player, _camera);
        }
    }
}