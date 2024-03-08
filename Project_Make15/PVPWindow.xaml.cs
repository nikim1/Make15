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
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UsrCtrl_GameWindow;
using Project_Make15.ServiceReference1;
using System.ServiceModel;

namespace Project_Make15
{
    /// <summary>
    /// Class that implements the PVP game. Uses Duplex service contract with web service.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public partial class PVPWindow : Window, ServiceReference1.IMake15ServiceCallback
    {

        #region Fields
        ServiceReference1.Make15ServiceClient client;
        private TextBlock namesBlock;
        private RichTextBox displayTextBox;
        private ListBox lstboxUntaken;
        private ListBox lstboxOpponent;
        private ListBox lstboxYour;
        private TextBox txtBoxChatInput;
        private Image imgYourArrow;
        private Image imgEnemyArrow;
        private UserControlGameWindow UC;
        private string playerID;
        private string playerName;
        Window parent;
        #endregion

        #region Constructors

        public PVPWindow(Window _parent, string _name)
        {
            InitializeComponent();
            //
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
            //nullify
            displayTextBox.Document.Blocks.Clear();
            //
            client = new Make15ServiceClient(new InstanceContext(this)); //service connection
            playerID = client.MakePlayer(playerName);
            RefreshLists();
        }
        #endregion

        #region EventHandling
        /// <summary>
        /// Removes player data from server and closes connection. Goes back to parent Window.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            //base.OnClosed(e);
            parent.Show();
            try
            {
                client.RemovePlayer(playerID);
                client.Close();
            }
            catch (Exception)
            {
                client.Abort();
            }
        }
        /// <summary>
        /// Indicate that player has picked a number from availables list.
        /// </summary>
        private void PVPUserControl_BtnPickClick(object sender, RoutedEventArgs e)
        {
            if (lstboxUntaken.SelectedItem != null)
            {
                client.MarkAsTaken(byte.Parse(lstboxUntaken.SelectedItem.ToString()), playerID);
                //check if winner
                RefreshLists();
            }
        }

        /// <summary>
        /// Chat connection. Display message and send to other player in the server.
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
            //send to opponent
            client.SendMsgToOpponent(playerID, playerName, txtBoxChatInput.Text);
        }

        #endregion

        #region ClassMethods
        /// <summary>
        /// Thread-safe refresh of lists showing the data.
        /// </summary>
        private void RefreshLists()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lstboxUntaken.ItemsSource = client.GetUntaken();
                lstboxYour.ItemsSource = client.GetTakenBy(playerID);
                lstboxOpponent.ItemsSource = client.GetTakenByOpponent(playerID);
            }));

        }
        /// <summary>
        /// Receives message from server. Color indication for users. Thread-safe.
        /// </summary>
        public void ReceiveMsg(string user, string msg)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextRange tr = new TextRange(displayTextBox.Document.ContentEnd, displayTextBox.Document.ContentEnd);
                tr.Text = string.Format($"\n{user}:");
                if (user == "System")
                {
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                }
                else
                {
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                }
                //text
                tr = new TextRange(displayTextBox.Document.ContentEnd, displayTextBox.Document.ContentEnd);
                tr.Text = msg;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                //displayTextBox.Text += string.Format("\n{0}: {1}", user, msg);

            }));
        }
        /// <summary>
        ///  Pulses - checks if player is active. Otherwise it will be removed from service data.
        /// </summary>
        void IMake15ServiceCallback.IsActive()
        {
            return;
        }
        /// <summary>
        /// Refreshes lists with displayed data.
        /// </summary>
        public void RefreshStat()
        {
            RefreshLists();
        }
        /// <summary>
        /// Changes Arrow Image indicators for whose turn it is.
        /// </summary>
        public void IsHisTurn(bool isMyTurn)
        {
            if (isMyTurn)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    imgYourArrow.Visibility = Visibility.Visible;
                    imgEnemyArrow.Visibility = Visibility.Hidden;
                }));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    imgYourArrow.Visibility = Visibility.Hidden;
                    imgEnemyArrow.Visibility = Visibility.Visible;
                }));
            }
        }
        /// <summary>
        /// Called by server to indicate that another player is found and sets the names block with it. Thread-safe.
        /// </summary>
        public void SetNamesWindow(string otherPlayerName)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                namesBlock.Text = string.Format($"{playerName}\nVS {otherPlayerName}");
            }));
        }
        #endregion

    }//class
}//namespace
