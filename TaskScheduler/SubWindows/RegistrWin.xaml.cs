using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TaskScheduler.SubWindows
{
    public partial class RegistrWin : Window
    {
        private MainWindow? app = Application.Current.MainWindow as MainWindow;
        public RegistrWin()
        {
            InitializeComponent();
        }

        private void lblRegLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtRegLogin.Focus();
        }

        private void txtRegLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtRegLogin.Text.Length > 0)
            {
                lblRegLogin.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblRegLogin.Visibility = Visibility.Visible;
            }
        }

        private void lblRegPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtRegPassword.Focus();
        }

        private void txtRegPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtRegPassword.SecurePassword.Length > 0)
            {
                lblRegPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblRegPassword.Visibility = Visibility.Visible;
            }
        }

        private void lblRegRepeatPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtRegRepeatPassword.Focus();
        }

        private void txtRegRepeatPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtRegRepeatPassword.SecurePassword.Length > 0)
            {
                lblRegRepeatPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblRegRepeatPassword.Visibility = Visibility.Visible;
            }
        }

        private void AccessRegBtn_Click(object sender, RoutedEventArgs e)
        {
            if (txtRegLogin.Text.Length == 0 || txtRegPassword.SecurePassword.Length == 0 || txtRegRepeatPassword.SecurePassword.Length == 0)
            {
                MessageBox.Show("Заполните все поля!", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string patternEmail = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
            if (!Regex.IsMatch(txtRegLogin.Text, patternEmail, RegexOptions.IgnoreCase))
            {
                MessageBox.Show("Введите корректный Email", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TSCore.IsEqualTo(txtRegPassword.SecurePassword, txtRegRepeatPassword.SecurePassword))
            {
                MessageBox.Show("Пароли не совпадают :(", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (TSCore.RegisterUser(txtRegLogin.Text, txtRegPassword.SecurePassword))
            {
                app.ClearAssignmentsList();
                app.SetToken("");
                app.StopTimer();
                MessageBox.Show("Успешно!", "Регистрация", MessageBoxButton.OK);
                this.Close();
            }
        }

        private void CancelRegBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
