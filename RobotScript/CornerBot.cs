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
    public class CornerBot : Robot
    {
        private PointI _topLeft = new PointI { X = 250, Y = 250 };
        private PointI _topRight = new PointI { X = 750, Y = 250 };
        private PointI _bottomLeft = new PointI { X = 250, Y = 750 };
        private PointI _bottomRight = new PointI { X = 750, Y = 750 };
        private int _currentCorner;
        private bool _moving;

        public void Init()
        {
            Name = "CornerBot";
            _currentCorner = 3;
            _moving = false;

            Trace(Name + " is Alive!");
        }

        public void Execute()
        {
            int degrees = 0;

            // If we are not moving, then we need to go to a corner
            if (!_moving)
            {
                switch (_currentCorner)
                {
                    case 0: degrees = Arena.PlotCourse(Arena.LocationX(this), Arena.LocationY(this), _topLeft.X, _topLeft.Y); break;
                    case 1: degrees = Arena.PlotCourse(Arena.LocationX(this), Arena.LocationY(this), _topRight.X, _topRight.Y); break;
                    case 2: degrees = Arena.PlotCourse(Arena.LocationX(this), Arena.LocationY(this), _bottomLeft.X, _bottomLeft.Y); break;
                    case 3: degrees = Arena.PlotCourse(Arena.LocationX(this), Arena.LocationY(this), _bottomRight.X, _bottomRight.Y); break;
                }

                // Begin driving to the corner
                Arena.Drive(this, degrees, 100);
                _moving = true;
                _currentCorner++;
                if (_currentCorner > 3) _currentCorner = 0;
                Trace(String.Format("Changing direction to {0}", degrees));
            }

            // See if we have arrived at the corner
            switch (_currentCorner)
            {
                case 0: if (AtCorner(Arena.LocationX(this), Arena.LocationY(this), _topLeft)) _moving = false; break;
                case 1: if (AtCorner(Arena.LocationX(this), Arena.LocationY(this), _topRight)) _moving = false; break;
                case 2: if (AtCorner(Arena.LocationX(this), Arena.LocationY(this), _bottomLeft)) _moving = false; break;
                case 3: if (AtCorner(Arena.LocationX(this), Arena.LocationY(this), _bottomRight)) _moving = false; break;
            }
        }

        private static bool AtCorner(int x, int y, PointI point)
        {
            // Top left and bottom right of a 5 meter rectangle around the target point
            int rectX1 = point.X - 5;
            int rectY1 = point.Y - 5;
            int rectX2 = point.X + 5;
            int rectY2 = point.Y + 5;

            // Check to be within 5 meters of our target location
            return x >= rectX1 && x <= rectX2 && y >= rectY1 && y <= rectY2;
        }
    }
}