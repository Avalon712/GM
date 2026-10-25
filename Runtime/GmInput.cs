#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
using UnityEngine;

namespace Game.GM
{
    /// <summary>
    /// 键盘和鼠标状态。有旧输入系统时用 <c>UnityEngine.Input</c>，只有关掉旧输入时才用 Input System。
    /// </summary>
    internal static class GmInput
    {
        public static bool GetKey(KeyCode key)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key);
#else
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !TryMap(key, out Key mapped))
                return false;

            return keyboard[mapped].isPressed;
#endif
        }

        public static bool GetMouseButton(int button)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(button);
#else
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return false;

            switch (button)
            {
                case 1:
                    return mouse.rightButton.isPressed;
                case 2:
                    return mouse.middleButton.isPressed;
                default:
                    return mouse.leftButton.isPressed;
            }
#endif
        }

#if !ENABLE_LEGACY_INPUT_MANAGER
        private static bool TryMap(KeyCode key, out Key mapped)
        {
            string name;
            switch (key)
            {
                case KeyCode.Return:
                    name = "Enter";
                    break;
                case KeyCode.KeypadEnter:
                    name = "NumpadEnter";
                    break;
                case KeyCode.LeftControl:
                    name = "LeftCtrl";
                    break;
                case KeyCode.RightControl:
                    name = "RightCtrl";
                    break;
                case KeyCode.Keypad0:
                    name = "Numpad0";
                    break;
                case KeyCode.Keypad1:
                    name = "Numpad1";
                    break;
                case KeyCode.Keypad2:
                    name = "Numpad2";
                    break;
                case KeyCode.Keypad3:
                    name = "Numpad3";
                    break;
                case KeyCode.Keypad4:
                    name = "Numpad4";
                    break;
                case KeyCode.Keypad5:
                    name = "Numpad5";
                    break;
                case KeyCode.Keypad6:
                    name = "Numpad6";
                    break;
                case KeyCode.Keypad7:
                    name = "Numpad7";
                    break;
                case KeyCode.Keypad8:
                    name = "Numpad8";
                    break;
                case KeyCode.Keypad9:
                    name = "Numpad9";
                    break;
                case KeyCode.KeypadPeriod:
                    name = "NumpadPeriod";
                    break;
                case KeyCode.KeypadDivide:
                    name = "NumpadDivide";
                    break;
                case KeyCode.KeypadMultiply:
                    name = "NumpadMultiply";
                    break;
                case KeyCode.KeypadMinus:
                    name = "NumpadMinus";
                    break;
                case KeyCode.KeypadPlus:
                    name = "NumpadPlus";
                    break;
                case KeyCode.KeypadEquals:
                    name = "NumpadEquals";
                    break;
                case KeyCode.Alpha0:
                    name = "Digit0";
                    break;
                case KeyCode.Alpha1:
                    name = "Digit1";
                    break;
                case KeyCode.Alpha2:
                    name = "Digit2";
                    break;
                case KeyCode.Alpha3:
                    name = "Digit3";
                    break;
                case KeyCode.Alpha4:
                    name = "Digit4";
                    break;
                case KeyCode.Alpha5:
                    name = "Digit5";
                    break;
                case KeyCode.Alpha6:
                    name = "Digit6";
                    break;
                case KeyCode.Alpha7:
                    name = "Digit7";
                    break;
                case KeyCode.Alpha8:
                    name = "Digit8";
                    break;
                case KeyCode.Alpha9:
                    name = "Digit9";
                    break;
                default:
                    name = key.ToString();
                    break;
            }

            return System.Enum.TryParse(name, out mapped);
        }
#endif
    }
}
