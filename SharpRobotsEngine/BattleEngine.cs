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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharpRobotsEngine
{
    #region Class: PointF

    /// <summary>
    /// 2D Location
    /// </summary>
    public struct PointF
    {
        public float X;
        public float Y;
    }

    #endregion

    #region Class: BotAssembly

    /// <summary>
    /// Access to the loaded .NET bot assembly DLL
    /// </summary>
    public class BotAssembly
    {
        public string ClassName { get; set; }
        public Assembly Assembly { get; set; }
        public Object AssemblyInstance { get; set; }

        public int Id { get; set; }
        public PointF Location;
        public PointF LastLocation;
        public int Direction { get; set; }
        public int Speed { get; set; }
        public int LastSpeed { get; set; }
        public int Damage { get; set; }
        public int ScanDirection { get; set; }
        public int ScanResolution { get; set; }
        public int MissilesInFlight { get; set; } // Max of 2
    }

    #endregion

    #region Class: Missile

    /// <summary>
    /// Missile weapon
    /// </summary>
    public class Missile
    {
        public int Id { get; set;  }
        public PointF Location;
        public PointF LastLocation { get; set; }
        public int Direction { get; set; }
        public int Speed { get; set; }
        public int Range { get; set; }
        public bool Dead { get; set; }
    }

    #endregion

    #region Class: BattleEngine

    /// <summary>
    /// The BattleEngine runs execution of the bots, runs fired missiles and checks and assigns collision damage
    /// </summary>
    public class BattleEngine
    {
        #region Fields

        private readonly Stopwatch _timer = new Stopwatch();
        private double _lastTime;
        private bool _deadMissiles;
        private const string Language = "CSharp";
        private const string Namespace = "RobotScript";
        private const string InitMethod = "Init";
        private const string ExecuteMethod = "Execute";
        // TODO Currently width and height at 1000 (0-999) as in original CROBOTS, but does not have to be
        private const int ArenaWidth = 999;
        private const int ArenaHeight = 999;
        private const int MaxRobots = 4;
        private const int MaxMissles = 2;

        #endregion

        #region Properties

        public List<BotAssembly> Bots { get; set; }
        public List<Missile> Missiles { get; set; }
        public CompilerErrorCollection Errors { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The constructor initializes the Battle Engine
        /// </summary>
        public BattleEngine()
        {
            Bots = new List<BotAssembly>();
            Missiles = new List<Missile>();
            Arena.ArenaWidth = ArenaWidth + 1;
            Arena.ArenaHeight = ArenaHeight + 1;
            Arena.EngineInstance = this;

            // Initialize and start the timer
            _lastTime = 0.0;
            _timer.Start();
        }

        #endregion

        #region Method: Load

        /// <summary>
        /// Load given robots, compile, instantiate and initialize 
        /// Sets loaded robots Id's and calls each robots Initialize method
        /// </summary>
        /// <param name="botsSource"></param>
        /// <returns></returns>
        public bool Load(string[] botsSource)
        {
            string currentSourceFile = String.Empty;
            Bots.Clear();
            if (null != Errors) Errors.Clear();

            try
            {
                int botId = 1;
                foreach (var sourceFile in botsSource)
                {
                    // Create a unique assembly name so we can pit the same bot class against itself
                    string assemblyName = Path.GetFileNameWithoutExtension(sourceFile) + botId;
                    currentSourceFile = sourceFile;
                    BotAssembly bot = Compile(sourceFile, assemblyName);

                    if (null == bot || null == bot.AssemblyInstance) return false;
                    Ininialize(ref bot, ++botId);
                    Bots.Add(bot);
                }
            }
            catch (Exception ex)
            {
                if (null != Errors) Errors.Add(new CompilerError(currentSourceFile, 0, 0, "0", ex.ToString()));
                return false;
            }

            return true;
        }

        #endregion

        #region Method: Execute

        /// <summary>
        /// The Execute method is called repeatedly and runs the robots
        /// on the battle field calling each robots Execute method, updating
        /// in flight missiles, checking for collisions and updating damage.
        /// </summary>
        public bool Execute()
        {
            int totalDamage = 0;

            // Work out how long since we were last here in seconds 
            double gameTime = _timer.ElapsedMilliseconds / 1000.0;
            double elapsedTime = gameTime - _lastTime;
            _lastTime = gameTime;

            // Remove any missiles that died last game frame
            if (_deadMissiles)
                RemoveDeadMissles();

            // Update any missiles
            foreach (var missile in Missiles)
            {
                missile.LastLocation = missile.Location;

                // Update robot position based on the velocity
                missile.Location.X += (float)Math.Sin(missile.Direction * Math.PI / 180) * missile.Speed * (float)elapsedTime;
                missile.Location.Y += (float)Math.Cos(missile.Direction * Math.PI / 180) * missile.Speed * (float)elapsedTime;
                missile.Range -= Arena.Distance((int)missile.LastLocation.X, (int)missile.LastLocation.Y, (int)missile.Location.X, (int)missile.Location.Y);

                // Fix the missiles position to not leave the Arena
                if (missile.Location.X < 0) missile.Location.X = 0;
                if (missile.Location.X > ArenaWidth - 1) missile.Location.X = ArenaWidth - 1;
                if (missile.Location.Y < 0) missile.Location.Y = 0;
                if (missile.Location.Y > ArenaHeight - 1) missile.Location.Y = ArenaHeight - 1;

                // Missile has reached it's defined range.
                if (missile.Range <= 0)
                {
                    // Explode the missile
                    // TODO Determine any damage to bots in the area
                    // Iterate each robot. Get range to this missile. If in damage range, apply the damage.

                    // Update the bot that fired the missile
                    Missile missile1 = missile;
                    Bots.Find(botAssembly => botAssembly.Id == missile1.Id).MissilesInFlight--;

                    // Mark missile as dead so we can clean it up
                    missile.Dead = true;
                    _deadMissiles = true;
                }
            }

            // Update bots
            foreach (var bot in Bots)
            {
                totalDamage += bot.Damage;

                // 100% damaged bots do nothing
                if (bot.Damage != 100)
                {
                    // Check if robot is changing there speed
                    // TODO Need to ramp up/down to the robots set speed
                    //if (bot.Speed != bot.LastSpeed)
                    //{
                    //    if (bot.Direction >= 0 && bot.Direction <= 180)
                    //    {
                    //        bot.Velocity.X += 40;
                    //        bot.Velocity.Y += 40;                        
                    //    }
                    //    else
                    //    {
                    //        bot.Velocity.X -= 40;
                    //        bot.Velocity.Y -= 40;
                    //    }

                    //    bot.LastSpeed = bot.Speed;
                    //}

                    bot.LastLocation = bot.Location;

                    // Update robot position based on the velocity
                    bot.Location.X += (float) Math.Sin(bot.Direction*Math.PI/180)*bot.Speed*(float) elapsedTime;
                    bot.Location.Y += (float) Math.Cos(bot.Direction*Math.PI/180)*bot.Speed*(float) elapsedTime;

                    // Fix the robots position to not leave the Arena
                    if (bot.Location.X < 0) bot.Location.X = 0;
                    if (bot.Location.X > ArenaWidth - 1) bot.Location.X = ArenaWidth - 1;
                    if (bot.Location.Y < 0) bot.Location.Y = 0;
                    if (bot.Location.Y > ArenaHeight - 1) bot.Location.Y = ArenaHeight - 1;

                    // TODO Add collision detection and damage

                    // Have robots perform an execution step
                    bot.Assembly.GetType(Namespace + "." + bot.ClassName).GetMethod(ExecuteMethod).Invoke(bot.AssemblyInstance, null);
                }
            }

            return totalDamage == 300 ? false : true;
        }

        #endregion

        #region Method: Scan

        /// <summary>
        /// Called from the Arena, when a bot calls the Arena.Scan() method
        /// 
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
        public int Scan(Robot robot, int degree, int resolution)
        {
            // TODO Fix to wrap degrees degree < 0 degree > 360 modulo 360
            if (degree < 0) degree = 0;
            if (degree > 359) degree = 359;
            if (resolution < 0) resolution = 0;
            if (resolution > 10) resolution = 10;

            // Get the BotAssembly of the given robot from our list of robots
            BotAssembly botAssembly = Bots.Find(ba => ba.Id == robot.Id);

            // Update these for display
            botAssembly.ScanDirection = degree;
            botAssembly.ScanResolution = resolution;

            // Iterate all bots (Max of 4 bots in the game)
            foreach (var bot in Bots)
            {
                // As long as it is not the given robot
                if (bot != botAssembly)
                {
                    // Plot a course from this bot to the given methods bot
                    int course = Arena.PlotCourse((int)bot.Location.X, 
                                                  (int)bot.Location.Y,
                                                  (int)botAssembly.Location.X,
                                                  (int)botAssembly.Location.Y);

                    // Given the Scan resolution ( +/- 10 degrees maximum ) is there a bot out there?
                    // TODO Fix up the degree start and end to be within 0-360
                    int degStart = degree - resolution;
                    int degEnd = degree + resolution;
                    for (int scanResolution = degStart; scanResolution <= degEnd; ++scanResolution)
                    {
                        if (course == scanResolution)
                        {
                            // Return the range to the discovered target
                            return Math.Abs(Arena.Distance((int)bot.Location.X,
                                                           (int)bot.Location.Y,
                                                           (int)botAssembly.Location.X,
                                                           (int)botAssembly.Location.Y));
                        }
                    }
                }
            }

            // No robot found
            return 0;
        }

        #endregion

        #region Method: FireCannon

        /// <summary>
        /// Called from the Arena, when a bot calls the Arena.Cannon() method
        /// 
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
        public bool FireCannon(Robot robot, int degree, int range)
        {
            // TODO Fix to wrap degrees degree < 0 degree > 360 modulo 360
            if (degree < 0) degree = 0;
            if (degree > 359) degree = 359;
            if (range < 0) range = 0;
            if (range > 700) range = 700;

            // TODO Determine where to start the missile in relation to the bot that fired it
            if (Bots.Find(botAssembly => botAssembly.Id == robot.Id).MissilesInFlight <= MaxMissles)
            {
                BotAssembly bot = Bots.Find(botAssembly => botAssembly.Id == robot.Id);

                bot.MissilesInFlight++;
                Missiles.Add(new Missile
                                      {
                                          Id = robot.Id,
                                          Speed = 100,
                                          Location = new PointF {X = bot.Location.X, Y = bot.Location.Y},
                                          Direction = degree,
                                          Range = range
                                      });

                return true;
            }

            return false;
        }

        #endregion

        #region Method: RemoveDeadMissles

        /// <summary>
        /// Remove any dead missiles from our list
        /// </summary>
        private void RemoveDeadMissles()
        {
            List<Missile> missiles = Missiles.Where(missile => !missile.Dead).ToList();
            Missiles = missiles;
        }

        #endregion

        #region Method: Compile

        /// <summary>
        /// Compiles the given source file and returns a <see cref="BotAssembly"/>
        /// containing a created instance of the assembly
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private BotAssembly Compile(string sourceFile, string assemblyName)
        {
            string source;
            string fileNameOnly = Path.GetFileNameWithoutExtension(sourceFile);
            string namespaceDotClass = Namespace + "." + fileNameOnly;

            using (StreamReader sr = new StreamReader(sourceFile))
                source = sr.ReadToEnd();

            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider(Language);
            CompilerParameters parameters = new CompilerParameters
                                                {
                                                    GenerateExecutable = false,
                                                    IncludeDebugInformation = false,
                                                    GenerateInMemory = false,
                                                    OutputAssembly = assemblyName + ".dll",
                                                    MainClass = namespaceDotClass
                                                };

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("SharpRobotsEngine.dll");
            parameters.CompilerOptions = "/optimize";

            // TODO Possible future features
            // Set the level at which the compiler 
            // should start displaying warnings.
            //parameters.WarningLevel = 3;
            // Set whether to treat all warnings as errors.
            //parameters.TreatWarningsAsErrors = false;
            //
            // Check for and disallow loops that take over execution, such as while (true) and recursion

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, source);
            PeekMsil(results, fileNameOnly);

            if (results.Errors.Count > 0)
                    Errors = results.Errors;
            else
            {
                try
                {
                    return new BotAssembly
                                {
                                    ClassName = fileNameOnly,
                                    Assembly = results.CompiledAssembly,
                                    AssemblyInstance = results.CompiledAssembly.CreateInstance(namespaceDotClass, true)
                                };
                }
                catch (Exception ex)
                {
                    string err = ex.ToString();
                    return null;
                }
            }

            return null;
        }

        #endregion

        #region Method: Ininialize

        /// <summary>
        /// Initialize the given robot with the given id, assign a random
        /// starting location and call the robots InitMethod
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="id"></param>
        private static void Ininialize(ref BotAssembly bot, int id)
        {
            bot.Id = id;
            bot.Location.X = Arena.Rand(ArenaWidth);
            bot.Location.Y = Arena.Rand(ArenaHeight);
            bot.LastLocation.X = bot.Location.X;
            bot.LastLocation.Y = bot.Location.Y;

            // Zero by default : bot.Speed = 0; bot.LastSpeed = 0; bot.Damage = 0; bot.Direction = 0; bot.ScanDirection = 0; bot.MissilesInFlight = 0;
            bot.Assembly.GetType(Namespace + "." + bot.ClassName).GetProperty("Id").SetValue(bot.AssemblyInstance, id, null);
            bot.Assembly.GetType(Namespace + "." + bot.ClassName).GetMethod(InitMethod).Invoke(bot.AssemblyInstance, null);
        }

        #endregion

        #region Method: PeekMsil

        /// <summary>
        /// This method is being used for debug and research / development
        /// </summary>
        /// <param name="compilerResults"></param>
        /// <param name="className"></param>
        private static void PeekMsil(CompilerResults compilerResults, string className)
        {
            MethodInfo mi = compilerResults.CompiledAssembly.GetType(Namespace + "." + className).GetMethod(ExecuteMethod);
            MethodBody mb = mi.GetMethodBody();
            Trace.WriteLine(String.Format("\r\nMethod: {0}", mi));

            if (null != mb)
            {
                // Display the general information included in the MethodBody object.
                Trace.WriteLine(String.Format("    Local variables are initialized: {0}", mb.InitLocals));
                Trace.WriteLine(String.Format("    Maximum number of items on the operand stack: {0}", mb.MaxStackSize));

                // Display information about the local variables in the method body.
                Console.WriteLine();
                foreach (LocalVariableInfo lvi in mb.LocalVariables)
                    Trace.WriteLine(String.Format("Local variable: {0}", lvi));

                byte[] msil = mb.GetILAsByteArray();
                Trace.WriteLine("IL Code");
                foreach (var b in msil)
                {
                    Trace.Write(b);
                    Trace.Write(" ");
                }
            }
        }

        #endregion
    }

    #endregion
}