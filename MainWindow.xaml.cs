using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ToDo {
    /// <summary>
    /// Interaction logic for the class MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Database dataObject;
        Dictionary<int, string> groups = new Dictionary<int, string>();
        CustomButton Selected;
        DateTime? DayFilter;
        bool DayFilterOn = false;
        int filter = 0;

        /// <summary>
        /// MainWindow constructor
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            dataObject = new Database();
            Refresh_Click();
        }
        /// <summary>
        /// Refresh whole window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender = null, RoutedEventArgs e = null) {
            RefreshGroups();
            RefreshTask();
            grid2.Visibility = Visibility.Hidden;
            shgrid.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Get fresh info about Tasks from database and put them into ListBox
        /// </summary>
        public void RefreshTask() {
            dataObject.OpenConnection();
            LiBox.Items.Clear();
            if (DayFilterOn) {
                DayFilter = Calendar.SelectedDate;
            } else {
                DayFilter = null;
            }

            System.Data.SQLite.SQLiteDataReader data = dataObject.Select(filter, DayFilter);

            if (data.HasRows) {
                while (data.Read()) {
                    CustomButton b = new CustomButton(data);
                    LiBox.Items.Add(b);
                }
            }
            dataObject.CloseConnection();
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
            Selected = (CustomButton)so.SelectedItem;
            if (Selected == null) {
                return;
            }
            EnterDetails(Selected);
        }
        /// <summary>
        /// Enter task details into tab
        /// </summary>
        /// <param name="CB">Selected CustomButton. null - create empty</param>
        /// <param name="edit">View edit mode</param>
        void EnterDetails(CustomButton CB = null, bool edit = false) {
            if (edit) {
                grid2.Visibility = Visibility.Visible;
                shgrid.Visibility = Visibility.Hidden;
                if (CB == null) {
                    CreateButton.Visibility = Visibility.Visible;
                    SubmitButton.Visibility = Visibility.Hidden;
                    CTitle.Text = "";
                    CDesc.Document.Blocks.Clear();
                    Combo.SelectedValue = "";
                    Deadline.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    done.IsChecked = false;
                } else {
                    CreateButton.Visibility = Visibility.Hidden;
                    SubmitButton.Visibility = Visibility.Visible;
                    CTitle.Text = CB.title;
                    CDesc.Document.Blocks.Clear();
                    CDesc.Document.Blocks.Add(new Paragraph(new Run(CB.description)));
                    Combo.SelectedValue = CB.group;
                    Deadline.SelectedDate = CB.deadline;
                    done.IsChecked = CB.complete;
                }
            } else {
                grid2.Visibility = Visibility.Hidden;
                shgrid.Visibility = Visibility.Visible;
                shTitle.Content = CB.title;
                shdone.IsChecked = CB.complete;
                shDesc.Text = CB.description;
                shDeadline.Content = CB.deadline.ToString("dddd, dd MMMM yyyy");
                shgroup.Content = CB.group;
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
            Refresh_Click();
        }
        /// <summary>
        /// Reaction for Cancel button
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            grid2.Visibility = Visibility.Hidden;
            shgrid.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Reaction for Add button
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e) {
            shgrid.Visibility = Visibility.Hidden;
            grid2.Visibility = Visibility.Visible;
            done.Visibility = Visibility.Hidden;
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
            shgrid.Visibility = Visibility.Hidden;
            grid2.Visibility = Visibility.Visible;
            done.Visibility = Visibility.Visible;
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
            if (Deadline.SelectedDate == null) {
                MessageBox.Show("Deadline must be chosen");
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
            var str = new TextRange(CDesc.Document.ContentStart, CDesc.Document.ContentEnd);
            dataObject.AddTask(CTitle.Text, Deadline.SelectedDate.Value, Gid, str.Text);
            Refresh_Click();
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
                RefreshGroups();
            }
            int Gid = groups.First(x => x.Value == g).Key;
            var str = new TextRange(CDesc.Document.ContentStart, CDesc.Document.ContentEnd);
            dataObject.UpdateTask(Selected.id, CTitle.Text, Deadline.SelectedDate.Value, done.IsChecked.Value, Gid, str.Text); ;
            Refresh_Click();
        }
        /// <summary>
        /// Reaction for changing filter group in ComboBox
        /// </summary>
        private void Com_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string g = Com.SelectedItem.ToString();
            filter = groups.FirstOrDefault(x => x.Value == g).Key;
            Refresh_Click();
        }

        /// <summary>
        /// Reaction for click on Done CheckBox in view mode
        /// </summary>
        private void Shdone_Click(object sender, RoutedEventArgs e) {
            if (Selected == null) {
                return;
            }
            dataObject.CompleteTask(Selected.id, shdone.IsChecked.Value);
            Refresh_Click();
        }

        /// <summary>
        /// Reaction for click on FilterByDay CheckBox
        /// </summary>
        private void DayFilter_Click(object sender, RoutedEventArgs e) {
            DayFilterOn = DayCheckbox.IsChecked.Value;
            if (DayFilterOn) {
                Calendar.Visibility = Visibility.Visible;
                Calendar.SelectedDate = DateTime.Now.Date;
            } else {
                Calendar.Visibility = Visibility.Collapsed;
            }
            Refresh_Click();
        }
    }
}
