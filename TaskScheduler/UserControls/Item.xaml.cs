using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskScheduler.SubWindows;

namespace TaskScheduler.UserControls
{
    public partial class Item : UserControl
    {
        private MainWindow app = Application.Current.MainWindow as MainWindow;

        public Item()
        {
            InitializeComponent();
        }

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.
            Register("Id", typeof(int), typeof(Item));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.
            Register("Title", typeof(string), typeof(Item));

        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.
            Register("Time", typeof(string), typeof(Item));

        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.
            Register("Color", typeof(SolidColorBrush), typeof(Item));

        public FontAwesome.WPF.FontAwesomeIcon Icon
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.
            Register("Icon", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(Item));

        public FontAwesome.WPF.FontAwesomeIcon IconBell
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)GetValue(IconBellProperty); }
            set { SetValue(IconBellProperty, value); }
        }

        public static readonly DependencyProperty IconBellProperty = DependencyProperty.
            Register("IconBell", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(Item));

        public string NoteDescription
        {
            get { return (string)GetValue(NoteDescriptionProperty); }
            set { SetValue(NoteDescriptionProperty, value); }
        }

        public static readonly DependencyProperty NoteDescriptionProperty = DependencyProperty.
            Register("NoteDescription", typeof(string), typeof(Item));

        private void border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.gridDesc.Visibility == Visibility.Collapsed)
                this.gridDesc.Visibility = Visibility.Visible;
            else
                this.gridDesc.Visibility = Visibility.Collapsed;
        }

        private void CheckBtn_Click(object sender, RoutedEventArgs e)
        {
            string token = app.GetToken();
            if (TSCore.ValidateToken(token))
            {
                bool result = TSCore.EditAssignment(this.Id, new Dictionary<string, object> { ["IsActive"] = false } );
                if (result)
                {
                    this.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#eba5bb"));
                    this.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
                    this.IconBell = FontAwesome.WPF.FontAwesomeIcon.BellSlash;
                    app.UpdateAssignmentsList();
                }    
            }
        }

        private void MuteBtn_Click(object sender, RoutedEventArgs e)
        {
            string token = app.GetToken();
            if (TSCore.ValidateToken(token))
            {
                bool result = TSCore.EditAssignment(this.Id, new Dictionary<string, object> { ["IsMuted"] = true });
                if (result)
                {
                    MuteBtn.Visibility = Visibility.Collapsed;
                    UnmuteBtn.Visibility = Visibility.Visible;
                    this.IconBell = FontAwesome.WPF.FontAwesomeIcon.BellSlash;
                }
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            EditAssignmentWin editWin = new EditAssignmentWin(this.Id, this.Title, this.Time, this.NoteDescription);
            editWin.Show();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            string token = app.GetToken();
            if (TSCore.ValidateToken(token))
            {
                if ( MessageBox.Show("Хотите удалить задачу?", "Удаление", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    bool result = TSCore.DeleteAssignment(this.Id);
                    if (result)
                    {
                        this.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void UnmuteBtn_Click(object sender, RoutedEventArgs e)
        {
            string token = app.GetToken();
            if (TSCore.ValidateToken(token))
            {
                bool result = TSCore.EditAssignment(this.Id, new Dictionary<string, object> { ["IsMuted"] = false });
                if (result)
                {
                    MuteBtn.Visibility = Visibility.Visible;
                    UnmuteBtn.Visibility = Visibility.Collapsed;
                    this.IconBell = FontAwesome.WPF.FontAwesomeIcon.Bell;
                }
            }
        }
    }
}
