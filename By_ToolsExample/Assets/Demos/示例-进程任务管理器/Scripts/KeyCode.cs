namespace Demos.示例_进程任务管理器.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public enum KeyboardState
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105,
    }

    public enum KeyCodeValue
    {
        NoKey = 0,

        VkBack = 0x08,
        VkTab = 0x09,
        VkEnter = 0x0D,
        VkShift = 0x10,
        VkCtrl = 0x11,
        VkAlt = 0x12,
        VkPause = 0x13,
        VkCapsLock = 0x14,
        VkEsc = 0x1B,
        VkSpace = 0x20,
        VkPageUp = 0x21,
        VkPageDown = 0x22,
        VkEnd = 0x23,
        VkHome = 0x24,
        VkLeft = 0x25,
        VkUp = 0x26,
        VkRight = 0x27,
        VkDown = 0x28,
        VkPrintScreen = 0x2C,
        VkInsert = 0x2D,
        VkDelete = 0x2E,

        Vk0 = 0x30,
        Vk1 = 0x31,
        Vk2 = 0x32,
        Vk3 = 0x33,
        Vk4 = 0x34,
        Vk5 = 0x35,
        Vk6 = 0x36,
        Vk7 = 0x37,
        Vk8 = 0x38,
        Vk9 = 0x39,

        VkA = 0x41,
        VkB = 0x42,
        VkC = 0x43,
        VkD = 0x44,
        VkE = 0x45,
        VkF = 0x46,
        VkG = 0x47,
        VkH = 0x48,
        VkI = 0x49,
        VkJ = 0x4A,
        VkK = 0x4B,
        VkL = 0x4C,
        VkM = 0x4D,
        VkN = 0x4E,
        VkO = 0x4F,
        VkP = 0x50,
        VkQ = 0x51,
        VkR = 0x52,
        VkS = 0x53,
        VkT = 0x54,
        VkU = 0x55,
        VkV = 0x56,
        VkW = 0x57,
        VkX = 0x58,
        VkY = 0x59,
        VkZ = 0x5A,

        VkLeftWin = 0x5B,
        VkRightWin = 0x5C,
        VkApps = 0x5D,

        VkNumpad0 = 0x60,
        VkNumpad1 = 0x61,
        VkNumpad2 = 0x62,
        VkNumpad3 = 0x63,
        VkNumpad4 = 0x64,
        VkNumpad5 = 0x65,
        VkNumpad6 = 0x66,
        VkNumpad7 = 0x67,
        VkNumpad8 = 0x68,
        VkNumpad9 = 0x69,
        VkMultiply = 0x6A,
        VkAdd = 0x6B,
        VkSubtract = 0x6D,
        VkDecimal = 0x6E,
        VkDivide = 0x6F,

        VkF1 = 0x70,
        VkF2 = 0x71,
        VkF3 = 0x72,
        VkF4 = 0x73,
        VkF5 = 0x74,
        VkF6 = 0x75,
        VkF7 = 0x76,
        VkF8 = 0x77,
        VkF9 = 0x78,
        VkF10 = 0x79,
        VkF11 = 0x7A,
        VkF12 = 0x7B,

        VkLeftShift = 0xA0,
        VkRightShift = 0xA1,
        VkLeftCtrl = 0xA2,
        VkRightCtrl = 0xA3,
        VkLeftAlt = 0xA4,
        VkRightAlt = 0xA5,

        VkOemSemicolon = 0xBA,
        VkOemPlus = 0xBB,
        VkOemComma = 0xBC,
        VkOemMinus = 0xBD,
        VkOemPeriod = 0xBE,
        VkOemQuestion = 0xBF,
        VkOemTilde = 0xC0,
        VkOemLeftBracket = 0xDB,
        VkOemBackslash = 0xDC,
        VkOemRightBracket = 0xDD,
        VkOemQuote = 0xDE,
    }

    /// <summary>
    /// Windows 虚拟键码显示表。
    /// 不使用 LINQ，避免部分 Unity 工程 API 兼容级别下 GroupBy 等扩展方法不可用。
    /// </summary>
    public static class KeyCodeName
    {
        public static readonly List<KeyValuePair<string, int>> KeyOptions = new()
        {
            new KeyValuePair<string, int>("不设置", (int)KeyCodeValue.NoKey),

            new KeyValuePair<string, int>("左Ctrl键", (int)KeyCodeValue.VkLeftCtrl),
            new KeyValuePair<string, int>("右Ctrl键", (int)KeyCodeValue.VkRightCtrl),
            new KeyValuePair<string, int>("左Alt键", (int)KeyCodeValue.VkLeftAlt),
            new KeyValuePair<string, int>("右Alt键", (int)KeyCodeValue.VkRightAlt),
            new KeyValuePair<string, int>("左Shift键", (int)KeyCodeValue.VkLeftShift),
            new KeyValuePair<string, int>("右Shift键", (int)KeyCodeValue.VkRightShift),
            new KeyValuePair<string, int>("左Win键", (int)KeyCodeValue.VkLeftWin),
            new KeyValuePair<string, int>("右Win键", (int)KeyCodeValue.VkRightWin),

            new KeyValuePair<string, int>("Tab键", (int)KeyCodeValue.VkTab),
            new KeyValuePair<string, int>("Enter键", (int)KeyCodeValue.VkEnter),
            new KeyValuePair<string, int>("Esc键", (int)KeyCodeValue.VkEsc),
            new KeyValuePair<string, int>("空格键", (int)KeyCodeValue.VkSpace),
            new KeyValuePair<string, int>("Backspace键", (int)KeyCodeValue.VkBack),
            new KeyValuePair<string, int>("CapsLock键", (int)KeyCodeValue.VkCapsLock),

            new KeyValuePair<string, int>("A键", (int)KeyCodeValue.VkA),
            new KeyValuePair<string, int>("B键", (int)KeyCodeValue.VkB),
            new KeyValuePair<string, int>("C键", (int)KeyCodeValue.VkC),
            new KeyValuePair<string, int>("D键", (int)KeyCodeValue.VkD),
            new KeyValuePair<string, int>("E键", (int)KeyCodeValue.VkE),
            new KeyValuePair<string, int>("F键", (int)KeyCodeValue.VkF),
            new KeyValuePair<string, int>("G键", (int)KeyCodeValue.VkG),
            new KeyValuePair<string, int>("H键", (int)KeyCodeValue.VkH),
            new KeyValuePair<string, int>("I键", (int)KeyCodeValue.VkI),
            new KeyValuePair<string, int>("J键", (int)KeyCodeValue.VkJ),
            new KeyValuePair<string, int>("K键", (int)KeyCodeValue.VkK),
            new KeyValuePair<string, int>("L键", (int)KeyCodeValue.VkL),
            new KeyValuePair<string, int>("M键", (int)KeyCodeValue.VkM),
            new KeyValuePair<string, int>("N键", (int)KeyCodeValue.VkN),
            new KeyValuePair<string, int>("O键", (int)KeyCodeValue.VkO),
            new KeyValuePair<string, int>("P键", (int)KeyCodeValue.VkP),
            new KeyValuePair<string, int>("Q键", (int)KeyCodeValue.VkQ),
            new KeyValuePair<string, int>("R键", (int)KeyCodeValue.VkR),
            new KeyValuePair<string, int>("S键", (int)KeyCodeValue.VkS),
            new KeyValuePair<string, int>("T键", (int)KeyCodeValue.VkT),
            new KeyValuePair<string, int>("U键", (int)KeyCodeValue.VkU),
            new KeyValuePair<string, int>("V键", (int)KeyCodeValue.VkV),
            new KeyValuePair<string, int>("W键", (int)KeyCodeValue.VkW),
            new KeyValuePair<string, int>("X键", (int)KeyCodeValue.VkX),
            new KeyValuePair<string, int>("Y键", (int)KeyCodeValue.VkY),
            new KeyValuePair<string, int>("Z键", (int)KeyCodeValue.VkZ),

            new KeyValuePair<string, int>("0键", (int)KeyCodeValue.Vk0),
            new KeyValuePair<string, int>("1键", (int)KeyCodeValue.Vk1),
            new KeyValuePair<string, int>("2键", (int)KeyCodeValue.Vk2),
            new KeyValuePair<string, int>("3键", (int)KeyCodeValue.Vk3),
            new KeyValuePair<string, int>("4键", (int)KeyCodeValue.Vk4),
            new KeyValuePair<string, int>("5键", (int)KeyCodeValue.Vk5),
            new KeyValuePair<string, int>("6键", (int)KeyCodeValue.Vk6),
            new KeyValuePair<string, int>("7键", (int)KeyCodeValue.Vk7),
            new KeyValuePair<string, int>("8键", (int)KeyCodeValue.Vk8),
            new KeyValuePair<string, int>("9键", (int)KeyCodeValue.Vk9),

            new KeyValuePair<string, int>("F1键", (int)KeyCodeValue.VkF1),
            new KeyValuePair<string, int>("F2键", (int)KeyCodeValue.VkF2),
            new KeyValuePair<string, int>("F3键", (int)KeyCodeValue.VkF3),
            new KeyValuePair<string, int>("F4键", (int)KeyCodeValue.VkF4),
            new KeyValuePair<string, int>("F5键", (int)KeyCodeValue.VkF5),
            new KeyValuePair<string, int>("F6键", (int)KeyCodeValue.VkF6),
            new KeyValuePair<string, int>("F7键", (int)KeyCodeValue.VkF7),
            new KeyValuePair<string, int>("F8键", (int)KeyCodeValue.VkF8),
            new KeyValuePair<string, int>("F9键", (int)KeyCodeValue.VkF9),
            new KeyValuePair<string, int>("F10键", (int)KeyCodeValue.VkF10),
            new KeyValuePair<string, int>("F11键", (int)KeyCodeValue.VkF11),
            new KeyValuePair<string, int>("F12键", (int)KeyCodeValue.VkF12),

            new KeyValuePair<string, int>("小键盘0键", (int)KeyCodeValue.VkNumpad0),
            new KeyValuePair<string, int>("小键盘1键", (int)KeyCodeValue.VkNumpad1),
            new KeyValuePair<string, int>("小键盘2键", (int)KeyCodeValue.VkNumpad2),
            new KeyValuePair<string, int>("小键盘3键", (int)KeyCodeValue.VkNumpad3),
            new KeyValuePair<string, int>("小键盘4键", (int)KeyCodeValue.VkNumpad4),
            new KeyValuePair<string, int>("小键盘5键", (int)KeyCodeValue.VkNumpad5),
            new KeyValuePair<string, int>("小键盘6键", (int)KeyCodeValue.VkNumpad6),
            new KeyValuePair<string, int>("小键盘7键", (int)KeyCodeValue.VkNumpad7),
            new KeyValuePair<string, int>("小键盘8键", (int)KeyCodeValue.VkNumpad8),
            new KeyValuePair<string, int>("小键盘9键", (int)KeyCodeValue.VkNumpad9),
            new KeyValuePair<string, int>("小键盘*键", (int)KeyCodeValue.VkMultiply),
            new KeyValuePair<string, int>("小键盘+键", (int)KeyCodeValue.VkAdd),
            new KeyValuePair<string, int>("小键盘-键", (int)KeyCodeValue.VkSubtract),
            new KeyValuePair<string, int>("小键盘.键", (int)KeyCodeValue.VkDecimal),
            new KeyValuePair<string, int>("小键盘/键", (int)KeyCodeValue.VkDivide),

            new KeyValuePair<string, int>("PageUp键", (int)KeyCodeValue.VkPageUp),
            new KeyValuePair<string, int>("PageDown键", (int)KeyCodeValue.VkPageDown),
            new KeyValuePair<string, int>("Home键", (int)KeyCodeValue.VkHome),
            new KeyValuePair<string, int>("End键", (int)KeyCodeValue.VkEnd),
            new KeyValuePair<string, int>("Insert键", (int)KeyCodeValue.VkInsert),
            new KeyValuePair<string, int>("Delete键", (int)KeyCodeValue.VkDelete),
            new KeyValuePair<string, int>("左方向键", (int)KeyCodeValue.VkLeft),
            new KeyValuePair<string, int>("上方向键", (int)KeyCodeValue.VkUp),
            new KeyValuePair<string, int>("右方向键", (int)KeyCodeValue.VkRight),
            new KeyValuePair<string, int>("下方向键", (int)KeyCodeValue.VkDown),
            new KeyValuePair<string, int>("PrintScreen键", (int)KeyCodeValue.VkPrintScreen),
        };

        public static readonly Dictionary<string, int> keyCodeDic = BuildKeyCodeDictionary();
        public static readonly string[] KeyNames = BuildKeyNameArray();

        private static Dictionary<string, int> BuildKeyCodeDictionary()
        {
            var result = new Dictionary<string, int>();
            for (int i = 0; i < KeyOptions.Count; i++)
            {
                string keyName = KeyOptions[i].Key;
                if (!result.ContainsKey(keyName))
                {
                    result.Add(keyName, KeyOptions[i].Value);
                }
            }

            return result;
        }

        private static string[] BuildKeyNameArray()
        {
            var result = new string[KeyOptions.Count];
            for (int i = 0; i < KeyOptions.Count; i++)
            {
                result[i] = KeyOptions[i].Key;
            }

            return result;
        }

        public static int GetKeyValueByIndex(int index)
        {
            if (KeyOptions == null || KeyOptions.Count == 0)
            {
                return 0;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= KeyOptions.Count)
            {
                index = KeyOptions.Count - 1;
            }

            return KeyOptions[index].Value;
        }

        public static string GetKeyNameByIndex(int index)
        {
            if (KeyNames == null || KeyNames.Length == 0)
            {
                return "无";
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= KeyNames.Length)
            {
                index = KeyNames.Length - 1;
            }

            return KeyNames[index];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInputEvent
    {
        public int VirtualCode;
        public int HardwareScanCode;
        public int Flags;
        public int TimeStamp;
        public IntPtr AdditionalInformation;
    }
}
