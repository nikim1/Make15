using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_Make15
{
    /// <summary>
    /// Window to display helpful information about the program and the game.
    /// </summary>
    public partial class HelpWindow : Window
    {

        #region Constructors
        //  private Window parent;
        public HelpWindow()
        {
            InitializeComponent();
            //fill the text
            WriteMenuSection();
            WritePVPSection();
            WritePVESection();

        } 
        #endregion

        #region TextWritingMethods

        private void WriteMenuSection()
        {
            txtBlckMenu.Text = "The goal of the game is to be the" +
                " first to have 3 numbers that summed together make 15! You can pick numbers from 1 to 9" +
                " and each number can be taken just once by some of the players." +
                " The Game menu consists of 3 buttons - Find Match, PVE and Help." +
                " Each button will lead you to a different page and you can easily go back to the Menu" +
                " by closing the current window that you have opened. ";
        }
        private void WritePVPSection()
        {
            txtBlckPVP.Text = "The Plaver-Versus-Player is online mode for players to play agaist each other" +
                " in real time and requires internet connection. When entered in it, you will see a window that will ask you to enter a name" +
                " for your next game that will be displayed as your nickname in the chat window. Note, that name" +
                " will not be saved for future games, so you can change it later. Once in the game, you will be announced in the chat" +
                " whenever another player is found. You can freely chat with him while playing.";
        }
        private void WritePVESection()
        {
            txtBlckPVE.Text = "The Player-Versus-Environment mode is good for begginers to learn how to play the game." +
                " You'll be playing against the System in an offline mode and you won't have to connect to the" +
                " game server for that job. You can freely experiment new tactics and strategies. You can also chat, but..." +
                " you'd better not expect an answer.";
        } 
        #endregion
        
    }
}
