using Duality.Audio;
using Duality.Data;
using Duality.Entities;
using Duality.Mechanics;
using Duality.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Duality.Scenes
{
    public enum GameplayState
    {
        Active,
        ReadingDocument,
        Logbook,
        EnteringCode,
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
        private int _logbookSelectedIndex = 0;
        private List<string> _cachedLogbook; // Holds the indexed list while the menu is open
        private FloatingTextManager _textManager = new FloatingTextManager();
        private float requiredStress = 0.5f;

        private float _currentFrameFrequency;


        private LockedDoor _activeDoor;
        private string _currentTypedCode = "";
        private bool _showAccessDenied = false;
        private float _deniedTimer = 0f;


        private List<GlitchParticle> _particles = new List<GlitchParticle>();
        private Random _random = new Random();

        public GameplayScene(Game1 game, string ldtkFilepath, string levelName) : base(game) {
            _ldtkFilePath = ldtkFilepath;
            _levelName = levelName;
        }

        public override void Initialize() {

            Game.Window.TextInput += OnTextInput;

            _polarityManager = new PolarityManager();
            _polarityManager.Strain = Game.Session.CarriedStrain;
            _polarityManager.CurrentFrequency = Game.Session.CarriedFrequency;
            ApplyActiveUpgrades();
            _levelManager = new LevelManager();
            _levelManager.LoadLDtkLevel(_ldtkFilePath, _levelName);
            if (!string.IsNullOrEmpty(_levelManager.CurrentLevel.OnLoadLog)) {
                // Check if the system has already forced this log on the player
                if (!Game.Session.SeenLevelLogs.Contains(_levelManager.CurrentLevel.LevelId)) {

                    // Force the UI overlay
                    _activeDocumentText = _levelManager.CurrentLevel.OnLoadLog;
                    _currentState = GameplayState.ReadingDocument;

                    // Record that they've seen it so it doesn't happen on respawn
                    Game.Session.MarkLogAsSeen(_levelManager.CurrentLevel.LevelId);

                    // Optional: Play a harsh system boot-up sound
                    // _audioManager.PlaySound("TerminalBoot");
                }
            }
            _levelManager.CurrentLevel.Documents.RemoveAll(doc => Game.Session.CollectedDocuments.Contains(doc.TextContent));
            _levelManager.CurrentLevel.UpgradeNodes.RemoveAll(u => Game.Session.UnlockedUpgrades.Contains(u.UpgradeId));

            _player = new Player {
                Position = _levelManager.CurrentLevel.PlayerStart,
                LastSafePosition = _levelManager.CurrentLevel.PlayerStart,
                StartPosition = _levelManager.CurrentLevel.PlayerStart
            };

            _audioManager = new AudioManager();
        }

        public override void LoadContent() {
            _renderer = new DualRenderer(Game.GraphicsDevice, Game._font, Game.Content);
            _camera = new Camera(_renderer.VirtualWidth, _renderer.VirtualHeight);
            _audioManager.LoadContent(Game.Content);
        }


        private void ApplyActiveUpgrades() {
            // Reset to base defaults first so we have a clean slate
            _polarityManager.MaxInsightTime = 3.0f;
            _polarityManager.RecoveryTime = 4.0f;

            // Apply modifiers
            if (Game.Session.UnlockedUpgrades.Contains("DeepLungs")) {
                _polarityManager.MaxInsightTime = 10.0f;
            }
            if (Game.Session.UnlockedUpgrades.Contains("RapidGrounding")) {
                _polarityManager.RecoveryTime = 2.0f;
            }
        }

        public override void Update(GameTime gameTime) {
            // Pause Game
            if (_currentState == GameplayState.Active && Game._inputManager.IsPausePressed) {
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
                case GameplayState.EnteringCode:
                    UpdateCodeState(gameTime);
                    break;
                case GameplayState.Logbook:
                    UpdateLogbookState(gameTime);
                    break;
            }

            
        }

        private void UpdateActiveState(GameTime gameTime) {

            _polarityManager.Update(gameTime, Game._inputManager.IsShiftingToInsight, Game._inputManager.IsShiftingToDensity);
            _audioManager.Update(gameTime, _polarityManager.CurrentFrequency, _polarityManager.TotalStress, _polarityManager.IsBurntOut);

            var currentLevel = _levelManager.CurrentLevel;
            _currentFrameFrequency = _polarityManager.CurrentFrequency;

            if (Game._inputManager.IsLogbookPressed) {
                _currentState = GameplayState.Logbook;
                _logbookSelectedIndex = 0;

                // Convert the HashSet to a List so we can scroll through it by index
                _cachedLogbook = new List<string>(Game.Session.CollectedDocuments);
                return; // Halt active updates
            }


            // Enemies now require the player reference for Hunter behaviour
            foreach (var enemy in currentLevel.Enemies) {
                enemy.Update(gameTime, _player, _currentFrameFrequency, _particles, currentLevel);
            }


            //Upgrades
            foreach (var upgrade in currentLevel.UpgradeNodes) {
                if (!upgrade.IsCollected && upgrade.InteractionArea.Intersects(_player.Bounds)) {

                    Game.Session.UnlockUpgrade(upgrade.UpgradeId); // Save permanently
                    upgrade.IsCollected = true; // Remove locally

                    ApplyActiveUpgrades();

                    _currentState = GameplayState.ReadingDocument;
                    _activeDocumentText = upgrade.TextContent;

                    // TODO: Trigger a screen flash, a sound, popup etc here
                }
            }


            //Document manger
            if (Game._inputManager.IsInteractPressed) {
                //Check for Documents First
                foreach (var document in currentLevel.Documents) {
                    // Check that it hasn't been collected yet!
                    if (!document.IsCollected && document.InteractionArea.Intersects(_player.Bounds)) {

                        _activeDocumentText = document.TextContent;
                        Game.Session.CollectDocument(document.TextContent); // Save globally

                        document.IsCollected = true; // Mark as picked up locally
                        // TODO: Play a terminal typing sound here
                        // _audioManager.PlaySound("typing_clicks");
                        _currentState = GameplayState.ReadingDocument;
                        return;
                    }
                }
                //Signs do not save to gloabal session
                if (currentLevel.Signs != null) {
                    foreach (var sign in currentLevel.Signs) {
                        if (sign.InteractionArea.Intersects(_player.Bounds)) {

                            // Feed the sign text directly into the terminal overlay
                            _activeDocumentText = sign.TextContent;
                            _currentState = GameplayState.ReadingDocument;

                            // TODO: Play a minor mechanical "click" sound here instead of a heavy terminal boot
                            // _audioManager.PlaySound("button_click");

                            return; // Halt interactions
                        }
                    }
                }

                // 2. Standard Interactables (Doors, etc.)
                foreach (var interactable in currentLevel.Interactables) {
                    if (interactable.IsClosed && interactable.InteractionArea.Intersects(_player.Bounds)) {
                        interactable.IsClosed = false;
                    }
                }


                if (currentLevel.LockedDoors != null) {
                    foreach (var door in currentLevel.LockedDoors) {
                        // Ensure it's locked, we are standing near it, AND it's physically solid in our current dimension
                        if (door.IsLocked && door.IsSolid(_currentFrameFrequency) && door.InteractionArea.Intersects(_player.Bounds)) {

                            _activeDoor = door;
                            _currentTypedCode = "";
                            _showAccessDenied = false;

                            _currentState = GameplayState.EnteringCode;
                            return; // Halt active updates
                        }
                    }
                }
            }

            _player.Update(gameTime, Game._inputManager.GetMovementDirection(), _polarityManager, currentLevel);


            if (Game._inputManager.IsDischargePressed && !_polarityManager.IsBurntOut && _currentFrameFrequency >= 0.8f && _polarityManager.TotalStress >= requiredStress) {
                float blastRadius = 150f; //TODO MAGIC NUMBER
                Vector2 playerCenter = _player.Bounds.Center.ToVector2();

                // 1. The Violent Grounding: Snap the player back to Density instantly.
                // Ensure your rendering and polarity managers respect this forced overwrite.
                _currentFrameFrequency = 0f;

                _activeBlast = new BlastEffect(playerCenter, blastRadius);

                for (int i = currentLevel.Enemies.Count - 1; i >= 0; i--) {
                    var enemy = currentLevel.Enemies[i];
                    Vector2 enemyCenter = enemy.Bounds.Center.ToVector2();
                    float distance = Vector2.Distance(playerCenter, enemyCenter);

                    // 2. Spatial Check: Is the entity inside the blast radius?
                    if (distance <= blastRadius) {

                        // Calculate text position once
                        Vector2 textPosition = new Vector2(enemy.Position.X, enemy.Position.Y - 20);

                        if (enemy.Behaviour == EnemyBehaviour.Stalker) {
                            // Stalker lives in Deep Insight. The blast detonates in Density beneath them.
                            // Spawn the clinical error directly over the Stalker's head.
                            _textManager.Add("[ PHASE_LOCKED ]", textPosition, Color.Red);
                        }
                        else {
                            // It's a standard guard living in Density. The blast hits them.

                            // SPAWN PARTICLES
                            for (int p = 0; p < 15; p++) {
                                _particles.Add(new GlitchParticle {
                                    Position = enemyCenter,
                                    Velocity = new Vector2((float)_random.NextDouble() * 2 - 1, (float)_random.NextDouble() * 2 - 1) * _random.Next(150, 400),
                                    BaseColor = enemy.BaseColor,
                                    MaxLife = 0.5f + (float)_random.NextDouble() * 0.5f,
                                    Life = 0.5f + (float)_random.NextDouble() * 0.5f,
                                    Size = _random.Next(2, 6)
                                });
                            }

                            // SPAWN KILL CONFIRMATION TEXT
                            _textManager.Add("[ TERMINATED ]", textPosition, Color.White);

                            // DELETE ENEMY
                            currentLevel.Enemies.RemoveAt(i);
                        }
                    }
                }

                // Trigger the burnout and the audio cue
                _polarityManager.TriggerDischarge();
                // _audioManager.PlaySound("ringing"); 
            }


            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _activeBlast?.Update(deltaTime);
            _camera.Follow(_player.Bounds.Center.ToVector2(), deltaTime, currentLevel.Bounds);
            _textManager.Update(deltaTime);

            // Level Transition Logic
            if (currentLevel.ExitZone != Rectangle.Empty && _player.Bounds.Intersects(currentLevel.ExitZone)) {

                // Calculate presence: 0.0 at frequency 0.5, and 1.0 at frequencies 0.0 and 1.0
                float exitPresence = Math.Abs(_currentFrameFrequency - 0.5f) * 2f;

                // The door only registers interaction if it is at least 80% physically anchored
                if (exitPresence >= 0.8f) {
                    // Save the system strain and frequency before moving
                    Game.Session.CarriedStrain = _polarityManager.Strain;
                    Game.Session.CarriedFrequency = _currentFrameFrequency;

                    if (!string.IsNullOrEmpty(currentLevel.NextLevelPath))
                        Game.ChangeScene(new GameplayScene(Game, _ldtkFilePath, currentLevel.NextLevelPath));
                    else
                        Game.ChangeScene(new MainMenuScene(Game));
                }
                else {
                    // Optional: Spawn a faint UI warning or particle burst to show the gate is de-synchronized
                    if (_random.NextDouble() < 0.05) {
                        Vector2 gateCenter = currentLevel.ExitZone.Center.ToVector2();
                        _textManager.Add("[ REGISTRY_ERROR ]", new Vector2(gateCenter.X - 40, gateCenter.Y), Color.DarkGray);
                    }
                }
            }



            // Update the particles backwards:
            for (int i = _particles.Count - 1; i >= 0; i--) {
                _particles[i].Update(deltaTime);
                if (_particles[i].Life <= 0) {
                    _particles.RemoveAt(i);
                }
            }

        }

        private void UpdateLogbookState(GameTime gameTime) {
            // Close the logbook
            if (Game._inputManager.IsLogbookPressed || Game._inputManager.IsCancelPressed) {
                _currentState = GameplayState.Active;
                _cachedLogbook.Clear(); // Free the memory
                return;
            }

            // Handle Scrolling
            if (_cachedLogbook.Count > 0) {
                if (Game._inputManager.IsUpPressed) {
                    _logbookSelectedIndex--;
                    if (_logbookSelectedIndex < 0) _logbookSelectedIndex = _cachedLogbook.Count - 1;
                }

                if (Game._inputManager.IsDownPressed) {
                    _logbookSelectedIndex++;
                    if (_logbookSelectedIndex >= _cachedLogbook.Count) _logbookSelectedIndex = 0;
                }
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

            Game.Window.TextInput -= OnTextInput;
            _renderer?.Unload(); // Explicitly clear the RenderTarget2D from VRAM
            _audioManager.Unload(); // Stop and dispose audio instances
        }

        private void SubmitCode() {
            if (_currentTypedCode == _activeDoor.Passcode.ToUpper()) {
                // Success!
                _activeDoor.IsLocked = false;
                _currentState = GameplayState.Active;
                _currentTypedCode = "";

                // _audioManager.PlaySound("door_unlock");
            }
            else {
                // Failure! Flash "ACCESS DENIED"
                _showAccessDenied = true;
                _deniedTimer = 1.0f; // Show error for 1 second

                // _audioManager.PlaySound("error_buzzer");
            }
        }

        private void UpdateCodeState(GameTime gameTime) {
            if (Game._inputManager.IsCancelPressed) { // E.g., Escape key
                _currentState = GameplayState.Active;
                _currentTypedCode = "";
            }

            if (_showAccessDenied) {
                _deniedTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_deniedTimer <= 0) {
                    _showAccessDenied = false;
                    _currentTypedCode = ""; // Clear the wrong code
                }
            }
        }

        private void OnTextInput(object sender, TextInputEventArgs e) {
            if (_currentState != GameplayState.EnteringCode) return;
            if (_showAccessDenied) return; // Freeze input while showing error

            // Handle Backspace
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Back) {
                if (_currentTypedCode.Length > 0) {
                    _currentTypedCode = _currentTypedCode.Substring(0, _currentTypedCode.Length - 1);
                }
                return;
            }

            // Handle Enter/Return (Submit the code)
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter) {
                SubmitCode();
                return;
            }

            // Only allow standard typing characters (Letters, Numbers, Symbols)
            if (char.IsLetterOrDigit(e.Character) || char.IsSymbol(e.Character) || char.IsPunctuation(e.Character)) {
                // Cap the length to 10 characters so it doesn't run off the screen
                if (_currentTypedCode.Length < 10) {
                    // Optional: Force everything to uppercase for the terminal aesthetic
                    _currentTypedCode += char.ToUpper(e.Character);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            // 1. DRAW THE WORLD TO MEMORY 
            // (This calls the massive Draw method in your DualRenderer)
            _renderer.Draw(gameTime, _polarityManager, _textManager, _levelManager.CurrentLevel, _player, _camera, _particles, _activeBlast);
            
            // 2. BLAST MEMORY TO SCREEN 
            // (Applies the CRT Shader and letterboxes the view)
            _renderer.PresentToScreen();

            // 3. DRAW UI ON TOP 
            // (Clean, un-glitched, perfectly scaled)
            _renderer.DrawHUD(_polarityManager);

            // 4. DRAW ACTIVE MENUS
            // (Dim the screen and draw the interactive UI over the shader)
            switch (_currentState) {
                case GameplayState.EnteringCode:
                    _renderer.DrawKeypadOverlay(_currentTypedCode, _showAccessDenied);
                    break;
                case GameplayState.ReadingDocument:
                    _renderer.DrawDocumentOverlay(_activeDocumentText);
                    break;
                case GameplayState.Logbook:
                    _renderer.DrawLogbookOverlay(_cachedLogbook, _logbookSelectedIndex);
                    break;
            }
        }

        //public override void Draw(GameTime gameTime) {
        //    _renderer.Draw(gameTime, _polarityManager, _levelManager.CurrentLevel, _player, _camera, _particles, _activeBlast);
        //    if (_currentState == GameplayState.ReadingDocument) {
        //        _renderer.DrawDocumentOverlay(_activeDocumentText);
        //    }
        //    else if (_currentState == GameplayState.Logbook) { 
        //        _renderer.DrawLogbookOverlay(_cachedLogbook, _logbookSelectedIndex);
        //    }
        //    else if(_currentState == GameplayState.EnteringCode) {

        //        _renderer.DrawKeypadOverlay(_currentTypedCode, _showAccessDenied);
        //    }

        //    _renderer.PresentToScreen();
        //}
    }
}