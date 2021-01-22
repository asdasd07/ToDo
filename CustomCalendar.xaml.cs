using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToDo {
    /// <summary>
    /// Interaction logic for CustomCalendar.xaml
    /// </summary>
    public partial class CustomCalendar : UserControl {
        public DateTime selectedDay = DateTime.Today;
        List<Button> dayList = new List<Button>();
        int daysInMonth;
        int firstDayOfWeek;
        int monthOffset = 0;

        public event EventHandler<RoutedEventArgs> DayChanged;
        public event EventHandler<RoutedEventArgs> MonthChanged;

        public CustomCalendar() {
            InitializeComponent();
            CreateMonth();
        }
        /// <summary>
        /// Create DayButton
        /// </summary>
        /// <param name="col">Column in calendar</param>
        /// <param name="row">Row in calendar</param>
        /// <param name="day">Assigned day</param>
        /// <param name="enable">Is enabled</param>
        private void DayButton(int col, int row, int day, bool enable) {
            Button btn = new Button {
                FontSize = 10,
                FontWeight = FontWeights.Light,
                Name = "Button" + day,
                Margin = new Thickness(1, 1, 1, 1),
                IsHitTestVisible = enable,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };
            btn.Click += SelectDay_Click;
            Grid.SetRow(btn, row + 1);
            Grid.SetColumn(btn, col + 1);

            Grid grid = new Grid();
            Label label = new Label {
                Content = day,
                Foreground = enable ? Brushes.White : Brushes.LightGray,
                Height = 23,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            grid.Children.Add(label);
            btn.Content = grid;
            calendarGrid.Children.Add(btn);

            if (enable) {
                dayList.Insert(day, btn);
            } else {
                dayList.Add(btn);
            }
        }
        /// <summary>
        /// Fill calendar with DayButtons for selected month
        /// </summary>
        public void CreateMonth() {
            DateTime tmp = DateTime.Today.AddMonths(monthOffset);
            selectedDay = new DateTime(tmp.Year, tmp.Month, selectedDay.Day);
            daysInMonth = DateTime.DaysInMonth(selectedDay.Year, selectedDay.Month);
            DateTime firstDay = new DateTime(selectedDay.Year, selectedDay.Month, 1);
            firstDayOfWeek = (int)firstDay.DayOfWeek;
            DateTime prevMonth = DateTime.Today.AddMonths(monthOffset - 1);
            int prevMonthCount = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

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
            CalendarMonth.Content = selectedDay.ToString("MMMM yyyy");
        }
        /// <summary>
        /// Fill DayButtons with grid of rectangles dependent on tasks
        /// </summary>
        /// <param name="Tasks">Tasks list</param>
        public void UpdateCalendar(List<CustomButton> Tasks) {
            for (int i = 1; i <= daysInMonth; i++) {
                DateTime date = new DateTime(selectedDay.Year, selectedDay.Month, i);
                List<CustomButton> t = Tasks.FindAll(x => x.deadline.Date == date.Date && x.complete == false);

                int cols = (int)Math.Ceiling(Math.Sqrt(t.Count));
                int rows = (int)Math.Ceiling((float)t.Count / cols);

                Grid group = (Grid)dayList[i].Content;
                if (group.Children.Count > 1) {
                    group.Children.RemoveAt(0);
                }

                Grid grid = new Grid();
                for (int j = 0; j < cols; j++) {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                for (int j = 0; j < rows; j++) {
                    grid.RowDefinitions.Add(new RowDefinition());
                }
                for (int j = 0; j < t.Count; j++) {
                    var r = new Rectangle { Fill = Tools.GetTaskBrush(t[j].groupID, t[j].id) };
                    r.SetValue(Grid.ColumnProperty, j % cols);
                    r.SetValue(Grid.RowProperty, j / cols);
                    grid.Children.Add(r);
                }
                group.Children.Insert(0, grid);
            }
            HighlightButton();
        }
        /// <summary>
        /// Highlight selected day and today
        /// </summary>
        public void HighlightButton() {
            foreach (Button b in dayList) {
                b.BorderBrush = Brushes.Transparent;
            }
            if (selectedDay.Month == DateTime.Now.Month) {
                Button b = dayList[DateTime.Now.Day];
                b.BorderBrush = Brushes.White;
            }
            Button btn = dayList[selectedDay.Day];
            btn.BorderBrush = Brushes.Blue;
        }
        /// <summary>
        /// Reaction for DayButton click
        /// </summary>
        private void SelectDay_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            selectedDay = new DateTime(selectedDay.Year, selectedDay.Month, int.Parse(btn.Name.Substring(6)));
            HighlightButton();
            DayChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Reaction for previous month button
        /// </summary>
        private void PrevMonth_Click(object sender, RoutedEventArgs e) {
            monthOffset--;
            CreateMonth();
            MonthChanged?.Invoke(this, e);
        }
        /// <summary>
        /// Reaction for next month button
        /// </summary>
        private void NextMonth_Click(object sender, RoutedEventArgs e) {
            monthOffset++;
            CreateMonth();
            MonthChanged?.Invoke(this, e);
        }

    }
}
