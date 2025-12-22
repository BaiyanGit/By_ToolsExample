namespace KeyPad
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// 按钮状态
    /// </summary>
    public enum KeyboardState
    {
        KeyUp = 0x0101, // 257 = 抬起
        KeyDown = 0x0100, // 256 = 按下
        SysKeyUp = 0x0105, // 261 = 抬起
        SysKeyDown = 0x0104 // 260 = 按下
    }

    public class KeyCodeName
    {
        public static Dictionary<string, int> keyCodeDic = new()
        {
            { "不设置", 0 },
            // 字母键
            { "A键", 65 },
            { "B键", 66 },
            { "C键", 67 },
            { "D键", 68 },
            { "E键", 69 },
            { "F键", 70 },
            { "G键", 71 },
            { "H键", 72 },
            { "I键", 73 },
            { "J键", 74 },
            { "K键", 75 },
            { "L键", 76 },
            { "M键", 77 },
            { "N键", 78 },
            { "O键", 79 },
            { "P键", 80 },
            { "Q键", 81 },
            { "R键", 82 },
            { "S键", 83 },
            { "T键", 84 },
            { "U键", 85 },
            { "V键", 86 },
            { "W键", 87 },
            { "X键", 88 },
            { "Y键", 89 },
            { "Z键", 90 },

            // 数字键
            { " 0键", 48 },
            { " 1键", 49 },
            { " 2键", 50 },
            { " 3键", 51 },
            { " 4键", 52 },
            { " 5键", 53 },
            { " 6键", 54 },
            { " 7键", 55 },
            { " 8键", 56 },
            { " 9键", 57 },

            // 函数键
            { "F1键", 112 },
            { "F2键", 113 },
            { "F3键", 114 },
            { "F4键", 115 },
            { "F5键", 116 },
            { "F6键", 117 },
            { "F7键", 118 },
            { "F8键", 119 },
            { "F9键", 120 },
            { "F10键", 121 },
            { "F11键", 122 },
            { "F12键", 123 },

            // 组合键
            { "左Ctrl键", 162 },
            { "右Ctrl键", 163 },
            { "左Alt键", 163 },
            { "右Alt键", 163 },
            { "Win键", 91 },
            { "FN键", 93 },
            { "Tap键", 9 },
            { "大小写键", 20 },

            // 控制键
            { " 空格键", 32 },
            { " Page Up页上键", 33 },
            { " Page Down页下键 ", 34 },
            { " End键", 35 },
            { " Home键", 36 },
            { " 左方向键", 37 },
            { " 上方向键", 38 },
            { " 右方向键", 39 },
            { " 下方向键", 40 },
            { " Select键", 41 },
            { " Print屏幕打印键", 42 },
            { " Execute执行键", 43 },
            { " Print Screen键 ", 44 },
            { " Insert键", 45 },
            { " Delete键", 46 },
            { " Help键", 47 },

            // 小键盘按键
            { "小键盘0键", 96 },
            { "小键盘1键", 97 },
            { "小键盘2键", 98 },
            { "小键盘3键", 99 },
            { "小键盘4键", 100 },
            { "小键盘5键", 101 },
            { "小键盘6键", 102 },
            { "小键盘7键", 103 },
            { "小键盘8键", 104 },
            { "小键盘9键", 105 },
            { "小键盘*键", 106 },
            { "小键盘+键", 107 },
            { "小键盘Separator键", 108 },
            { "小键盘-键", 109 },
            { "小键盘.键", 110 },
            { "小键盘/键", 111 },

            // OEM特定键
            { "分号:键;", 186 },
            { "等号=键", 187 },
            { "逗号,键", 188 },
            { "连字号-键", 189 },
            { "句号.键", 190 },
            { "斜杠/问号键", 191 },
            { "波浪号~键", 192 },
            { "左方括号[键", 219 },
            { @"反斜杠\键", 220 },
            { "右方括号]键", 221 },
            { "单引号'键", 222 },
            { "(保留)      ", 223 },
        };
    }

    public enum KeyCodeValue
    {
        NoKey = 0,

        #region 字母键

        VkA = 0x41, // 65 = A键
        VkB = 0x42, // 66 = B键
        VkC = 0x43, // 67 = C键
        VkD = 0x44, // 68 = D键
        VkE = 0x45, // 69 = E键
        VkF = 0x46, // 70 = F键
        VkG = 0x47, // 71 = G键
        VkH = 0x48, // 72 = H键
        VkI = 0x49, // 73 = I键
        VkJ = 0x4A, // 74 = J键
        VkK = 0x4B, // 75 = K键
        VkL = 0x4C, // 76 = L键
        VkM = 0x4D, // 77 = M键
        VkN = 0x4E, // 78 = N键
        VkO = 0x4F, // 79 = O键
        VkP = 0x50, // 80 = P键
        VkQ = 0x51, // 81 = Q键
        VkR = 0x52, // 82 = R键
        VkS = 0x53, // 83 = S键
        VkT = 0x54, // 84 = T键
        VkU = 0x55, // 85 = U键
        VkV = 0x56, // 86 = V键
        VkW = 0x57, // 87 = W键
        VkX = 0x58, // 88 = X键
        VkY = 0x59, // 89 = Y键
        VkZ = 0x5A, // 90 = Z键

        #endregion

        #region 数字键

        Vk0 = 0x30, // 48 = 0键
        Vk1 = 0x31, // 49 = 1键 
        Vk2 = 0x32, // 50 = 2键
        Vk3 = 0x33, // 51 = 3键
        Vk4 = 0x34, // 52 = 4键
        Vk5 = 0x35, // 53 = 5键
        Vk6 = 0x36, // 54 = 6键
        Vk7 = 0x37, // 55 = 7键
        Vk8 = 0x38, // 56 = 8键
        Vk9 = 0x39, // 57 = 9键

        #endregion

        #region 函数键

        VkF1 = 0x70, // 112 = F1键
        VkF2 = 0x71, // 113 = F2键
        VkF3 = 0x72, // 114 = F3键
        VkF4 = 0x73, // 115 = F4键
        VkF5 = 0x74, // 116 = F5键
        VkF6 = 0x75, // 117 = F6键
        VkF7 = 0x76, // 118 = F7键
        VkF8 = 0x77, // 119 = F8键
        VkF9 = 0x78, // 120 = F9键
        VkF10 = 0x79, // 121 = F10键
        VkF11 = 0x7A, // 122 = F11键
        VkF12 = 0x7B, // 123 = F12键

        #endregion

        #region 组合键

        VkLCtrl = 0xA2, //  162 = 左Ctrl键
        VkRCtrl = 0xA3, //  163 = 右Ctrl键
        VkLAlt = 0xA2, //  163 = 左Alt键
        VkRAlt = 0xA2, //  163 = 右Alt键
        VkWin = 0x5B, // 91 = Win键
        VkFn = 0x5D, // 93 = FN键
        VkTap = 0x9, // 9 = tap键
        VkUpToLow = 0x14, // 20 = 大小写

        #endregion

        #region 控制键

        VkSpace = 0x20, // 32 = 空格键
        VkPrior = 0x21, // 33 = Page Up页上键
        VkNext = 0x22, // 34 = Page Down页下键  
        VkEnd = 0x23, // 35 = End键
        VkHome = 0x24, // 36 = Home键
        VkLeft = 0x25, // 37 = 左方向键
        VkUp = 0x26, // 38 = 上方向键
        VkRight = 0x27, // 39 = 右方向键
        VkDown = 0x28, // 40 = 下方向键 
        VkSelect = 0x29, // 41 = Select键
        VkPrint = 0x2A, // 42 = Print屏幕打印键
        VkExecute = 0x2B, // 43 = Execute执行键
        VkSnapshot = 0x2C, // 44 = Print Screen键  
        VkInsert = 0x2D, // 45 = Insert键
        VkDelete = 0x2E, // 46 = Delete键
        VkHelp = 0x2F, // 47 = Help键

        #endregion

        #region 小键盘按键

        VkNumpad0 = 0x60, // 96 = 小键盘0键
        VkNumpad1 = 0x61, // 97 = 小键盘1键
        VkNumpad2 = 0x62, // 98 = 小键盘2键
        VkNumpad3 = 0x63, // 99 = 小键盘3键
        VkNumpad4 = 0x64, // 100 = 小键盘4键
        VkNumpad5 = 0x65, // 101 = 小键盘5键
        VkNumpad6 = 0x66, // 102 = 小键盘6键
        VkNumpad7 = 0x67, // 103 = 小键盘7键
        VkNumpad8 = 0x68, // 104 = 小键盘8键
        VkNumpad9 = 0x69, // 105 = 小键盘9键
        VkMultiply = 0x6A, // 106 = 小键盘*键
        VkAdd = 0x6B, // 107 = 小键盘+键
        VkSeparator = 0x6C, // 108 = 小键盘Separator键
        VkSubtract = 0x6D, // 109 = 小键盘-键
        VkDecimal = 0x6E, // 110 = 小键盘.键
        VkDivide = 0x6F, // 111 = 小键盘/键

        #endregion

        #region OEM特定键

        VkOem1 = 0xBA, // 186 = 分号:键;
        VkOemPlus = 0xBB, // 187 = 等号=键
        VkOemComma = 0xBC, // 188 = 逗号,键
        VkOemMinus = 0xBD, // 189 = 连字号-键
        VkOemPeriod = 0xBE, // 190 = 句号.键
        VkOem2 = 0xBF, // 191 = 斜杠/问号键
        VkOem3 = 0xC0, // 192 = 波浪号~键
        VkOem4 = 0xDB, // 219 = 左方括号[键
        VkOem5 = 0xDC, // 220 = 反斜杠\键
        VkOem6 = 0xDD, // 221 = 右方括号]键
        VkOem7 = 0xDE, // 222 = 单引号'键
        VkOem8 = 0xDF, //  223= (保留)

        #endregion

        #region 其他键

        VkFinal = 0x18, // 24 = Final键
        VkHanja = 0x19, // 25 = Hanja键
        VkConvert = 0x1C, // 28 = Convert键
        VkNonconvert = 0x1D, // 29 = NonConvert键
        VkAccept = 0x1E, // 30 = Accept键 
        VkModechange = 0x1F // 31 = ModeChange键

        #endregion
    }


    /// <summary>
    /// 自键盘的原始输入数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInputEvent
    {
        /// <summary>
        /// 按键的虚拟键码,范围是1到254之间的整数。
        /// </summary>
        public int VirtualCode;

        /// <summary>
        /// 按键的硬件扫描码。
        /// </summary>
        public int HardwareScanCode;

        /// <summary>
        /// 扩展键标志、注入事件标志、上下文代码和转换状态标志等信息的组合。可以根据位进行检查,判断事件是否被注入等。
        /// </summary>
        public int Flags;

        /// <summary>
        /// 此消息的时间戳,相当于GetMessageTime对此消息返回的值。
        /// </summary>
        public int TimeStamp;

        /// <summary>
        /// 与消息关联的额外信息。
        /// </summary>
        public IntPtr AdditionalInformation;
    }
}