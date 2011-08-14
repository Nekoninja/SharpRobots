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

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            _gameEventTimer = new Timer {Interval = 200, Enabled = true};
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
            _arenaWidth = splitContainer.Panel1.Width;
            _arenaHeight = splitContainer.Panel1.Height;

            // Determine proper scaling ratio from the robots world coordinates to the screen coordinates
            _scaleX = (double)_arenaWidth / Arena.ArenaWidth;
            _scaleY = (double)_arenaHeight / Arena.ArenaHeight;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void New()
        {
            // Resets all

            // Stop timer
            _gameEventTimer.Stop();

            // Clear battle field
            // Dump scripts

        }

        /// <summary>
        /// 
        /// </summary>
        private void Open()
        {
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

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            // Begin battle
            if (_scripts.Length == 0)
            {
                MessageBox.Show(@"You must first open up some robot scripts", @"Error");
                return;
            }

            if (_battleEngine.Load(_scripts))
            {
                _gameEventTimer.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Exit()
        {
            Application.Exit();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Options()
        {
            // Do we have any options?
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Help()
        {
            // Show the html page
        }

        /// <summary>
        /// 
        /// </summary>
        private static void About()
        {
            // Show about box
        }

        private void NextGameFrame(Object myObject, EventArgs myEventArgs)
        {
            _gameEventTimer.Stop();



            _gameEventTimer.Start();
        }
    }
}
