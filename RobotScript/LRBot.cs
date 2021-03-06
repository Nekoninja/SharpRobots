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

namespace RobotScript
{
    /// <summary>
    /// LRBot (Left Right Robot)
    /// LRBot moves from the left to the right then right to left
    /// changing direction before hitting the wall
    /// </summary>
    public class LRBot : Robot
    {
        #region Fields

        private int _nextDegree;
        private int _resolution;
        private int Direction { get; set; }
        private const int MaxSpeed = 25;

        #endregion

        #region Method: Init

        /// <summary>
        /// Init
        /// First of the two required methods we must implement
        /// The BotEngine will call this method one time after loading and compiling our robot
        /// </summary>
        public void Init()
        {
            Name = "LRBot";
            _resolution = 5;
            Direction = 90;
        }

        #endregion

        #region Method: Execute

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
            int range;

            // If speed is zero, start moving
            if (Arena.Speed(this) == 0)
            {
                Arena.Drive(this, Direction, MaxSpeed);
                Trace("Bot sets speed to 100");
            }

            // If about to hit a wall, change direction 180 degrees
            if (Arena.LocationX(this) < 50 || Arena.LocationX(this) > Arena.ArenaWidth - 50)
            {
                Direction = Direction == 90 ? 270 : 90;
                Arena.Drive(this, Direction, MaxSpeed);
                Trace(String.Format("Changing direction to {0}", Direction));
            }

            if ((range = Arena.Scan(this, _nextDegree, _resolution)) > 0)
            {
                if (Arena.Cannon(this, _nextDegree, range))
                {
                    Trace(String.Format("Firing Cannon at {0} degrees with a range of {1} Scanner resolution {2}", _nextDegree, range, _resolution));
                }
            }

            _nextDegree += 10;

            if (_nextDegree > 359) _nextDegree = 0;
            //Trace(String.Format("Next Degree: {0}", _nextDegree));
        }

        #endregion
    }
}