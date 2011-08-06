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
    public class Sniper2 : Robot
    {
        private int Direction { get; set; }

        /// <summary>
        /// Init
        /// First of the two required methods we must implement
        /// The BotEngine will call this method one time after loading and compiling our robot
        /// </summary>
        public void Init()
        {
            Direction = 90;
        }

        /// <summary>
        /// Execute
        /// The second of the two required methods to implement
        /// The BotEngine will call this method once each game cycle allowing us
        /// to implement/execute our robot logic
        /// 
        /// The sniper bot simply travels back and forth east and west, changing direction
        /// before hitting the arena wall
        /// </summary>
        public void Execute()
        {
            // If speed is zero, start moving
            if (Arena.Speed(this) == 0)
            {
                Arena.Drive(this, Direction, 50);
            }

            // If about to hit a wall, change direction 180 degrees
            if (Arena.LocationX(this) < 50 || Arena.LocationX(this) > Arena.ArenaWidth - 50)
            {
                Direction = Direction == 90 ? 270 : 90;
                Arena.Drive(this, Direction, 50);
            }
        }
    }
}