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