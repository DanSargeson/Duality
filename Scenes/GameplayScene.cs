using Microsoft.Xna.Framework;
using Duality.Entities;
using Duality.Mechanics;
using Duality.Rendering;
using Duality.Data;
using Duality.Audio;

namespace Duality.Scenes
{
    public enum GameplayState
    {
        Active,
        ReadingDocument,
        Paused // Ready for future use
    }

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
        private GameplayState _currentState = GameplayState.Active;
        private string _activeDocumentText = string.Empty;
        private BlastEffect _activeBlast;

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

            switch (_currentState) {
                case GameplayState.Active:
                    UpdateActiveState(gameTime);
                    break;
                case GameplayState.ReadingDocument:
                    UpdateReadingState(gameTime);
                    break;
            }

            
        }

        private void UpdateActiveState(GameTime gameTime) {

            _polarityManager.Update(gameTime, Game._inputManager.IsShiftingToInsight, Game._inputManager.IsShiftingToDensity);
            _audioManager.Update(_polarityManager.CurrentFrequency, _polarityManager.TotalStress);

            var currentLevel = _levelManager.CurrentLevel;

            // Enemies now require the player reference for Hunter behaviour
            foreach (var enemy in currentLevel.Enemies) {
                enemy.Update(gameTime, _player, _polarityManager.CurrentFrequency);
            }

            if (Game._inputManager.IsInteractPressed) {
                // 1. Check for Documents First
                foreach (var document in currentLevel.Documents) { // Assuming you create a Documents list in Level.cs
                    if (document.InteractionArea.Intersects(_player.Bounds)) {
                        _activeDocumentText = document.TextContent; // Grab the text parsed from LDtk
                        _currentState = GameplayState.ReadingDocument;
                        return; // Halt further updates this frame
                    }
                }
                // 2. Standard Interactables (Doors, etc.)
                foreach (var interactable in currentLevel.Interactables) {
                    if (interactable.IsClosed && interactable.InteractionArea.Intersects(_player.Bounds)) {
                        interactable.IsClosed = false;
                    }
                }
            }

            _player.Update(gameTime, Game._inputManager.GetMovementDirection(), _polarityManager, currentLevel);


            if (Game._inputManager.IsDischargePressed && !_polarityManager.IsBurntOut && _polarityManager.CurrentFrequency >= 0.8f) { //TODO MAGIC NUMBER -  REMOVE/MOVE
                float blastRadius = 150f;       //TODO MAGIC NUMBER -  REMOVE/MOVE
                Vector2 playerCenter = _player.Bounds.Center.ToVector2();

                _activeBlast = new BlastEffect(playerCenter, blastRadius);

                for (int i = currentLevel.Enemies.Count - 1; i >= 0; i--) {
                    var enemy = currentLevel.Enemies[i];

                    // 2. The Target Check: Only affect enemies that are physically solid in the current frequency
                    if (enemy.IsDangerous(_polarityManager.CurrentFrequency)) {
                        Vector2 enemyCenter = enemy.Bounds.Center.ToVector2();
                        float distance = Vector2.Distance(playerCenter, enemyCenter);

                        if (distance <= blastRadius) {
                            currentLevel.Enemies.RemoveAt(i);
                        }
                    }
                }

                // Trigger the burnout and the audio cue
                _polarityManager.TriggerDischarge();
                // _audioManager.PlaySound("ringing"); // Hook up your single-shot audio here
            }


            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _activeBlast?.Update(deltaTime);
            _camera.Follow(_player.Bounds.Center.ToVector2(), deltaTime, currentLevel.Bounds);

            // Level Transition Logic
            if (currentLevel.ExitZone != Rectangle.Empty && _player.Bounds.Intersects(currentLevel.ExitZone)) {
                if (!string.IsNullOrEmpty(currentLevel.NextLevelPath))
                    Game.ChangeScene(new GameplayScene(Game, _ldtkFilePath, currentLevel.NextLevelPath));
                else
                    Game.ChangeScene(new MainMenuScene(Game)); // Back to menu if game is over
            }

        }


        private void UpdateReadingState(GameTime gameTime) {
            // If the player presses Interact or a specific 'Close' button
            if (Game._inputManager.IsInteractPressed /* || Game._inputManager.IsCancelPressed */) {
                _activeDocumentText = string.Empty;
                _currentState = GameplayState.Active;
            }
        }

        public override void Unload() {
            _renderer?.Unload(); // Explicitly clear the RenderTarget2D from VRAM
            _audioManager.Unload(); // Stop and dispose audio instances
        }

        public override void Draw(GameTime gameTime) {
            _renderer.Draw(gameTime, _polarityManager, _levelManager.CurrentLevel, _player, _camera, _activeBlast);
            if (_currentState == GameplayState.ReadingDocument) {
                _renderer.DrawDocumentOverlay(_activeDocumentText);
            }

            _renderer.PresentToScreen();
        }
    }
}