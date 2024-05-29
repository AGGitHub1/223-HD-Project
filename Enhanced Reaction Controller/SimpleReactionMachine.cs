using System;
using System.Runtime.InteropServices;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SimpleReactionMachine
{
    class SimpleReactionMachine
    {
        const string TOP_LEFT_JOINT = "┌";
        const string TOP_RIGHT_JOINT = "┐";
        const string BOTTOM_LEFT_JOINT = "└";
        const string BOTTOM_RIGHT_JOINT = "┘";
        const string TOP_JOINT = "┬";
        const string BOTTOM_JOINT = "┴";
        const string LEFT_JOINT = "├";
        const string JOINT = "┼";
        const string RIGHT_JOINT = "┤";
        const char HORIZONTAL_LINE = '─';
        const char PADDING = ' ';
        const string VERTICAL_LINE = "│";

        static private IController controller = null!;
        static private IGui gui = null!;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        const int STD_OUTPUT_HANDLE = -11;

        static bool IsConsoleAvailable()
        {
            IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
            return GetConsoleMode(handle, out _);
        }

        static void Main(string[] args)
        {
            if (IsConsoleAvailable())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0}{1}{2}", TOP_LEFT_JOINT, new string(HORIZONTAL_LINE, 50), TOP_RIGHT_JOINT);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", LEFT_JOINT, new string(HORIZONTAL_LINE, 50), RIGHT_JOINT);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
                Console.WriteLine("{0}{1}{2}", BOTTOM_LEFT_JOINT, new string(HORIZONTAL_LINE, 50), BOTTOM_RIGHT_JOINT);

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                SafeSetCursorPosition(5, 6);
                Console.Write("{0,-20}", "- For Insert Coin press SPACE");
                SafeSetCursorPosition(5, 7);
                Console.Write("{0,-20}", "- For Go/Stop action press ENTER");
                SafeSetCursorPosition(5, 8);
                Console.Write("{0,-20}", "- For Exit press ESC");
            }
            else
            {
                Console.WriteLine("Console operations are not supported in this environment.");
            }

            // Create a time for Tick event
            Timer timer = new Timer(10);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            // Connect GUI with the Controller and vice versa
            controller = new EnhancedReactionController();
            gui = new Gui();
            gui.Connect(controller);
            controller.Connect(gui, new RandomGenerator());

            // Reset the GUI
            gui.Init();
            // Start the timer
            timer.Enabled = true;

            // Run the menu
            bool quitPressed = false;
            while (!quitPressed)
            {
                if (IsConsoleAvailable())
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Enter:
                            controller.GoStopPressed();
                            break;
                        case ConsoleKey.Spacebar:
                            controller.CoinInserted();
                            break;
                        case ConsoleKey.Escape:
                            quitPressed = true;
                            break;
                    }
                }
            }
        }

        // This event occurs every 10 msec
        private static void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            controller.Tick();
        }

        // Internal implementation of Random Generator
        private class RandomGenerator : IRandom
        {
            Random rnd = new Random(100);

            public int GetRandom(int from, int to)
            {
                return rnd.Next(to - from) + from;
            }
        }

        // Internal implementation of GUI
        private class Gui : IGui
        {
            private IController controller = null!;
            public void Connect(IController controller)
            {
                this.controller = controller;
            }

            public void Init()
            {
                SetDisplay("Start your game!");
            }

            public void SetDisplay(string text)
            {
                PrintUserInterface(text);
            }

            private void PrintUserInterface(string text)
            {
                if (IsConsoleAvailable())
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    SafeSetCursorPosition(15, 2);
                    Console.Write(new string(' ', 40)); // Clear the previous text by overwriting with spaces
                    SafeSetCursorPosition(15, 2);
                    Console.Write("{0,-20}", text);
                }
                else
                {
                    Console.WriteLine(text);
                }
            }
        }

        static void SafeSetCursorPosition(int left, int top)
        {
            try
            {
                if (IsConsoleAvailable())
                {
                    Console.SetCursorPosition(left, top);
                }
            }
            catch (IOException)
            {
                // Handle exception if cursor positioning is not supported
                Console.WriteLine($"Cursor positioning not supported. Cannot set cursor to ({left}, {top}).");
            }
        }
    }
}