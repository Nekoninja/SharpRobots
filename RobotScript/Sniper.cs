// Always include the below
using System;
using SharpRobotsEngine;

// Always use this namespace
namespace RobotScript
{
    /// <summary>
    /// Our robot class name, needs to be unique from all other robots that will compete
    /// Always derive our class from the Robot base class
	///
	/// Strategy: since a scan of the entire battlefield can be done in 90
	/// degrees from a corner, sniper can scan the field quickly.
    /// </summary>
    public class Sniper : Robot
    {
        #region Fields

        private int _corner;                    // Current corner 0, 1, 2, or 3
        private int _corner1X, _corner1Y;       // Corner 1 x and y
        private int _corner2X, _corner2Y;       //   "    2 "  "  "
        private int _corner3X, _corner3Y;       //   "    3 "  "  "
        private int _corner4X, _corner4Y;       //   "    4 "  "  "
        private int _startScanPosition1;        // Starting scan position for corner 1
        private int _startScanPosition2;        // Starting scan position for corner 2
        private int _startScanPosition3;        // Starting scan position for corner 3
        private int _startScanPosition4;        // Starting scan position for corner 4
        private int _scanStart;                 // Current scan start
        private int _damage;                    // Last damage check
        private int _closestTarget;             // Check for targets in range
        private int _range;                     // Range to target
        private int _scanDirection;             // Scan direction

        #region Move Logic for NewCorner

        private bool _moving;                   // Moving or not
        private bool _speed20;                  // True when we are moving at the slow down speed
        private int _angle;                     // computed angle to the wall we are heading to
        private int _movingX;                   // X direction we are moving
        private int _movingY;                   // Y direction we are moving

        #endregion

        #endregion

        #region Method: Init

        /// <summary>
        /// Init
        /// First of the two required methods we must implement
        /// The BotEngine will call this method one time after loading and compiling our robot
        /// </summary>
        public void Init()
        {
            // Initialize the corner info x and y location of a corner, and starting scan degree
            _corner1X = 10;
            _corner1Y = 10;
            _corner2X = 990;
            _corner2Y = 10;
            _corner3X = 10;
            _corner3Y = 990;
            _corner4X = 990;
            _corner4Y = 990;
            _startScanPosition1 = 0;
            _startScanPosition2 = 270;
            _startScanPosition3 = 180;
            _startScanPosition4 = 90;
            _closestTarget = 9999;

            // Start at a random corner
            NewCorner();

            // Get current damage
            _damage = Arena.Damage(this);

            // Starting scan direction
            _scanDirection = _scanStart;
        }

        #endregion

        #region Method: Execute

        /// <summary>
        /// Execute
        /// The second of the two required methods to implement
        /// The BotEngine will call this method once each game cycle allowing us
        /// to execute our robot logic
        /// </summary>
        public void Execute()
        {
            // If no target
            if (_closestTarget == 9999)
            {
                _range = Arena.Scan(this, _scanDirection, 1);         // Look at a direction
            }

            // If target
            if (_range <= 700 && _range > 0)
            {
                // Set closestTarget flag
                _closestTarget = _range;

                // Fire!
                Arena.Cannon(this, _scanDirection, _range);

                // Check target again
                _range = Arena.Scan(this, _scanDirection, 1);
            }
            else
            {
                _scanDirection += 2;
                _closestTarget = 9999;
            }

            // Check for damage incurred and if so, move
            if (_damage != Arena.Damage(this))
            {
                NewCorner();
                _damage = Arena.Damage(this);
                _scanDirection = _scanStart;
            }

            // Check for any targets in range
            if (!_moving && _closestTarget == 9999)
            {
                // Nothing, move to new corner
                NewCorner();
                _scanDirection = _scanStart;
            }

            _damage = Arena.Damage(this);

            //while (_scanDirection < _scanStart + 90)
            //{
            //    // Scan through 90 degree range
            //    _range = Arena.Scan(this, _scanDirection, 1);         // Look at a direction

            //    if (_range <= 700 && _range > 0)
            //    {
            //        // Keep firing while in range
            //        while (_range > 0)
            //        {
            //            // Set closestTarget flag
            //            _closestTarget = _range;

            //            // Fire!
            //            Arena.Cannon(this, _scanDirection, _range);

            //            // Check target again
            //            _range = Arena.Scan(this, _scanDirection, 1);

            //            // Sustained several hits, go to new corner
            //            if (_damage + 15 > Arena.Damage(this))
            //                _range = 0;
            //        }

            //        // Back up scan, in case
            //        _scanDirection -= 10;
            //    }

            //    // Increment scan
            //    _scanDirection += 2;

            //    // Check for damage incurred and if so, move
            //    if (_damage != Arena.Damage(this))
            //    {
            //        NewCorner();
            //        _damage = Arena.Damage(this);
            //        _scanDirection = _scanStart;
            //    }
            //}

            //// Check for any targets in range
            //if (_closestTarget == 9999)
            //{
            //    // Nothing, move to new corner
            //    NewCorner();
            //    _damage = Arena.Damage(this);
            //    _scanDirection = _scanStart;
            //}
            //// Targets in range, resume
            //else
            //    _scanDirection = _scanStart;

            //_closestTarget = 9999;
        }

        #endregion

        #region Method: NewCorner

        /// <summary>
        /// Robot drives to a new corner
        /// </summary>
        private void NewCorner()
        {
            if (!_moving)
            {
                int newCorner = Arena.Rand(4);

                // But make it different than the current corner
                if (newCorner == _corner)
                    _corner = (newCorner + 1) % 4;
                else
                    _corner = newCorner;

                // Set new x, y and scan start
                switch (_corner)
                {
                    case 0:
                        _movingX = _corner1X;
                        _movingY = _corner1Y;
                        _scanStart = _startScanPosition1;
                        break;
                    case 1:
                        _movingX = _corner2X;
                        _movingY = _corner2Y;
                        _scanStart = _startScanPosition2;
                        break;
                    case 2:
                        _movingX = _corner3X;
                        _movingY = _corner3Y;
                        _scanStart = _startScanPosition3;
                        break;
                    case 3:
                        _movingX = _corner4X;
                        _movingY = _corner4Y;
                        _scanStart = _startScanPosition4;
                        break;
                }

                // Find the heading we need to get to the desired corner
                _angle = Arena.PlotCourse(_movingX, _movingY, Arena.LocationX(this), Arena.LocationY(this));

                // Start drive train, full speed
                Arena.Drive(this, _angle, 100);

                _moving = true;
            }

            if (_moving)
            {
                // Keep traveling until we are within 100 meters
                // speed is checked in case we run into wall, other robot
                // not terribly great, since were are doing nothing while moving
                if (Arena.Distance(Arena.LocationX(this), Arena.LocationY(this), _movingX, _movingY) <= 100)
                {
                    // Cut speed, if we have not already, and creep the rest of the way
                    if (!_speed20)
                    {
                        Arena.Drive(this, _angle, 20);
                        _speed20 = true;
                    }

                    // When distance is 10 or less, stop
                    if (Arena.Distance(Arena.LocationX(this), Arena.LocationY(this), _movingX, _movingY) <= 10)
                    {
                        // Stop drive, should coast in the rest of the way
                        Arena.Drive(this, _angle, 0);
                        _moving = false;
                    }
                }
            }

            // Keep traveling until we are within 100 meters
            // speed is checked in case we run into wall, other robot
            // not terribly great, since were are doing nothing while moving
            //while (Distance(Arena.LocationX(this), Arena.LocationY(this), _movingX, _movingY) > 100 && Arena.Speed(this) > 0)
            //{}

            // Cut speed, and creep the rest of the way
            //Arena.Drive(this, _angle, 20);
            //while (Distance(Arena.LocationX(this), Arena.LocationY(this), _movingX, _movingY) > 10 && Arena.Speed(this) > 0)
            //{}

            // Stop drive, should coast in the rest of the way
            //Arena.Drive(this, _angle, 0);
        }

        #endregion
    }
}
