using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Duality.Rendering
{

    public class Camera
    {
        public Vector2 Position { get; private set; }
       // private Viewport _viewport;

        private int _virtualWidth;
        private int _virtualHeight;

        public Camera(int virtualWidth, int virtualHeight) {
            _virtualWidth = virtualWidth;
            _virtualHeight = virtualHeight;
            Position = Vector2.Zero;
        }

        
        public void Follow(Vector2 targetPosition, float deltaTime, Rectangle levelBounds) {

            Vector2 desiredPosition = Vector2.Lerp(Position, targetPosition, 5f * deltaTime);

            // Calculate the limits so the edge of the screen doesn't pass the edge of the level
            float minX = levelBounds.X + (_virtualWidth / 2f);
            float maxX = levelBounds.Width - (_virtualWidth / 2f);
            float minY = levelBounds.Y + (_virtualHeight / 2f);
            float maxY = levelBounds.Height - (_virtualHeight / 2f);

            // Fallback in case the screen is actually larger than the level itself
            if (maxX < minX) maxX = minX;
            if (maxY < minY) maxY = minY;

            // Clamp the camera's position to stay within those limits
            desiredPosition.X = MathHelper.Clamp(desiredPosition.X, minX, maxX);
            desiredPosition.Y = MathHelper.Clamp(desiredPosition.Y, minY, maxY);

            Position = desiredPosition;
        }

        public Matrix GetTransform(Vector2 shakeOffset) {
            // 1. Move the world negatively so the camera's target position becomes (0,0)
            // 2. Add the chaotic screen shake offset
            // 3. Move the world positively by half the screen dimensions so (0,0) sits perfectly in the middle of your monitor
            return Matrix.CreateTranslation(new Vector3(-Position.X + shakeOffset.X, -Position.Y + shakeOffset.Y, 0)) *
                   Matrix.CreateTranslation(new Vector3(_virtualWidth / 2f, _virtualHeight / 2f, 0));
        }
    }
}