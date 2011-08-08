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

namespace SharpRobotsEngine
{
    /// <summary>
    /// The static Arena class is the API available to and used by the players
    /// robots to interact on the Battle Field
    /// </summary>
    public static class Arena
    {
        #region Fields

        private const int RandMin = 0;
        private const int RandMax = 32767;

        #endregion

        #region Properties

        public static BattleEngine EngineInstance { private get; set; }
        public static int ArenaWidth  { get; set; }
        public static int ArenaHeight { get; set; }

        #endregion

        #region Method: Scan

        /// <summary>
        /// The Scan() function invokes the robot's scanner, at a specified degree and resolution.
        /// Scan() returns 0 if no robots are within the scan range or a positive integer representing
        /// the  range to the closest robot.  Degree should be within the range 0-359, otherwise degree
        /// is forced into 0-359 by a modulo 360 operation, and made positive if necessary.
        /// Resolution controls the scanner's sensing resolution, up to +/- 10 degrees.
        /// 
        /// Examples:
        ///   range = Scan(robot, 45, 0);   // scan 45, with no variance
        ///   range = Scan(robot, 365, 10); // scans the range from 355 to 15
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="degree"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static int Scan(Robot robot, int degree, int resolution)
        {
            return EngineInstance.Scan(robot, degree, resolution);
        }

        #endregion

        #region Method: Cannon

        /// <summary>
        /// The Cannon() function fires a missile heading a specified range and direction.
        /// Cannon() returns 1 (true) if a missile was fired, or 0 (false) if the cannon is reloading.
        /// Degree is forced into the range 0-359 as in Scan() and Drive().
        /// Range can be 0-700, with greater ranges truncated to 700.
        /// 
        /// Examples:
        ///    degree = 45;                              // set a direction to test
        ///    if ((range=Scan(robot, degree, 2)) > 0)   // see if a target is there
        ///      Cannon(robot, degree, range);           // fire a missile
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="degree"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool Cannon(Robot robot, int degree, int range)
        {
            return EngineInstance.FireCannon(robot, degree, range);
        }

        #endregion

        #region Method: Drive

        /// <summary>
        /// The Drive() function activates the robot's drive mechanism, on a specified heading and speed.
        /// Degree is forced into the range 0-359 as in Scan().
        /// Speed is expressed as a percent, with 100 as maximum.
        /// A speed of 0 disengages the drive.
        /// Changes in direction can be negotiated at speeds of less than 50 percent.
        /// 
        /// Examples:
        ///    Drive(robot, 0, 100);  // head due east, at maximum speed
        ///    Drive(robot, 90, 0);   // stop motion
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="degree"></param>
        /// <param name="speed"></param>
        public static void Drive(Robot robot, int degree, int speed)
        {
            if (speed < 0) speed = 0;
            if (speed > 100) speed = 100;
            if (degree < 0) degree = 0;
            if (degree > 359) degree = 359;

            EngineInstance.Bots.Find(botAssembly => botAssembly.Id == robot.Id).Speed = speed;

            if (speed <= 50)
                EngineInstance.Bots.Find(botAssembly => botAssembly.Id == robot.Id).Direction = degree;
        }

        #endregion

        #region Method: Damage

        /// <summary>
        /// The Damage() function returns the current amount of damage incurred.
        /// Damage() takes no arguments, and returns the percent of damage, 0-99.
        /// (100 percent damage means the robot is completely disabled, thus no longer running!) 
        /// 
        /// Examples:
        ///   d = Damage(robot);        // save current state
        ///   ; ; ;                     // other instructions
        ///   if (d != Damage(robot))   // compare current state to prior state
        ///   {
        ///     Drive(robot, 90, 100);  // robot has been hit, start moving
        ///     d = Damage(robot);      // get current damage again
        ///   } 
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public static int Damage(Robot robot)
        {
            return EngineInstance.Bots.Find(botAssembly => botAssembly.Id == robot.Id).Damage;
        }

        #endregion

        #region Method: Speed

        /// <summary>
        /// The Speed() function returns the current speed of the robot.
        /// Speed() takes no arguments, and returns the percent of speed, 0-100.
        /// Note that Speed() may not always be the same as the last drive(), because of acceleration and de-acceleration.
        /// 
        /// Examples:
        ///   Drive(robot, 270, 100);   // start drive, due south
        ///   ; ; ;                     // other instructions
        ///   if (Speed(robot) == 0)    // check current speed
        ///   {
        ///     Drive(robot, 90, 20);   // ran into the south wall, or another robot
        ///   }
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public static int Speed(Robot robot)
        {
            return EngineInstance.Bots.Find(botAssembly => botAssembly.Id == robot.Id).Speed;
        }

        #endregion

        #region Method: LocationX

        /// <summary>
        /// The LocationX() function returns the robot's current x axis location in range 0-999.
        /// 
        /// Examples:
        ///    Drive(robot, 180, 50);           // start heading for west wall
        ///    while (LocationX(robot)) > 20)
        ///      ;                              // do nothing until we are close
        ///    Drive(robot, 180, 0);            // stop drive
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public static int LocationX(Robot robot)
        {
            return (int)EngineInstance.Bots.Find(botAssembly => botAssembly.Id == robot.Id).Location.X;
        }

        #endregion

        #region Method: LocationY

        /// <summary>
        /// The LocationY() function returns the robot's current y axis location in range 0-999.
        /// 
        /// Examples:
        ///    Drive(robot, 0, 50);             // start heading for east wall
        ///    while (LocationY(robot)) > 20)
        ///      ;                              // do nothing until we are close
        ///    Drive(robot, 180, 0);            // stop drive
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public static int LocationY(Robot robot)
        {
            return (int)EngineInstance.Bots.Find(botAssembly => botAssembly.Id == robot.Id).Location.Y;
        }

        #endregion

        #region Method: Distance

        /// <summary>
        /// Classical Pythagorean distance formula to return the distance between the two points
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static int Distance(int x1, int y1, int x2, int y2)
        {
            int x = x1 - x2;
            int y = y1 - y2;

            return Sqrt((x * x) + (y * y));
        }

        #endregion

        #region Method: PlotCourse

        /// <summary>
        /// Plot course function, return degree heading to reach destination x, y; uses Atan() trig function
        /// x2 and y2 is the current X,Y of your robot
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static int PlotCourse(int x1, int y1, int x2, int y2)
        {
            double thetaRadians = Math.Atan2(y2 - y1, x2 - x1);
            double thetaDegrees = (thetaRadians + Math.PI) * 360.0 / (2.0 * Math.PI);

            return (int)thetaDegrees;
        }

        #endregion

        #region Method: Rand

        /// <summary>
        /// The Rand() function returns a random number between 0 and limit, up to 32767.
        /// 
        /// Examples:
        ///   degree = Rand(360);               // pick a random starting point
        ///   range = Scan(robot, degree, 0);   // and scan
        /// </summary>
        /// <returns></returns>
        public static int Rand(int limit)
        {
            Random rnd = new Random((int) DateTime.Now.Ticks);

            if (limit > RandMax) limit = RandMax;

            return rnd.Next(RandMin, limit);
        }

        #endregion

        #region Method: Sqrt

        /// <summary>
        /// The Sqrt() returns the square root of a number.
        /// Number is made positive, if necessary.
        /// 
        /// Examples:
        ///    x = x1 - x2;     // compute the classical distance formula
        ///    y = y1 - y2;     // between two points (x1,y1) (x2,y2)
        ///    distance = Sqrt((x*x) - (y*y));
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int Sqrt(int number)
        {
            double r = Math.Sqrt(number);

            return (int) Math.Abs(r);
        }

        #endregion

        #region Method: Sin

        /// <summary>
        /// Sin() takes a degree argument, 0-359, and returns the trigometric value
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double Sin(int degree)
        {
            double angle = degree * Math.PI / 180;

            return Math.Sin(angle);
        }

        #endregion

        #region Method: Cos

        /// <summary>
        /// Cos() takes a degree argument, 0-359, and returns the trigometric value
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double Cos(int degree)
        {
            double angle = degree * Math.PI / 180;

            return Math.Cos(angle);
        }

        #endregion

        #region Method: Tan

        /// <summary>
        /// Tan() takes a degree argument, 0-359, and returns the trigometric value
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double Tan(int degree)
        {
            double angle = degree * Math.PI / 180;

            return Math.Tan(angle);
        }

        #endregion

        #region Method: Atan

        /// <summary>
        /// Atan() takes a ratio argument and returns a degree value, between -90 and +90.
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static int Atan(int ratio)
        {
            double angle = Math.Atan(ratio);
            angle *= 180 / Math.PI;

            return (int)angle;
        }

        #endregion

        #region Method: ReciprocalDegrees

        /// <summary>
        /// ReciprocalDegrees() returns the reciprocal degree of the given degree
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static int ReciprocalDegrees(int degrees)
        {
            if (degrees >= 180)
                return degrees - 180;

            return degrees + 180;
        }

        #endregion
    }
}