using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    #region CustomEventDeclaration
    /// <summary>
    /// EventArgs element that is made to keep a string object (name) within itself.
    /// </summary>
    public class NameRoutedEventArgs : RoutedEventArgs
    {
        private string name;
        public NameRoutedEventArgs(string _data)
        {
            name = _data;
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
    }
    public delegate void NameRoutedEventHandler(object sender, NameRoutedEventArgs e); 
    #endregion

    /// <summary>
    /// A window that lets user choose a name in a correct format and passes it to the parent window.
    /// </summary>
    public partial class NamePickerWindow : Window
    {
        #region Fields
        /// <summary>
        /// Event that is raised whenever player picks a correct name by clicking the button.
        /// </summary>
        public event NameRoutedEventHandler BtnPickClick;
        //Manu window
        private Window parent;
        //Boolean to check if window is closed without picking a correct name.
        private bool isSet = false;
        #endregion

        #region Constructors

        public NamePickerWindow(Window _parent)
        {
            InitializeComponent();
            parent = _parent;
            txtBoxName.Text = string.Format("Player{0}", new Random().Next(1, 99));
        }

        #endregion

        #region EventsHandling
        /// <summary>
        /// Handles button click. On valid name, close the program and pass the name to the parent Window.
        /// </summary>
        private void btnPick_Click(object sender, RoutedEventArgs e)
        {
            if (txtBoxName.Background == Brushes.Green)
            {
                BtnPickClick?.Invoke(this, new NameRoutedEventArgs(txtBoxName.Text));
                isSet = true;
                this.Close();
            }
        }
        /// <summary>
        /// Validation on every text change.
        /// </summary>
        private void txtBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ValidateTxtBox())
            {
                txtBoxName.Background = Brushes.Red;
                return;
            }
            else
            {
                txtBoxName.Background = Brushes.Green;
            }
        }
        /// <summary>
        /// Enter click works the same way as the button for easier use.
        /// </summary>
        private void txtBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txtBoxName.Background == Brushes.Green) //on enter, pick it
            {
                BtnPickClick?.Invoke(this, new NameRoutedEventArgs(txtBoxName.Text));
                isSet = true;
                this.Close();
            }
        }
        #endregion

        #region ClassMethods
        /// <summary>
        /// Checks if the name is valid using Regex.
        /// </summary>
        /// <returns>Returns if the whole string is valid</returns>
        private bool ValidateTxtBox()
        {
            Regex rule = new Regex("^[A-Z]([A-Z]|[a-z]|[0-9])*$");
            var match = rule.Match(txtBoxName.Text);
            return match.Success;
        }
        /// <summary>
        /// If window is closed but name is not picked, parent window will show.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            if (!isSet)
            {
                parent.Show();
            }
        } 
        #endregion


    }
}
