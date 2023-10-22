using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TaskScheduler.SubWindows
{
    public partial class EditAssignmentWin : Window
    {
        private MainWindow? app = Application.Current.MainWindow as MainWindow;
        private int assignmentId;
        public EditAssignmentWin(int id, string title, string deadline, string description)
        {
            InitializeComponent();
            txtEditDescription.Text = description;
            txtEditTime.Text = deadline;
            txtEditTitle.Text = title;
            assignmentId = id;
        }

        private void AccessEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (txtEditDescription.Text.Length == 0 || txtEditTime.Text.Length == 0 || txtEditTitle.Text.Length == 0)
            {
                MessageBox.Show("Заполните все поля!", "Редактирование задачи", MessageBoxButton.OK, MessageBoxImage.Error); 
                return;
            }

            Regex regex = new Regex("^[0-2]{1}[0-9]{1}:[0-5]{1}[0-9]{1}-[0-2]{1}[0-9]{1}:[0-5]{1}[0-9]{1}$");
            if (!regex.IsMatch(txtEditTime.Text))
            {
                MessageBox.Show("Проверьте правильность введенного времени (Например, 00:01-23:59)", "Редактирование задачи", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string token = app.GetToken();
            if (TSCore.ValidateToken(token))
            {
                bool result = TSCore.EditAssignment(assignmentId, new Dictionary<string, object> { ["name"] = txtEditTitle.Text, ["deadline"] = txtEditTime.Text, ["description"] = txtEditDescription.Text });
                if (result) 
                {
                    MessageBox.Show("Успешно!", "Редактирование задачи", MessageBoxButton.OK);
                    app.UpdateAssignmentsList();
                    this.Close();
                }
            }
        }

        private void CancelEditBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtEditDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtEditDescription.Text.Length > 0)
            {
                lblEditDescription.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblEditDescription.Visibility = Visibility.Visible;
            }
        }

        private void lblEditDescription_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEditDescription.Focus();
        }

        private void lblEditTime_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEditTime.Focus();
        }

        private void txtEditTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtEditTime.Text.Length > 0)
            {
                lblEditTime.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblEditTime.Visibility = Visibility.Visible;
            }
        }

        private void lblEditTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEditTitle.Focus();
        }

        private void txtEditTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtEditTitle.Text.Length > 0)
            {
                lblEditTitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblEditTitle.Visibility = Visibility.Visible;
            }
        }
    }
}
