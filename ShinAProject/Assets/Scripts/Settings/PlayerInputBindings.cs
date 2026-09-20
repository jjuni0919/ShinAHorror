using System;
using System.Collections.Generic;
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
        Crouch,
        Interact,
        Tablet,
        WorldInteract,
        DropItem
    }

    public static class PlayerInputBindings
    {
        private static readonly Dictionary<PlayerAction, Key> CachedBindings = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            CachedBindings.Clear();
        }

        public static Key GetKey(PlayerAction action)
        {
            if (CachedBindings.TryGetValue(action, out Key cachedKey))
            {
                return cachedKey;
            }

            if (!Enum.IsDefined(typeof(PlayerAction), action))
            {
                return Key.None;
            }

            string key = GetPreferenceKey(action);
            Key binding = PlayerPrefs.HasKey(key)
                ? GetStoredKey(PlayerPrefs.GetInt(key), GetDefaultKey(action))
                : GetDefaultKey(action);
            CachedBindings[action] = binding;
            return binding;
        }

        public static void SetKey(PlayerAction action, Key key)
        {
            if (!Enum.IsDefined(typeof(PlayerAction), action))
            {
                return;
            }

            Key binding = Enum.IsDefined(typeof(Key), key) ? key : Key.None;
            PlayerPrefs.SetInt(GetPreferenceKey(action), (int)binding);
            CachedBindings[action] = binding;
            PlayerPrefs.Save();
        }

        public static void ResetKey(PlayerAction action)
        {
            if (!Enum.IsDefined(typeof(PlayerAction), action))
            {
                return;
            }

            PlayerPrefs.DeleteKey(GetPreferenceKey(action));
            CachedBindings.Remove(action);
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
            Keyboard keyboard = Keyboard.current;
            Key key = GetKey(action);
            return keyboard == null || key == Key.None ? null : keyboard[key];
        }

        private static Key GetStoredKey(int value, Key fallback)
        {
            return Enum.IsDefined(typeof(Key), value) ? (Key)value : fallback;
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
                PlayerAction.Interact => Key.E,
                PlayerAction.WorldInteract => Key.F,
                PlayerAction.Tablet => Key.Tab,
                PlayerAction.DropItem => Key.Q,
                _ => Key.None
            };
        }
    }
}
