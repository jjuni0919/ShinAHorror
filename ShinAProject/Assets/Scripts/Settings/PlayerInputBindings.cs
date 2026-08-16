using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace ShinA.Settings
{
    public enum PlayerAction
    {
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        Run,
        Jump,
        Crouch
    }

    public static class PlayerInputBindings
    {
        public static Key GetKey(PlayerAction action)
        {
            string key = GetPreferenceKey(action);
            return PlayerPrefs.HasKey(key) ? (Key)PlayerPrefs.GetInt(key) : GetDefaultKey(action);
        }

        public static void SetKey(PlayerAction action, Key key)
        {
            PlayerPrefs.SetInt(GetPreferenceKey(action), (int)key);
            PlayerPrefs.Save();
        }

        public static void ResetKey(PlayerAction action)
        {
            PlayerPrefs.DeleteKey(GetPreferenceKey(action));
            PlayerPrefs.Save();
        }

        public static bool IsPressed(PlayerAction action)
        {
            KeyControl control = GetControl(action);
            return control != null && control.isPressed;
        }

        public static bool WasPressedThisFrame(PlayerAction action)
        {
            KeyControl control = GetControl(action);
            return control != null && control.wasPressedThisFrame;
        }

        private static KeyControl GetControl(PlayerAction action)
        {
            return Keyboard.current?[GetKey(action)];
        }

        private static string GetPreferenceKey(PlayerAction action)
        {
            return $"settings.controls.binding.{action}";
        }

        private static Key GetDefaultKey(PlayerAction action)
        {
            return action switch
            {
                PlayerAction.MoveForward => Key.W,
                PlayerAction.MoveBackward => Key.S,
                PlayerAction.MoveLeft => Key.A,
                PlayerAction.MoveRight => Key.D,
                PlayerAction.Run => Key.LeftShift,
                PlayerAction.Jump => Key.Space,
                PlayerAction.Crouch => Key.LeftCtrl,
                _ => Key.None
            };
        }
    }
}
