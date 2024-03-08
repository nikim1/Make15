using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Project_Make15
{
    /// <summary>
    /// Implements the Menu of the game.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        PVPWindow pvpWin;
        private bool isPlayingPVP = false; //indicator for PVP or PVE game is being chosen after name pick
        string name = string.Empty; //Player name 
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region EventHandlingMethods

        /// <summary>
        /// Indicates that PVP option is chosen. Proceed to name-picking window
        /// </summary>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            isPlayingPVP = true;
            NamePickerWindow namePicker = new NamePickerWindow(this);
            namePicker.Show();
            namePicker.BtnPickClick += OnNameSet;

        }
        /// <summary>
        /// Handles correct name picking from NamePickerWIndow. Creates a game based on the chosen PVP or PVE option.
        /// </summary>
        public void OnNameSet(object sender, NameRoutedEventArgs e)
        {//set name
            name = e.Name;
          
            if (isPlayingPVP)
            {
                //create pvp game
                try
                {
                    pvpWin = new PVPWindow(this, name);
                }
                catch (Exception)
                {
                    MessageBox.Show("Server connection could not be made. Make sure server is running.");
                    this.Show();
                    return;
                }
                this.Hide();
                pvpWin.Show();
                pvpWin.Owner = this;
            }
            else //pve
            {
                PVEWindow pveWin = new PVEWindow(this, name);
                this.Hide();

                pveWin.Show();
                pveWin.Owner = this;
            }
        }

        /// <summary>
        /// Creates a Help window
        /// </summary>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWind = new HelpWindow();
            helpWind.Show();
        }
        /// <summary>
        /// Indicates that PVE option is chosen. Proceed to name-picking window
        /// </summary>
        private void btnPVE_Click(object sender, RoutedEventArgs e)
        {
            isPlayingPVP = false;
            this.Hide();
            NamePickerWindow namePicker = new NamePickerWindow(this);
            namePicker.Show();
            namePicker.BtnPickClick += OnNameSet;
        }
        /// <summary>
        /// Shutdown the program.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        } 
        #endregion
    }
}
