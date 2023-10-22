using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TaskScheduler.SubWindows
{
    public partial class AuthorizationWin : Window
    {
        private MainWindow? app = Application.Current.MainWindow as MainWindow;
        public AuthorizationWin()
        {
            InitializeComponent();
        }

        private void lblLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtLogin.Focus();
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtLogin.Text.Length > 0)
            {
                lblLogin.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblLogin.Visibility = Visibility.Visible;
            }
        }

        private void lblPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPassword.Focus();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPassword.SecurePassword.Length > 0)
            {
                lblPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblPassword.Visibility = Visibility.Visible;
            }
        }

        private void CancelAuthBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AccessAuthBtn_Click(object sender, RoutedEventArgs e)
        {
            if (txtLogin.Text.Length == 0 || txtPassword.SecurePassword.Length == 0)
            {
                MessageBox.Show("Заполните все поля!", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string result = TSCore.AuthUser(txtLogin.Text, txtPassword.SecurePassword);
            if (result != "")
            {
                MessageBox.Show("Успешно!", "Регистрация", MessageBoxButton.OK);
                app.SetToken(result);
                app.UpdateAssignmentsList();
                app.StartTimer();
                this.Close();
            }
        }
    }
}
