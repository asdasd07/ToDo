using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToDo {
    /// <summary>
    /// Interaction logic for the class MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        enum Mode { Hide, View, Edit };
        Database dataObject;
        Dictionary<int, string> groups = new Dictionary<int, string>();
        List<CustomButton> Tasks = new List<CustomButton>();
        CustomButton Selected;
        bool DayFilterOn = false;
        int filter = 0;

        /// <summary>
        /// MainWindow constructor
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            dataObject = new Database();
            Refreshgroups();
            CCalendar.DayChanged += Calendar_DayChanged;
            CCalendar.MonthChanged += Calendar_MonthChanged;
        }

        /// <summary>
        /// Refresh whole window
        /// </summary>
        void Refresh() {
            Refreshgroups();
            RefreshTask();
            ChangeMode(Mode.Hide);
        }
        /// <summary>
        /// Get fresh info about Tasks from database and put them into TaskBox
        /// </summary>
        void RefreshTask() {
            dataObject.OpenConnection();
            TaskBox.Items.Clear();
            System.Data.SQLite.SQLiteDataReader data = dataObject.Select(filter, CCalendar.selectedDay.Month, CCalendar.selectedDay.Year);

            Tasks.Clear();
            TaskBox.Items.Clear();
            if (data != null && data.HasRows) {
                while (data.Read()) {
                    Tasks.Add(new CustomButton(data));
                }
                foreach (CustomButton b in Tasks) {
                    if (!DayFilterOn || DayFilterOn && CCalendar.selectedDay.Day == b.deadline.Day) {
                        TaskBox.Items.Add(b);
                    }
                }
            }
            dataObject.CloseConnection();
            CCalendar.UpdateCalendar(Tasks);
        }
        /// <summary>
        /// Put task info to TaskBox for selected day, without database querry
        /// </summary>
        void QuickRefresh() {
            TaskBox.Items.Clear();
            foreach (CustomButton b in Tasks) {
                if (CCalendar.selectedDay.Day == b.deadline.Day) {
                    TaskBox.Items.Add(b);
                }
            }
        }
        /// <summary>
        /// Get fresh info about Groups from database and put them into ComboBoxes
        /// </summary>
        void Refreshgroups() {
            dataObject.OpenConnection();
            groups.Clear();
            var data = dataObject.SelectGroups();
            if (data.HasRows) {
                while (data.Read()) {
                    groups.Add(int.Parse(data["id"].ToString()), data["name"].ToString());
                }
            }
            dataObject.CloseConnection();
            ECombo.ItemsSource = groups.Values;

            if (gFilter.SelectedItem == null || !groups.ContainsValue(gFilter.SelectedItem.ToString())) {
                gFilter.SelectedIndex = 0;
                filter = 0;
            }
            var group2 = groups.Values.ToList();
            group2.Insert(0, "Any");
            gFilter.ItemsSource = group2;
        }

        /// <summary>
        /// Reaction for changing selected task in ListBox
        /// </summary>
        void TaskBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox so = (ListBox)sender;
            if (Selected != null) {
                Selected.BorderBrush = Brushes.Transparent;
            }
            Selected = (CustomButton)so.SelectedItem;
            if (Selected != null) {
                Selected.BorderBrush = Brushes.CornflowerBlue;
                EnterDetails(Selected);
            }
        }
        /// <summary>
        /// Enter task details into tab
        /// </summary>
        /// <param name="CB">Selected CustomButton. null - create empty</param>
        /// <param name="edit">View edit mode</param>
        void EnterDetails(CustomButton CB = null, bool edit = false) {
            if (edit) {
                ChangeMode(Mode.Edit);
                if (CB == null) {
                    CreateButton.Visibility = Visibility.Visible;
                    SubmitButton.Visibility = Visibility.Hidden;
                    ETitle.Text = "";
                    EDesc.Clear();
                    ECombo.SelectedValue = "";
                    EDeadline.Text = CCalendar.selectedDay.ToShortDateString();
                    DoneCheckbox.IsChecked = false;
                } else {
                    CreateButton.Visibility = Visibility.Hidden;
                    SubmitButton.Visibility = Visibility.Visible;
                    ETitle.Text = CB.title;
                    EDesc.Text = CB.description;
                    ECombo.SelectedValue = CB.group;
                    EDeadline.Text = CB.deadline.ToShortDateString();
                    DoneCheckbox.IsChecked = CB.complete;
                }
            } else {
                ChangeMode(Mode.View);
                VTitle.Content = CB.title;
                DoneCheckbox.IsChecked = CB.complete;
                VDesc.Text = CB.description;
                VDeadline.Content = CB.deadline.ToString("dddd, dd MMMM yyyy");
                VGroup.Content = CB.group;
            }
        }
        /// <summary>
        /// Change mode between edit, details or hide mode.
        /// </summary>
        /// <param name="mod">Hide, View, Edit</param>
        void ChangeMode(Mode mod) {
            switch (mod) {
                case Mode.Hide:
                    gLabels.Visibility = Visibility.Hidden;
                    gEdit.Visibility = Visibility.Hidden;
                    gView.Visibility = Visibility.Hidden;
                    DoneCheckbox.Visibility = Visibility.Hidden;
                    break;
                case Mode.View:
                    gLabels.Visibility = Visibility.Visible;
                    gEdit.Visibility = Visibility.Hidden;
                    gView.Visibility = Visibility.Visible;
                    DoneCheckbox.Visibility = Visibility.Visible;
                    break;
                case Mode.Edit:
                    gLabels.Visibility = Visibility.Visible;
                    gEdit.Visibility = Visibility.Visible;
                    gView.Visibility = Visibility.Hidden;
                    DoneCheckbox.Visibility = Visibility.Visible;
                    break;
            }
        }
        /// <summary>
        /// Reaction for calendar day change
        /// </summary>
        void Calendar_DayChanged(object sender, RoutedEventArgs e) {
            if (gEdit.Visibility == Visibility.Visible) {
                EDeadline.Text = CCalendar.selectedDay.ToShortDateString();
            }
            if (DayFilterOn) {
                QuickRefresh();
            }
        }
        /// <summary>
        /// Reaction for calendar month change
        /// </summary>
        void Calendar_MonthChanged(object sender, RoutedEventArgs e) {
            if (gEdit.Visibility == Visibility.Hidden) {
                Refresh();
            }
        }
        /// <summary>
        /// Reaction for Delete button
        /// </summary>
        private void Del_Click(object sender, RoutedEventArgs e) {
            if (Selected == null) {
                return;
            }
            dataObject.DeleteTask(Selected.id, Selected.groupID);
            Refresh();
        }
        /// <summary>
        /// Reaction for Cancel button
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            if (Selected == null) {
                ChangeMode(Mode.Hide);
            } else {
                ChangeMode(Mode.View);
            }
        }
        /// <summary>
        /// Reaction for Add button
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e) {
            ChangeMode(Mode.Edit);
            Selected = null;
            CreateButton.Visibility = Visibility.Visible;
            SubmitButton.Visibility = Visibility.Hidden;
            EnterDetails(null, true);
        }
        /// <summary>
        /// Reaction for Edit button
        /// </summary>
        private void Edit_Click(object sender, RoutedEventArgs e) {
            if (Selected == null) {
                return;
            }
            ChangeMode(Mode.Edit);
            CreateButton.Visibility = Visibility.Hidden;
            SubmitButton.Visibility = Visibility.Visible;
            EnterDetails(Selected, true);
        }
        /// <summary>
        /// Checks whether each of the required fields has been filled in
        /// </summary>
        /// <returns>True if filled in</returns>
        bool IsFilled() {
            if (ETitle.Text.Length == 0) {
                MessageBox.Show("Title must have at least 1 character");
                return false;
            }
            if (ECombo.Text.Length == 0) {
                MessageBox.Show("Group must have at least 1 character");
                return false;
            }
            if (!DateTime.TryParseExact(EDeadline.Text, "d.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out _)) {
                MessageBox.Show("Deadline has an invalid format");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Reaction for Create button
        /// </summary>
        private void Create_Click(object sender, RoutedEventArgs e) {
            if (!IsFilled()) {
                return;
            }
            string g = ECombo.Text;
            g = g.Substring(0, Math.Min(20, g.Length));
            if (!groups.ContainsValue(g)) {
                if (!dataObject.GroupExists(g)) {
                    dataObject.AddGroup(g);
                }
                Refreshgroups();
            }
            int Gid = groups.FirstOrDefault(x => x.Value == g).Key;
            var str = EDesc.Text;
            dataObject.AddTask(ETitle.Text, DateTime.Parse(EDeadline.Text), Gid, str);
            Refresh();
        }
        /// <summary>
        /// Reaction for Submit button
        /// </summary>
        private void Submit_Click(object sender, RoutedEventArgs e) {
            if (Selected == null) {
                return;
            }
            if (!IsFilled()) {
                return;
            }
            string g = ECombo.Text;
            if (!groups.ContainsValue(g)) {
                if (!dataObject.GroupExists(g)) {
                    dataObject.AddGroup(g);
                }
                Refreshgroups();
            }
            int Gid = groups.First(x => x.Value == g).Key;
            var str = EDesc.Text;
            dataObject.UpdateTask(Selected.id, ETitle.Text, DateTime.Parse(EDeadline.Text), DoneCheckbox.IsChecked.Value, Gid, str);
            Refresh();
        }
        /// <summary>
        /// Reaction for changing filter group in ComboBox
        /// </summary>
        private void gFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string g = gFilter.SelectedItem.ToString();
            filter = groups.FirstOrDefault(x => x.Value == g).Key;
            Refresh();
        }

        /// <summary>
        /// Reaction for click on Done CheckBox in view mode
        /// </summary>
        private void Done_Click(object sender, RoutedEventArgs e) {
            if (Selected == null) {
                return;
            }
            dataObject.CompleteTask(Selected.id, DoneCheckbox.IsChecked.Value);
            Refresh();
        }
        /// <summary>
        /// Reaction for click on FilterByDay CheckBox
        /// </summary>
        private void DayFilter_Click(object sender, RoutedEventArgs e) {
            DayFilterOn = DayCheckbox.IsChecked.Value;
            if (DayFilterOn) {
                QuickRefresh();
            } else {
                Refresh();
            }
        }

    }
}
