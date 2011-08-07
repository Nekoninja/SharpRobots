#region Apache License
/*
   Copyright 2011 Fred A. Rosenbaum Jr.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License. 
 */
#endregion
using System;
using System.Threading;
using SharpRobotsEngine;

namespace SharpRobotsConsole
{
    class Program
    {
        #region Fields

        private const int ScreenWidth = 100;
        private const int ScreenHeight = 40;
        private const int ArenaWidth = 80;
        private const int ArenaHeight = 40;
        private static double _scaleX;
        private static double _scaleY;
        private static int _cycles;
        private static BattleEngine _battleEngine;

        #endregion

        #region Method: Main

        /// <summary>
        /// No options other than paths/filenames to the C# source
        /// for each robot
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You must provide the robot files you want to compile");
                return;
            }

            Init();

            // Load, compile and instantiate the robots given on the command line
            if (_battleEngine.Load(args))
            {
                // Execute battle until one is left standing
                while (true)
                {
                    if (!_battleEngine.Execute())
                    {
                        // Display winner
                        return;
                    }

                    _cycles++;
                    UpdateDisplay();

                    // Check for quit 'Q' key
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);

                        if (consoleKeyInfo.Key == ConsoleKey.Q)
                            return;
                    }

                    Thread.Sleep(20);
                }
            }

            foreach (var error in _battleEngine.Errors)
                Console.WriteLine(error);

            return;
        }

        #endregion

        #region Method: Init

        /// <summary>
        /// Setup Console and battle engine
        /// </summary>
        private static void Init()
        {
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += CtrlCHandler;
            Console.CursorVisible = false;

            // Change the console size
            Console.WindowWidth = ScreenWidth;
            Console.WindowHeight = ScreenHeight;

            _battleEngine = new BattleEngine();

            // Determine proper scaling ratio from the robots world coordinates to the screen coordinates
            // Screen width  80 / Battle Field width  1000 = 0.08
            // Screen height 24 / Battle Field height 1000 = 0.024
            _scaleX = (double)ArenaWidth / Arena.ArenaWidth;
            _scaleY = (double)ArenaHeight / Arena.ArenaHeight;

            // Render the right Wall
            for (int y = 0; y < ArenaHeight; y++)
            {
                Console.SetCursorPosition(ArenaWidth, y);
                Console.Write("|");
            }
        }

        #endregion

        #region Method: UpdateDisplay

        /// <summary>
        /// Display next cycle of robot battles to screen
        /// </summary>
        private static void UpdateDisplay()
        {
            int botNum = 0;
            int yPos = 0;

            foreach (var bot in _battleEngine.Bots)
            {
                int x = (int) (bot.Location.X * _scaleX);
                int y = (int) (bot.Location.Y * _scaleY);
                int lastX = (int)(bot.LastLocation.X * _scaleX);
                int lastY = (int)(bot.LastLocation.Y * _scaleY);

                Console.SetCursorPosition(lastX, lastY);
                Console.Write(" ");
                Console.SetCursorPosition(x, y);
                Console.Write("O");

                // Bot info
                Console.SetCursorPosition(ArenaWidth + 1, yPos);
                Console.Write(String.Format("{0} {1}", botNum+1, bot.ClassName.Length < 12 ? bot.ClassName : bot.ClassName.Substring(0, 12)));
                Console.SetCursorPosition(ArenaWidth + 1, yPos + 1);
                Console.Write(String.Format(" D% {0,3} Sc {1,3}", bot.Damage, bot.ScanDirection));
                Console.SetCursorPosition(ArenaWidth + 1, yPos + 2);
                Console.Write(String.Format(" Sp {0,3} Hd {1,3}", bot.Speed, bot.Direction));

                botNum++;
                yPos += 5;
            }

            // Bottom info
            Console.SetCursorPosition(ArenaWidth + 1, ArenaHeight - 1);
            Console.Write("Cycle: " + _cycles);
        }

        #endregion

        #region Method: CtrlCHandler

        /// <summary>
        /// Exit when user presses Ctrl + C
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected static void CtrlCHandler(object sender, ConsoleCancelEventArgs args)
        {
            Environment.Exit(1);
        }

        #endregion
    }
}
