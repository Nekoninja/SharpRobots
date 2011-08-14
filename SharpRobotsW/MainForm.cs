using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpRobotsEngine;

namespace SharpRobotsW
{
    public partial class MainForm : Form
    {
        #region Fields

        private int _arenaWidth = 80;
        private int _arenaHeight = 40;
        private double _scaleX;
        private double _scaleY;
        private int _cycles;
        private string[] _scripts;
        private readonly BattleEngine _battleEngine;
        private readonly Timer _gameEventTimer;
        private bool _gameRunning;
        private Bitmap _robot0Bitmap;
        private Bitmap _missileBitmap;
        private int _x = 20;
        private int _y = 20;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            _gameEventTimer = new Timer {Interval = 35, Enabled = true};
            _gameEventTimer.Tick += NextGameFrame;
            _battleEngine = new BattleEngine();

            Init();
        }

        #endregion

        #region Menu Methods

        private void NewToolStripMenuItemClick(object sender, EventArgs e) { New(); }
        private void OpenToolStripMenuItemClick(object sender, EventArgs e) { Open(); }
        private void StartToolStripMenuItemClick(object sender, EventArgs e) { Start(); }
        private void ExitToolStripMenuItemClick(object sender, EventArgs e) { Exit(); }
        private void OptionsToolStripMenuItemClick(object sender, EventArgs e) { Options(); }
        private void ContentsToolStripMenuItemClick(object sender, EventArgs e) { Help(); }
        private void AboutToolStripMenuItemClick(object sender, EventArgs e) { About(); }

        #endregion

        #region Tool Strip Methods

        private void NewToolStripButtonClick(object sender, EventArgs e) { New(); }
        private void OpenToolStripButtonClick(object sender, EventArgs e) { Open(); }
        private void StartToolStripButtonClick(object sender, EventArgs e) { Start(); }
        private void HelpToolStripButtonClick(object sender, EventArgs e) { Help(); }

        #endregion

        #region Method: Init

        /// <summary>
        /// Setup Console and battle engine
        /// </summary>
        private void Init()
        {
            _gameEventTimer.Stop();
            _gameRunning = false;
            _arenaWidth = splitContainer.Panel1.Width;
            _arenaHeight = splitContainer.Panel1.Height;

            // Determine proper scaling ratio from the robots world coordinates to the screen coordinates
            _scaleX = (double)_arenaWidth / Arena.ArenaWidth;
            _scaleY = (double)_arenaHeight / Arena.ArenaHeight;

            _robot0Bitmap = new Bitmap("images\\tank0.png");
            _missileBitmap = new Bitmap("images\\missile.png");
        }

        #endregion

        #region Method: New

        /// <summary>
        /// 
        /// </summary>
        private void New()
        {
            // Resets all

            // Stop timer
            _gameEventTimer.Stop();
            _gameRunning = false;

            // Clear battle field
            // Dump scripts

        }

        #endregion

        #region Method: Open

        /// <summary>
        /// 
        /// </summary>
        private void Open()
        {
            if (_gameRunning) return;

            // Load all selected scripts
            OpenFileDialog ofd = new OpenFileDialog
                                     {
                                         Title = @"Load Scripts",
                                         Filter = @"C# files (*.cs)|*.cs|All files (*.*)|*.*",
                                         AutoUpgradeEnabled = true,
                                         RestoreDirectory = true,
                                         Multiselect = true
                                     };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _scripts = ofd.FileNames;
            }
        }

        #endregion

        #region Method: Start

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (_gameRunning) return;

            // Begin battle
            if (_scripts.Length == 0)
            {
                MessageBox.Show(@"You must first open up some robot scripts", @"Error");
                return;
            }

            if (_battleEngine.Load(_scripts))
            {
                _gameRunning = true;
                _gameEventTimer.Start();
            }
        }

        #endregion

        #region Method: Exit

        /// <summary>
        /// 
        /// </summary>
        private static void Exit()
        {
            Application.Exit();
        }

        #endregion

        #region Method: Opyions

        /// <summary>
        /// 
        /// </summary>
        private static void Options()
        {
            // Do we have any options?
        }

        #endregion

        #region Method: Help

        /// <summary>
        /// 
        /// </summary>
        private static void Help()
        {
            // Show the html page
        }

        #endregion

        #region Method: About

        /// <summary>
        /// 
        /// </summary>
        private void About()
        {
            if (_gameRunning) _gameEventTimer.Stop();

            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();

            if (_gameRunning) _gameEventTimer.Start();
        }

        #endregion

        #region Event: NextGameFrame

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="myEventArgs"></param>
        private void NextGameFrame(Object myObject, EventArgs myEventArgs)
        {
            _gameEventTimer.Stop();
            ++_x;
            ++_y;
            if (_x > _arenaWidth - 16) _x = 20;
            if (_y > _arenaHeight - 16) _y = 20;

            if (!_battleEngine.Execute())
            {
                // Display winner
                return;
            }

            _cycles++;
            splitContainer.Panel1.Invalidate();
            _gameEventTimer.Start();
        }

        #endregion

        private void SplitContainerPanel1Paint(object sender, PaintEventArgs e)
        {
            int botNum = 0;

            foreach (var missile in _battleEngine.Missiles)
            {
                int x = (int)(missile.Location.X * _scaleX);
                int y = (int)(missile.Location.Y * _scaleY);

                if (missile.Dead) continue;
                e.Graphics.DrawImage(_missileBitmap, x, y);
            }

            foreach (var bot in _battleEngine.Bots)
            {
                int x = (int)(bot.Location.X * _scaleX);
                int y = (int)(bot.Location.Y * _scaleY);

                // Update the robots position on screen
                e.Graphics.DrawImage(_robot0Bitmap, x, y);

                botNum++;
            }

        }
    }
}
