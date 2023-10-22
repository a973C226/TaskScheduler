using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TaskScheduler.Models;
using TaskScheduler.SubWindows;

namespace TaskScheduler
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private string authToken = "";
        private string[] monthsNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
        private Dictionary<string, string> daysOfWeekNames = new Dictionary<string, string>
        { ["Monday"] = "Понедельник", 
            ["Tuesday"] = "Вторник", 
            ["Wednesday"] = "Среда", 
            ["Thursday"] = "Четверг",
            ["Friday"] = "Пятница", 
            ["Saturday"] = "Суббота", 
            ["Sunday"] = "Воскресение" };

        public MainWindow()
        {
            InitializeComponent();

            timer.Tick += new EventHandler(timerTick);

            if (!TSCore.CreateContext())
                Application.Current.Shutdown();

        }

        private void timerTick(object? sender, EventArgs e)
        {
            timer.Interval = new TimeSpan(1, 0, 0);
            if (TSCore.ValidateToken(authToken))
            {
                bool sendEmail = false;
                List<string> assignmentsTitles = new List<string>();
                List<Assignment> assignments = TSCore.GetAssignments(authToken, DateTime.Now.Date.ToString());
                foreach (Assignment assignment in assignments)
                {
                    if (assignment.IsActive && !assignment.IsMuted)
                    {
                        string deadlineStr = assignment.Deadline.Split('-')[1];
                        TimeSpan deadlineTimeSpan = new TimeSpan();
                        TimeSpan.TryParse(deadlineStr, new CultureInfo("ru-RU"), out deadlineTimeSpan);
                        TimeSpan timeNow = DateTime.Now.TimeOfDay;
                        if (timeNow > deadlineTimeSpan) 
                        {
                            if (timeNow.Add(new TimeSpan(1, 0, 0)) > deadlineTimeSpan)
                            {
                                sendEmail = true;
                                assignmentsTitles.Add(assignment.Name);
                            }
                        }
                    }
                }
                if (sendEmail)
                {
                    /*TSCore.SendEmail(assignmentsTitles, authToken).GetAwaiter();*/
                }
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime dateNow = DateTime.Today;
            calendar.SelectedDate = dateNow;
        }

        public void StartTimer()
        {
            timer.Interval = new TimeSpan(0, 0, 0);
            timer.Start();
        }

        public void StopTimer() 
        {
            timer.Stop();
            timer.Interval = new TimeSpan(0, 0, 0);
        }

        public void SetDate(DateTime date)
        {
            selectedMonthLbl.Text = monthsNames[date.Month - 1];
            dayNowLbl.Text = date.Day.ToString();
            monthNowLbl.Text = monthsNames[date.Month - 1];
            dayOfWeekNowLbl.Text = daysOfWeekNames[date.DayOfWeek.ToString()];
        }

        public void SetToken(string token)
        {
            authToken = token;
            txtToken.Text = authToken;
        }

        public string GetToken() { return authToken; }

        public void ClearAssignmentsList()
        {
            assignmentsList.Children.Clear();
        }

        public void AddAssignmentInList(Assignment assignment)
        {
            UserControls.Item item = new UserControls.Item();
            item.Id = assignment.Id;
            item.Title = assignment.Name;
            item.Time = assignment.Deadline;
            item.NoteDescription = assignment.Description;
            if (assignment.IsActive)
            {
                item.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f1f1"));
                item.Icon = FontAwesome.WPF.FontAwesomeIcon.CircleThin;

            }
            else
            {
                item.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#eba5bb"));
                item.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
            }
            if (assignment.IsMuted)
            {
                item.IconBell = FontAwesome.WPF.FontAwesomeIcon.BellSlash;
                item.UnmuteBtn.Visibility = Visibility.Visible;
                item.MuteBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                item.IconBell = FontAwesome.WPF.FontAwesomeIcon.Bell;
                item.UnmuteBtn.Visibility = Visibility.Collapsed;
                item.MuteBtn.Visibility = Visibility.Visible;
            }
            assignmentsList.Children.Add(item);
        }

        public void UpdateAssignmentsList(string date = null)
        {
            ClearAssignmentsList();
            SetDate(calendar.SelectedDate.Value);
            string selectedDate = calendar.SelectedDate.Value.Date.ToString();
            if (date != null)
                selectedDate = date;
            List<Assignment> assignments = TSCore.GetAssignments(authToken, selectedDate);
            int activeAssignments = 0;
            foreach (Assignment assignment in assignments) 
            {
                if (assignment.IsActive) 
                {
                    activeAssignments++;
                }
                AddAssignmentInList(assignment);
            }
            countAssignments.Text = string.Format("Total {0} tasks - {1} tasks left", assignments.Count, activeAssignments);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void lblNote_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtNote.Focus();
        }

        private void txtNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNote.Text.Length > 0)
            {
                lblNote.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblNote.Visibility = Visibility.Visible;
            }
        }

        private void lblTime_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtTime.Focus();
        }

        private void txtTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (txtTime.Text.Length > 0)
            {
                lblTime.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblTime.Visibility = Visibility.Visible;
            }
        }

        private void AddAssignmentBtn_Click(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex("^[0-2]{1}[0-9]{1}:[0-5]{1}[0-9]{1}-[0-2]{1}[0-9]{1}:[0-5]{1}[0-9]{1}$");
            if (txtNote.Text.Length == 0)
            {
                MessageBox.Show("Введите название задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!regex.IsMatch(txtTime.Text))
            {
                MessageBox.Show("Проверьте правильность введенного времени (Например, 00:01-23:59)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (TSCore.ValidateToken(authToken))
            {
                NoteDescription noteDescription = new NoteDescription(authToken, txtNote.Text, txtTime.Text, calendar.SelectedDate.Value.Date.ToString());
                noteDescription.Show();
            }

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AuthBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWin authWin = new AuthorizationWin();
            authWin.Show();
        }

        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            RegistrWin registrWin = new RegistrWin();
            registrWin.Show();
        }

        private void txtToken_TextChanged(object sender, TextChangedEventArgs e)
        {
            authToken = txtToken.Text;
        }

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime selectedDate = calendar.SelectedDate.Value;
            if (this.IsLoaded)
            {
                SetDate(selectedDate);
            }
            if (authToken != "")
            {
                UpdateAssignmentsList(selectedDate.Date.ToString());
            }
        }

        private void YesterdayBtn_Click(object sender, RoutedEventArgs e)
        {
            calendar.SelectedDate = calendar.SelectedDate.Value.AddDays(-1);
        }

        private void TomorrowBtn_Click(object sender, RoutedEventArgs e)
        {
            calendar.SelectedDate = calendar.SelectedDate.Value.AddDays(1);
        }
    }
}
