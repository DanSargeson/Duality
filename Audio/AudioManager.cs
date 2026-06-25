using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;

namespace Duality.Audio
{
    public class AudioManager
    {
        private SoundEffectInstance _densityHum;
        private SoundEffectInstance _insightStatic; // Replaces the ringing
        private SoundEffect _heartbeatSound;        // A single deep "thud" or digital "blip"

        private float _heartbeatTimer = 0f;

        public void LoadContent(ContentManager content) {
            _densityHum = content.Load<SoundEffect>("Audio/hum").CreateInstance();
            _densityHum.IsLooped = true;
            _densityHum.Volume = 0.5f;

            // Load a deep static, radio hiss, or digital distortion
            _insightStatic = content.Load<SoundEffect>("Audio/static").CreateInstance();
            _insightStatic.IsLooped = true;
            _insightStatic.Volume = 0f;

            // Load a single, heavy bass-kick or low digital thud
            _heartbeatSound = content.Load<SoundEffect>("Audio/heartbeat");

            _densityHum.Play();
            _insightStatic.Play();
        }

        public void Update(GameTime gameTime, float currentFrequency, float totalStress, bool isBurntOut) {
            if (_densityHum == null || _insightStatic == null) return;

            // --- THE ANTI-STEALING SAFEGUARD ---
            if (_densityHum.State != SoundState.Playing) _densityHum.Play();
            if (_insightStatic.State != SoundState.Playing) _insightStatic.Play();

            // -------------------------------------------------------------
            // KILL SWITCH (System Burnout)
            // -------------------------------------------------------------
            if (isBurntOut) {
                _insightStatic.Volume = 0f;
                _densityHum.Volume = 0.25f; // Keep the lonely hum active
                _heartbeatTimer = 0f;
                return;
            }

            // -------------------------------------------------------------
            // BAND 1: THE SAFE ZONE (0.0 to 0.4)
            // -------------------------------------------------------------
            // Hum starts at max (0.25) and fades out entirely by 0.4 frequency
            float humVolume = 1f - MathHelper.Clamp(currentFrequency / 0.4f, 0f, 1f);
            _densityHum.Volume = humVolume * 0.25f;

            // -------------------------------------------------------------
            // BAND 2: THE STALKER RIFT (0.2 to 0.8)
            // -------------------------------------------------------------
            if (currentFrequency > 0.2f && currentFrequency < 0.8f) {
                _heartbeatTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Calculate how deep into the rift we are (0.0 = just entered, 1.0 = right at Insight border)
                float riftProgress = (currentFrequency - 0.2f) / 0.6f;

                // Heartbeat speeds up the closer they get to full Insight
                float beatInterval = MathHelper.Lerp(3.2f, 0.9f, riftProgress);

                if (_heartbeatTimer >= beatInterval) {
                    float beatVol = MathHelper.Lerp(0.2f, 0.8f, riftProgress);
                    float beatPitch = MathHelper.Lerp(-0.1f, 0.4f, riftProgress);

                    _heartbeatSound.Play(beatVol, beatPitch, 0.0f);
                    _heartbeatTimer = 0f;
                }
            }
            else {
                _heartbeatTimer = 0f; // Instantly kill the heartbeat if we leave the mid-frequencies
            }

            // -------------------------------------------------------------
            // BAND 3: INSIGHT INTERACTION (0.8 to 1.0)
            // -------------------------------------------------------------
            if (currentFrequency >= 0.8f) {
                // Calculate how close we are to absolute max frequency
                float insightProgress = (currentFrequency - 0.8f) / 0.2f;

                _insightStatic.Volume = MathHelper.Lerp(0.1f, 0.4f, insightProgress);

                // We still use TotalStress here so the static sounds sicker as the strain bar fills!
                _insightStatic.Pitch = -(totalStress * 0.7f);
            }
            else {
                _insightStatic.Volume = 0f; // Instantly silence static if we drop below 0.8
            }
        }

        public void Unload() {
            _densityHum?.Stop();
            _densityHum?.Dispose();

            _insightStatic?.Stop();
            _insightStatic?.Dispose();

            _heartbeatSound?.Dispose();
        }
    }
}