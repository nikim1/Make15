using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using UsrCtrl_GameWindow;

namespace Project_Make15
{
    /// <summary>
    /// Window that uses single Thread to implement a game between the user and a simple AI.
    /// No need to connect to the server and much lighter than the PVP Window implementation.
    /// </summary>
    public partial class PVEWindow : Window
    {
        #region Fields
        //fields coming from User Control (for easier use)
        private TextBlock namesBlock;
        private RichTextBox displayTextBox;
        private ListBox lstboxUntaken;
        private ListBox lstboxOpponent;
        private ListBox lstboxYour;
        private TextBox txtBoxChatInput;
        private Image imgYourArrow;
        private Image imgEnemyArrow;
        private UserControlGameWindow UC;
        private string playerName;
        Window parent;
        //players data
        private bool isPlayerTurn = true;
        private BindingList<byte>[] playersPicks;
        private BindingList<byte> availablePicks;
        #endregion

        #region Constructors
        /// <summary>
        /// Sets the UC data and initialize the players.
        /// </summary>
        public PVEWindow(Window _parent, string _name)
        {
            InitializeComponent();
            //initialize
            playersPicks = new BindingList<byte>[2];
            playersPicks[0] = new BindingList<byte>();//player
            playersPicks[1] = new BindingList<byte>();//system
            availablePicks = new BindingList<byte> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            //binding
            parent = _parent;
            parent.Hide();
            playerName = _name;
            //initialize
            UC = PVPUserControl;
            displayTextBox = UC.Chat;
            namesBlock = UC.NamesBlock;
            lstboxUntaken = UC.ListBoxUntaken;
            lstboxOpponent = UC.ListBoxOpponent;
            lstboxYour = UC.ListBoxYour;
            txtBoxChatInput = UC.ChatInputTextBox;
            imgYourArrow = UC.YourArrowImg;
            imgEnemyArrow = UC.EnemyArrowImg;
            imgYourArrow.Visibility = Visibility.Visible;
            imgEnemyArrow.Visibility = Visibility.Hidden;
            //nullify
            displayTextBox.Document.Blocks.Clear();
            namesBlock.Text = string.Format("{0}\nVS System", playerName);
            //bind
            lstboxUntaken.ItemsSource = availablePicks;
            lstboxYour.ItemsSource = playersPicks[0];
            lstboxOpponent.ItemsSource = playersPicks[1];
            SendSytemMsg("Welcome!");
        }

        #endregion

        #region EventHandling
        /// <summary>
        /// Validate user pick. Add to player's choice List and check if he's the winner, otherwise process to AI turn.
        /// </summary>
        private void PVPUserControl_BtnPickClick(object sender, RoutedEventArgs e)
        {
            if (lstboxUntaken.SelectedItem != null && isPlayerTurn)
            {
                playersPicks[0].Add((byte)lstboxUntaken.SelectedItem);
                availablePicks.Remove((byte)lstboxUntaken.SelectedItem);
                isPlayerTurn = false;
                imgEnemyArrow.Visibility = Visibility.Visible;
                imgYourArrow.Visibility = Visibility.Hidden;

                if (!isWinner(0))//if there is no winner yet
                {
                    SystemTakeTurn();
                }
            }
        }
        /// <summary>
        /// Chat message using color indication for user name.
        /// </summary>
        private void PVPUserControl_ChatEnterClick(object sender, RoutedEventArgs e)
        {
            //name
            TextRange tr = new TextRange(displayTextBox.Document.ContentEnd, displayTextBox.Document.ContentEnd);
            tr.Text = string.Format($"\nYou :");
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
            //text
            tr = new TextRange(displayTextBox.Document.ContentEnd, displayTextBox.Document.ContentEnd);
            tr.Text = txtBoxChatInput.Text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
            //just sending it to yourself , no opponent
        }
        /// <summary>
        /// On program close, go back to the parent window.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            //base.OnClosed(e);
            parent.Show();
        }
        #endregion

        #region ClassMethods
        /// <summary>
        /// Chat message from the system with different color indication.
        /// </summary>
        private void SendSytemMsg(string msg)
        {
            TextRange tr = new TextRange(displayTextBox.Document.ContentEnd, displayTextBox.Document.ContentEnd);
            tr.Text = string.Format($"\nSystem:");
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
            //text
            tr = new TextRange(displayTextBox.Document.ContentEnd, displayTextBox.Document.ContentEnd);
            tr.Text = msg;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }

        /// <summary>
        /// Uses O(n^2) algorithm based on checking if 3 of player numbers summed make 15. 
        /// Optimization that fixes one of the 3 choices to the last number as it's assumed that any other 3 can't make 15 or the game
        /// would have ended earlier.
        /// </summary>
        /// <param name="playerIndex"> 0 for User and 1 for AI</param>
        /// <returns>If the player is a winner</returns>
        public bool isWinner(byte playerIndex)
        {//we will fix the last number as first as we assume that if new numbers have been added, 
         //the player was not a winner before that
            if (playersPicks[playerIndex].Count == 0) return false;
            //
            var first = playersPicks[playerIndex].Last();
            //
            for (var i = 0; i < playersPicks[playerIndex].Count - 2; i++)
            {//second number
                for (var j = i + 1; j < playersPicks[playerIndex].Count - 1; j++)
                {//third
                    if (first + playersPicks[playerIndex][i] + playersPicks[playerIndex][j] == 15)
                    {
                        if (playerIndex == 0)
                        {
                            SendSytemMsg("Congratulations! You won! Outstanding game.");
                        }
                        else
                        {
                            SendSytemMsg("Oh, it seems like I won this time. Don't worry, it will be our secret!");
                        }
                        return true;//winner!
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Adds a new number to the AI picks and removes it from available.Checks if AI is a winner, otherwise it's user turn.
        /// </summary>
        /// <param name="numIndex">Index in available numbers</param>
        private void SystemPlay(int numIndex)
        {
            playersPicks[1].Add(availablePicks[numIndex]);
            availablePicks.RemoveAt(numIndex);
            if (!isWinner(1))
            {
                isPlayerTurn = true;
                imgYourArrow.Visibility = Visibility.Visible;
                imgEnemyArrow.Visibility = Visibility.Hidden;
            }

        }
        /// <summary>
        /// Uses simple logic to decide it's next pick choice. Checks for Draw.
        /// </summary>
        private void SystemTakeTurn()
        {
            if (availablePicks.Count == 0)
            {
                SendSytemMsg("Draw! No more available numbers left. Great game!");
                return;
            }

            //check for winning move
            for (int firstInd = 0; firstInd < playersPicks[1].Count; firstInd++)
            {
                for (int secInd = firstInd + 1; secInd < playersPicks[1].Count; secInd++)
                {
                    int winningMove = 15 - playersPicks[1][firstInd] - playersPicks[1][secInd];
                    if (availablePicks.Contains((byte)winningMove))
                    {

                        SystemPlay(availablePicks.IndexOf((byte)winningMove));
                        return;
                    }
                }
            }
            //lets check if player has a winning move and take it
            for (int firstInd = 0; firstInd < playersPicks[0].Count; firstInd++)
            {
                for (int secInd = firstInd + 1; secInd < playersPicks[0].Count; secInd++)
                {
                    int winningMove = 15 - playersPicks[0][firstInd] - playersPicks[0][secInd];
                    if (availablePicks.Contains((byte)winningMove))
                    {
                        SystemPlay(availablePicks.IndexOf((byte)winningMove));
                        return;
                    }
                }
            }
            //lets pick a random number
            SystemPlay(new Random().Next(0, availablePicks.Count - 1));
            // }
        }
        
        #endregion
    }
}
