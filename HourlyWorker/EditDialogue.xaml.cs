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

namespace HourlyWorker
{
    /// <summary>
    /// Interaction logic for EditDialogue.xaml
    /// </summary>
    public partial class EditDialogue : Window
    {
        /// <summary>
        /// Main window instance reference.
        /// </summary>
        private MainWindow mainWindow;

        public EditDialogue()
        {
            InitializeComponent();

            mainWindow = (MainWindow)Application.Current.MainWindow;
            TimeInput.Text = $"{mainWindow.EditWorkProject.TimeSpan.Hours:00}:{mainWindow.EditWorkProject.TimeSpan.Minutes:00}";
        }

        /// <summary>
        /// On time input changed.
        /// </summary>
        private void TimeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditButton.IsEnabled = CheckInput(TimeInput.Text);
        }

        /// <summary>
        /// Close the edit dialogue window.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Edit the work project's time.
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetTime(TimeInput.Text, out int hours, out int minues))
            {
                mainWindow.EditWorkProject.TimeSpan.Edit(hours, minues);

                Close();
            }
        }

        private bool CheckInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            if (!input.Contains(':'))
                return false;

            string[] split = input.Split(':');
            if (split.Length != 2)
                return false;

            if (!int.TryParse(split[0], out int hours))
                return false;

            if (!int.TryParse(split[1], out int minutes)) 
                return false;

            if (minutes < 0 || minutes > 59)
                return false;

            if (hours < 0)
                return false;

            return true;
        }

        private bool GetTime(string input, out int hours, out int minutes)
        {
            hours = minutes = 0;
            if (!CheckInput(input))
                return false;

            string[] split = input.Split(':');
            hours = int.Parse(split[0]);
            minutes = int.Parse(split[1]);
            return true;
        }

    }
}
