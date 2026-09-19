using UnityEngine;

namespace ShinA.Settings
{
    public static class GameSettings
    {
        private const string MasterVolumeKey = "settings.audio.masterVolume";
        private const string MouseSensitivityKey = "settings.controls.mouseSensitivity";

        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultMouseSensitivity = 0.3f;
        public const float MinimumLookSensitivity = 0.03f;
        public const float MaximumLookSensitivity = 0.35f;

        public static float MasterVolume
        {
            get => GetNormalizedValue(MasterVolumeKey, DefaultMasterVolume);
            set
            {
                float clamped = Normalize(value, DefaultMasterVolume);
                PlayerPrefs.SetFloat(MasterVolumeKey, clamped);
                AudioListener.volume = clamped;
            }
        }

        public static float MouseSensitivityNormalized
        {
            get => GetNormalizedValue(MouseSensitivityKey, DefaultMouseSensitivity);
            set
            {
                float clamped = Normalize(value, DefaultMouseSensitivity);
                PlayerPrefs.SetFloat(MouseSensitivityKey, clamped);
            }
        }

        public static float LookSensitivity => Mathf.Lerp(
            MinimumLookSensitivity, MaximumLookSensitivity, MouseSensitivityNormalized);

        internal static void SaveChanges()
        {
            PlayerPrefs.Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySavedSettings()
        {
            AudioListener.volume = MasterVolume;
        }

        private static float GetNormalizedValue(string key, float fallback)
        {
            return Normalize(PlayerPrefs.GetFloat(key, fallback), fallback);
        }

        private static float Normalize(float value, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp01(value);
        }
    }
}
