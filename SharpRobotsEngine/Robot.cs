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

using System.Diagnostics;
using System.Text;

namespace SharpRobotsEngine
{
    /// <summary>
    /// A players robot class must derive from this base class
    /// See the various robot code examples
    /// </summary>
    public class Robot
    {
        #region Fields

        public TextWriterTraceListener Log { get; private set; }
        public int Id { get; set; }
        public string Name { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public Robot()
        {
            Name = "Unknown";
        }

        #endregion

        #region Method: Trace

        /// <summary>
        /// The Trace method wraps our base class trace listener and does the necessary flushing to the
        /// log file, else we get no trace information
        /// </summary>
        /// <param name="message"></param>
        protected void Trace(string message)
        {
            if (Log == null)
            {
                Log = new TextWriterTraceListener("botLog" + Id + ".txt");
                System.Diagnostics.Trace.Listeners.Add(Log);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Robot ");
            sb.Append(Id);
            sb.Append(" : ");
            sb.Append(Name);
            sb.Append(" : ");
            sb.Append(message);

            Log.WriteLine(sb.ToString());
            Log.Flush();
        }

        #endregion
    }
}
