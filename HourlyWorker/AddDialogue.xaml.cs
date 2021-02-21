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
    /// Interaction logic for AddDialogue.xaml
    /// </summary>
    public partial class AddDialogue : Window
    {

        /// <summary>
        /// Main window instance reference.
        /// </summary>
        private MainWindow mainWindow;

        /// <summary>
        /// Window initialization.
        /// </summary>
        public AddDialogue()
        {
            InitializeComponent();

            mainWindow = (MainWindow)Application.Current.MainWindow;
        }

        /// <summary>
        /// Close this window.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Add new WorkProject.
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            WorkProject wp = new WorkProject(NameInput.Text, new CustomSpan());
            if (mainWindow.AddProject(wp))
                Close();
        }

        /// <summary>
        /// Check if the input name is correct.
        /// </summary>
        private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddButton.IsEnabled = mainWindow.CheckProjectName(NameInput.Text, false);
        }
    }
}
