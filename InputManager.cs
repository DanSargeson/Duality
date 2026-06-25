using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Duality
{
    public class InputManager
    {
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        // Call this exactly once per frame at the very top of Game1.Update()
        public void Update() {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();
        }

        // Semantic Action: Polarity Shifting (Continuous)
        public bool IsShiftingToInsight => _currentKeyState.IsKeyDown(Keys.Q);
        public bool IsShiftingToDensity => _currentKeyState.IsKeyDown(Keys.E);

        public bool IsLogbookPressed => Keyboard.GetState().IsKeyDown(Keys.Tab) && _previousKeyState.IsKeyUp(Keys.Tab);
        public bool IsDischargePressed => _currentKeyState.IsKeyDown(Keys.Space) && _previousKeyState.IsKeyDown(Keys.Space);
        public bool IsInteractPressed => WasActionPressed(Keys.Enter);

        // Semantic Action: Movement (Returns a normalized vector to prevent fast diagonal movement)
        public Vector2 GetMovementDirection() {
            Vector2 direction = Vector2.Zero;

            if (_currentKeyState.IsKeyDown(Keys.W) || _currentKeyState.IsKeyDown(Keys.Up))
                direction.Y -= 1;
            if (_currentKeyState.IsKeyDown(Keys.S) || _currentKeyState.IsKeyDown(Keys.Down))
                direction.Y += 1;
            if (_currentKeyState.IsKeyDown(Keys.A) || _currentKeyState.IsKeyDown(Keys.Left))
                direction.X -= 1;
            if (_currentKeyState.IsKeyDown(Keys.D) || _currentKeyState.IsKeyDown(Keys.Right))
                direction.X += 1;

            if (direction != Vector2.Zero)
                direction.Normalize();

            return direction;
        }

        // Utility: Check for a single key press (useful for pausing, opening doors, etc.)
        public bool WasActionPressed(Keys key) {
            return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
        }

        // Example of a semantic single-press action

        public bool IsUpPressed => WasActionPressed(Keys.W) || WasActionPressed(Keys.Up);
        public bool IsDownPressed => WasActionPressed(Keys.S) || WasActionPressed(Keys.Down);
        public bool IsCancelPressed => WasActionPressed(Keys.Escape);
        public bool IsPausePressed => WasActionPressed(Keys.Escape);
    }
}