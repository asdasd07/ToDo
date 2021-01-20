using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToDo {
    /// <summary>
    /// Interaction logic for the class MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Database dataObject;
        Dictionary<int, string> groups = new Dictionary<int, string>();
        CustomButton Selected;
        DateTime DayFilter = DateTime.Today;
        bool DayFilterOn = false;
        int filter = 0;
        enum Mode { Hide, View, Edit };

        int prevMonthCount;
        int monthOffset = 0;
        public DateTime selectedMonth;
        private int daysInMonth;
        private int firstDayOfWeek;
        private List<Button> dayList = new List<Button>();
        List<CustomButton> Tasks = new List<CustomButton>();


        /// <summary>
        /// MainWindow constructor
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            dataObject = new Database();
            CreateMonth();
            RefreshGroups();
        }
        /// <summary>
        /// Refresh whole window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh() {
            RefreshGroups();
            RefreshTask();
            ChangeMode(Mode.Hide);
        }
        /// <summary>
        /// Get fresh info about Tasks from database and put them into ListBox
        /// </summary>
        public void RefreshTask() {
            dataObject.OpenConnection();
            LiBox.Items.Clear();
            System.Data.SQLite.SQLiteDataReader data = dataObject.Select(filter, selectedMonth.Month, selectedMonth.Year);

            Tasks.Clear();
            LiBox.Items.Clear();
            if (data != null && data.HasRows) {
                while (data.Read()) {
                    Tasks.Add(new CustomButton(data));
                }
                foreach (CustomButton b in Tasks) {
                    if (!DayFilterOn || DayFilterOn && DayFilter.Day == b.deadline.Day) {
                        LiBox.Items.Add(b);
                    }
                }
            }
            dataObject.CloseConnection();
            UpdateCalendar();
        }
        public void CreateMonth() {
            selectedMonth = DateTime.Today.AddMonths(monthOffset);
            daysInMonth = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);
            DateTime firstDay = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            firstDayOfWeek = (int)firstDay.DayOfWeek;
            DateTime prevMonth = DateTime.Today.AddMonths(monthOffset - 1);
            prevMonthCount = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            calendarGrid.Children.RemoveRange(7, calendarGrid.Children.Count - 7);
            dayList.Clear();

            int offset = (firstDayOfWeek + 6) % 7;
            offset = offset == 0 ? 6 : offset - 1;
            int day = prevMonthCount - offset;
            bool part = false;
            for (int row = 0; row < 6; row++) {
                for (int col = 0; col < 7; col++) {
                    DayButton(col, row, day, part);
                    if (day == daysInMonth && part || day == prevMonthCount && !part) {
                        day = 0;
                        part = !part;
                    }
                    day++;
                }
            }
            CalendarMonth.Content = selectedMonth.ToString("MMMM yyyy");
        }

        private void DayButton(int col, int row, int day, bool enable) {
            Button btn = new Button {
                FontSize = 10,
                FontWeight = FontWeights.Light,
                Name = "Button" + day,
                Margin = new Thickness(1, 1, 1, 1),
                IsHitTestVisible = enable,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            btn.Click += SelectDay_Click;
            Grid.SetRow(btn, row + 1);
            Grid.SetColumn(btn, col + 1);

            CustomCalendarDay customCalendarDay = new CustomCalendarDay();
            btn.Content = customCalendarDay;
            btn.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            btn.VerticalContentAlignment = VerticalAlignment.Stretch;
            calendarGrid.Children.Add(btn);
            Label label = (Label)((Grid)customCalendarDay.Content).Children[1];
            label.Content = day;
            label.Foreground = enable ? Brushes.White : Brushes.LightGray;
            if (enable) {
                dayList.Insert(day, btn);
            } else {
                dayList.Add(btn);
            }
        }

        private void UpdateCalendar() {
            for (int i = 1; i <= daysInMonth; i++) {
                DateTime date = new DateTime(selectedMonth.Year, selectedMonth.Month, i);
                List<CustomButton> t = Tasks.FindAll(x => x.deadline.Date == date.Date && x.complete == false);

                int cols = (int)Math.Ceiling(Math.Sqrt(t.Count));
                int rows = (int)Math.Ceiling((float)t.Count / cols);

                CustomCalendarDay customCalendarDay = (CustomCalendarDay)dayList[i].Content;
                Grid group = (Grid)customCalendarDay.Content;
                group.Children.RemoveAt(0);

                Grid grid = new Grid();
                for (int j = 0; j < cols; j++) {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                for (int j = 0; j < rows; j++) {
                    grid.RowDefinitions.Add(new RowDefinition());
                }
                for (int j = 0; j < t.Count; j++) {
                    var r = new Rectangle { Fill = Tools.getTaskBrush(t[j].groupID, t[j].id) };
                    r.SetValue(Grid.ColumnProperty, j % cols);
                    r.SetValue(Grid.RowProperty, j / cols);
                    grid.Children.Add(r);
                }
                group.Children.Insert(0, grid);
            }
            HighlightButton();
        }

        public void HighlightButton() {
            foreach (Button b in dayList) {
                b.BorderThickness = new Thickness(0);
            }
            if (selectedMonth.Month == DateTime.Now.Month) {
                Button b = dayList[DateTime.Now.Day];
                b.BorderThickness = new Thickness(1);
                b.BorderBrush = Brushes.White;
            }
            Button btn = dayList[DayFilter.Day];
            btn.BorderThickness = new Thickness(1);
            btn.BorderBrush = Brushes.Blue;
        }


        /// <summary>
        /// Get fresh info about Groups from database and put them into ComboBoxes
        /// </summary>
        public void RefreshGroups() {
            dataObject.OpenConnection();
            groups.Clear();
            var data = dataObject.SelectGroups();
            if (data.HasRows) {
                while (data.Read()) {
                    groups.Add(int.Parse(data["id"].ToString()), data["name"].ToString());
                }
            }
            dataObject.CloseConnection();
            Combo.ItemsSource = groups.Values;

            if (Com.SelectedItem == null || !groups.ContainsValue(Com.SelectedItem.ToString())) {
                Com.SelectedIndex = 0;
                filter = 0;
            }
            var group2 = groups.Values.ToList();
            group2.Insert(0, "Any");
            Com.ItemsSource = group2;
        }

        /// <summary>
        /// Reaction for changing selected task in ListBox
        /// </summary>
        private void LiBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
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
                    CTitle.Text = "";
                    CDesc.Clear();
                    Combo.SelectedValue = "";
                    EdDeadline.Text = DayFilter.ToShortDateString();
                    shdone.IsChecked = false;
                } else {
                    CreateButton.Visibility = Visibility.Hidden;
                    SubmitButton.Visibility = Visibility.Visible;
                    CTitle.Text = CB.title;
                    CDesc.Text = CB.description;
                    Combo.SelectedValue = CB.group;
                    EdDeadline.Text = CB.deadline.ToShortDateString();
                    shdone.IsChecked = CB.complete;
                }
            } else {
                ChangeMode(Mode.View);
                shTitle.Content = CB.title;
                shdone.IsChecked = CB.complete;
                shDesc.Text = CB.description;
                shDeadline.Content = CB.deadline.ToString("dddd, dd MMMM yyyy");
                shgroup.Content = CB.group;
            }
        }

        void ChangeMode(Mode mod) {
            switch (mod) {
                case Mode.Hide:
                    gLabels.Visibility = Visibility.Hidden;
                    gEdit.Visibility = Visibility.Hidden;
                    gView.Visibility = Visibility.Hidden;
                    shdone.Visibility = Visibility.Hidden;
                    break;
                case Mode.View:
                    gLabels.Visibility = Visibility.Visible;
                    gEdit.Visibility = Visibility.Hidden;
                    gView.Visibility = Visibility.Visible;
                    shdone.Visibility = Visibility.Visible;
                    break;
                case Mode.Edit:
                    gLabels.Visibility = Visibility.Visible;
                    gEdit.Visibility = Visibility.Visible;
                    gView.Visibility = Visibility.Hidden;
                    shdone.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SubMonth_Click(object sender, RoutedEventArgs e) {
            monthOffset--;
            DayFilter = DayFilter.AddMonths(-1);
            CreateMonth();
            Refresh();
        }
        private void AddMonth_Click(object sender, RoutedEventArgs e) {
            monthOffset++;
            DayFilter = DayFilter.AddMonths(1);
            CreateMonth();
            Refresh();
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
        /// <returns>true if filled in</returns>
        bool IsFilled() {
            if (CTitle.Text.Length == 0) {
                MessageBox.Show("Title must have at least 1 character");
                return false;
            }
            if (Combo.Text.Length == 0) {
                MessageBox.Show("Group must have at least 1 character");
                return false;
            }
            if (!DateTime.TryParseExact(EdDeadline.Text, "d.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out _)) {
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
            string g = Combo.Text;
            g = g.Substring(0, Math.Min(20, g.Length));
            if (!groups.ContainsValue(g)) {
                if (!dataObject.GroupExists(g)) {
                    dataObject.AddGroup(g);
                }
                RefreshGroups();
            }
            int Gid = groups.FirstOrDefault(x => x.Value == g).Key;
            var str = CDesc.Text;
            dataObject.AddTask(CTitle.Text, DateTime.Parse(EdDeadline.Text), Gid, str);
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
            string g = Combo.Text;
            if (!groups.ContainsValue(g)) {
                if (!dataObject.GroupExists(g)) {
                    dataObject.AddGroup(g);
                }
                //RefreshGroups();
            }
            int Gid = groups.First(x => x.Value == g).Key;
            var str = CDesc.Text;
            dataObject.UpdateTask(Selected.id, CTitle.Text, DateTime.Parse(EdDeadline.Text), shdone.IsChecked.Value, Gid, str);
            Refresh();
        }
        /// <summary>
        /// Reaction for changing filter group in ComboBox
        /// </summary>
        private void Com_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string g = Com.SelectedItem.ToString();
            filter = groups.FirstOrDefault(x => x.Value == g).Key;
            Refresh();
        }

        /// <summary>
        /// Reaction for click on Done CheckBox in view mode
        /// </summary>
        private void Shdone_Click(object sender, RoutedEventArgs e) {
            if (Selected == null) {
                return;
            }
            dataObject.CompleteTask(Selected.id, shdone.IsChecked.Value);
            Refresh();
        }

        private void SelectDay_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            DayFilter = new DateTime(selectedMonth.Year, selectedMonth.Month, int.Parse(btn.Name.Substring(6)));
            if (gEdit.Visibility == Visibility.Visible) {
                EdDeadline.Text = DayFilter.ToShortDateString();
            }
            HighlightButton();
            if (DayFilterOn) {
                Refresh();
            }
        }

        /// <summary>
        /// Reaction for click on FilterByDay CheckBox
        /// </summary>
        private void DayFilter_Click(object sender, RoutedEventArgs e) {
            DayFilterOn = DayCheckbox.IsChecked.Value;
            Refresh();
        }

    }
}
