// Always include the below
using System;
using SharpRobotsEngine;

// Always use this namespace
namespace RobotScript
{
    /// <summary>
    /// Our robot class name, needs to be unique from all other robots that will compete
    /// Always derive our class from the Robot base class
    /// </summary>
    public class ExampleBot : Robot
    {
        /// <summary>
        /// Init
        /// First of the two required methods we must implement
        /// The BotEngine will call this method one time after loading and compiling our robot
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// Execute
        /// The second of the two required methods to implement
        /// The BotEngine will call this method once each game cycle allowing us
        /// to implement/execute our robot logic
        /// </summary>
        public void Execute()
        {
            int range = Arena.Scan(this, 0, 10);
        }
    }
}