using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskScheduler.Models;

namespace TaskScheduler.SubWindows
{
    public partial class NoteDescription : Window
    {
        private MainWindow? app = Application.Current.MainWindow as MainWindow;
        private string token;
        private string assignmentName;
        private string deadline;
        private string date;
        public NoteDescription(string token, string assignmentName, string deadline, string date)
        {
            InitializeComponent();
            this.token = token;
            this.assignmentName = assignmentName;
            this.deadline = deadline;
            this.date = date;
        }

        private void AccessBtn_Click(object sender, RoutedEventArgs e)
        {
            Assignment? result = TSCore.AddAssignment(token, assignmentName, deadline, txtDesc.Text, date);
            if (result != null)
            {
                MessageBox.Show("Успешно!", "Новая задача", MessageBoxButton.OK);
                app.UpdateAssignmentsList();
                app.txtNote.Text = "";
                app.txtTime.Text = "";
                this.Close();
            }
        }

        private void lblDesc_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtDesc.Focus();
        }

        private void txtDesc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtDesc.Text.Length > 0)
            {
                lblDesc.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblDesc.Visibility = Visibility.Visible;
            }
        }
    }
}
