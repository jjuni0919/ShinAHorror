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
            get => PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            set
            {
                float clamped = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(MasterVolumeKey, clamped);
                AudioListener.volume = clamped;
                PlayerPrefs.Save();
            }
        }

        public static float MouseSensitivityNormalized
        {
            get => PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivity);
            set
            {
                PlayerPrefs.SetFloat(MouseSensitivityKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        public static float LookSensitivity => Mathf.Lerp(
            MinimumLookSensitivity, MaximumLookSensitivity, MouseSensitivityNormalized);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySavedSettings()
        {
            AudioListener.volume = MasterVolume;
        }
    }
}
