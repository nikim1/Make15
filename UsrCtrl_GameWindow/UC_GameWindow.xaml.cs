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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UsrCtrl_GameWindow
{
    /// <summary>
    /// User Controls for a game of Make 15.
    /// </summary>
    public partial class UserControlGameWindow : UserControl
    {

        #region EventHandlers
        public event RoutedEventHandler BtnPickClick;
        public event RoutedEventHandler ChatEnterClick;
        #endregion

        #region Constructor

        public UserControlGameWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        public Image EnemyArrowImg
        {
            get
            {
                return imgEnemyArrow;
            }
        }
        public Image YourArrowImg
        {
            get
            {
                return imgYourArrow;
            }
        }
        public RichTextBox Chat
        {
            get
            {
                return txtBoxChat;//chat;
            }
        }
        public TextBlock NamesBlock
        {
            get
            {
                return txtBlkNames;//namesBlock;
            }
        }
        public ListBox ListBoxUntaken
        {
            get
            {
                return lstBoxUntaken;
            }
        }
        public ListBox ListBoxOpponent
        {
            get
            {
                return lstBoxOpponentNum;
            }
        }
        public ListBox ListBoxYour
        {
            get
            {
                return lstBoxYourNum;
            }
        }
        public TextBox ChatInputTextBox
        {
            get
            {
                return txtInput;
            }
        }

        #endregion

        #region EventHandlingMethods
        private void btnPick_Click(object sender, RoutedEventArgs e)
        {
            BtnPickClick?.Invoke(this, new RoutedEventArgs());
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txtInput.Text.Length != 0)
            {
                ChatEnterClick?.Invoke(this, new RoutedEventArgs());
                txtInput.Text = string.Empty;
            }
        } 
        #endregion
    }
}
