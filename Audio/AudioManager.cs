using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;

namespace Duality.Audio
{
    public class AudioManager
    {
        private SoundEffectInstance _densityHum;
        private SoundEffectInstance _insightRing;

        public void LoadContent(ContentManager content) {
            // Load the raw audio files (.wav or .mp3 compiled through the MGCB tool)
            SoundEffect humEffect = content.Load<SoundEffect>("Audio/hum");
            SoundEffect ringEffect = content.Load<SoundEffect>("Audio/ringing");

            // Create manipulate-able instances
            _densityHum = humEffect.CreateInstance();
            _densityHum.IsLooped = true;
            _densityHum.Volume = 0.5f;

            _insightRing = ringEffect.CreateInstance();
            _insightRing.IsLooped = true;
            _insightRing.Volume = 0f;

            // Start playing both silently/low immediately
            _densityHum.Play();
            _insightRing.Play();
        }

        public void Update(float currentFrequency, float totalStress) {
            if (_densityHum == null || _insightRing == null) return;

            // 1. Density Hum: Loudest at 0.0, fades out completely by 0.8
            float humVolume = 1f - Math.Min(currentFrequency / 0.8f, 1f);
            _densityHum.Volume = humVolume * 0.5f; // 0.5f is the base volume cap

            // 2. Insight Ring: Starts fading in at 0.2, peaks at 1.0
            //float ringVolume = Math.Max(0, (currentFrequency - 0.2f) / 0.8f);
            //_insightRing.Volume = ringVolume * 0.4f;

            //// 3. Pitch Modulation based on Stress
            //// MonoGame pitch goes from -1.0f (down an octave) to 1.0f (up an octave).
            //// As stress peaks, the ringing pitches up to become highly oppressive.
            //float ringPitch = totalStress * 0.7f;
            //_insightRing.Pitch = ringPitch;
        }

        // Clean up when leaving the scene
        public void Unload() {
            _densityHum?.Stop();
            _densityHum?.Dispose();

            _insightRing?.Stop();
            _insightRing?.Dispose();
        }
    }
}