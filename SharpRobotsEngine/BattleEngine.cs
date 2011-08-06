using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Compiled bot access
    /// </summary>
    public class BotAssembly
    {
        public string ClassName { get; set; }
        public Assembly Assembly { get; set; }
        public Object AssemblyInstance { get; set; }

        public int Id { get; set; }
        public PointF Location;
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
        public int Direction { get; set; }
        public int Speed { get; set; }
        public int Range { get; set; }
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
        public List<Missile> Missles { get; set; }
        public CompilerErrorCollection Errors { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public BattleEngine()
        {
            Bots = new List<BotAssembly>();
            Missles = new List<Missile>();
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
        /// Sets loaded robots Id and calls it's Initialize method
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
                    currentSourceFile = sourceFile;
                    BotAssembly bot = Compile(sourceFile);

                    if (null == bot || null == bot.AssemblyInstance) return false;
                    Bots.Add(bot);
                    Ininialize(ref bot, botId++);
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
        /// 
        /// </summary>
        public bool Execute()
        {
            int totalDamage = 0;

            // Work out how long since we were last here in seconds 
            double gameTime = _timer.ElapsedMilliseconds / 1000.0;
            double elapsedTime = gameTime - _lastTime;
            _lastTime = gameTime;

            // TODO Add collision detection

            // TODO Add missiles
            // Update any missiles

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

                    // Update robot position based on the velocity
                    bot.Location.X += (float) Math.Sin(bot.Direction*Math.PI/180)*bot.Speed*(float) elapsedTime;
                    bot.Location.Y += (float) Math.Cos(bot.Direction*Math.PI/180)*bot.Speed*(float) elapsedTime;

                    // Fix the robot position to not leave the Arena
                    if (bot.Location.X < 0) bot.Location.X = 0;
                    if (bot.Location.X > ArenaWidth - 1) bot.Location.X = ArenaWidth - 1;
                    if (bot.Location.Y < 0) bot.Location.Y = 0;
                    if (bot.Location.Y > ArenaHeight - 1) bot.Location.Y = ArenaHeight - 1;

                    // Determine collisions and damage

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
            if (degree < 0) degree = 0;
            if (degree > 359) degree = 359;
            if (resolution < 0) resolution = 0;
            if (resolution > 10) resolution = 10;
            Bots.Find(botAssembly => botAssembly.Id == robot.Id).ScanDirection = degree;
            Bots.Find(botAssembly => botAssembly.Id == robot.Id).ScanResolution = resolution;

            // TODO Given direction and resolution, determine if a bot is out there
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
            // TODO Finish the method
            if (degree < 0) degree = 0;
            if (degree > 359) degree = 359;
            if (range < 0) range = 0;
            if (range > 700) range = 700;

            // TODO Determine where to start the missile in relation to the bot that fired it
            if (Bots.Find(botAssembly => botAssembly.Id == robot.Id).MissilesInFlight <= MaxMissles)
            {
                Missles.Add(new Missile
                                      {
                                          Id = 0, // TODO Do we really need an id? Could use parent bot id so we know who hit whom
                                          Speed = 100,
                                          Location = new PointF {X = 0, Y = 0},
                                          Direction = degree,
                                          Range = range
                                      });
            }

            return false;
        }

        #endregion

        #region Method: Compile

        /// <summary>
        /// Compiles the given source file and returns a <see cref="BotAssembly"/>
        /// containing a created instance of the code
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        private BotAssembly Compile(string sourceFile)
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
                                                    OutputAssembly = fileNameOnly + ".dll",
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
        /// 
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="id"></param>
        private static void Ininialize(ref BotAssembly bot, int id)
        {
            bot.Id = id;
            bot.Location.X = Arena.Rand(ArenaWidth);
            bot.Location.Y = Arena.Rand(ArenaHeight);
            // Zero by default
            //bot.Speed = 0; bot.LastSpeed = 0; bot.Damage = 0; bot.Direction = 0; bot.ScanDirection = 0; bot.MissilesInFlight = 0;
            bot.Assembly.GetType(Namespace + "." + bot.ClassName).GetProperty("Id").SetValue(bot.AssemblyInstance, id, null);
            bot.Assembly.GetType(Namespace + "." + bot.ClassName).GetMethod(InitMethod).Invoke(bot.AssemblyInstance, null);
        }

        #endregion

        #region Method: PeekMsil

        /// <summary>
        /// Currently for debug
        /// </summary>
        /// <param name="compilerResults"></param>
        /// <param name="className"></param>
        private static void PeekMsil(CompilerResults compilerResults, string className)
        {
            MethodInfo mi = compilerResults.CompiledAssembly.GetType(Namespace + "." + className).GetMethod(ExecuteMethod);
            MethodBody mb = mi.GetMethodBody();
            Console.WriteLine("\r\nMethod: {0}", mi);

            if (null != mb)
            {
                // Display the general information included in the MethodBody object.
                Console.WriteLine("    Local variables are initialized: {0}", mb.InitLocals);
                Console.WriteLine("    Maximum number of items on the operand stack: {0}", mb.MaxStackSize);

                // Display information about the local variables in the method body.
                Console.WriteLine();
                foreach (LocalVariableInfo lvi in mb.LocalVariables)
                    Console.WriteLine("Local variable: {0}", lvi);

                byte[] msil = mb.GetILAsByteArray();
                Console.WriteLine("IL Code");
                foreach (var b in msil)
                {
                    Console.Write(b);
                    Console.Write(" ");
                }
            }
        }

        #endregion
    }

    #endregion
}