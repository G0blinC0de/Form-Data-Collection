namespace Helios.Relay
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    public static class QuickEditModeOptions
    {
        public const int STD_INPUT_HANDLE = -10;

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int ioMode);

        /// <summary>
        ///     This flag enables the user to use the mouse to select and edit text. To enable this option, you must also set
        ///     the ExtendedFlags flag.
        /// </summary>
        private const int QUICK_EDIT_MODE = 64;

        /// <summary>ExtendedFlags must be enabled in order to enable InsertMode or QuickEditMode.</summary>
        private const int EXTENDED_FLAGS = 128;

        public static bool DisableQuickEdit()
        {
            var conHandle = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(conHandle, out var mode))
            {
                var error = Marshal.GetLastWin32Error();
                var errorMessage = new Win32Exception(error).Message;
                Console.WriteLine(errorMessage);
                return false;
            }

            mode &= ~(QUICK_EDIT_MODE | EXTENDED_FLAGS);

            return SetConsoleMode(conHandle, mode);
        }

        public static bool EnableQuickEdit()
        {
            var conHandle = GetStdHandle(STD_INPUT_HANDLE);

            if (!GetConsoleMode(conHandle, out var mode))
            {
                var error = Marshal.GetLastWin32Error();
                var errorMessage = new Win32Exception(error).Message;
                Console.WriteLine(errorMessage);
                return false;
            }

            mode |= QUICK_EDIT_MODE | EXTENDED_FLAGS;

            return SetConsoleMode(conHandle, mode);
        }
    }
}