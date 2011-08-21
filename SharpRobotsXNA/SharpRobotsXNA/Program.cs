using System;

namespace SharpRobotsXNA
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SharpRobotsGame game = new SharpRobotsGame())
            {
                game.Run();
            }
        }
    }
#endif
}

