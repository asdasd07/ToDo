using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToDo {
    /// <summary>
    /// Logika interakcji dla klasy CustomButton.xaml
    /// </summary>
    public partial class CustomButton : UserControl {
        public int id, groupID;
        public string title = "", description = "", group = "";
        public DateTime creation, end, deadline;
        public bool complete;

        public CustomButton() {
            InitializeComponent();
        }
        public CustomButton(System.Data.SQLite.SQLiteDataReader data) {
            InitializeComponent();
            id = int.Parse(data["id"].ToString());
            groupID = int.Parse(data["groupid"].ToString());
            title = data["title"].ToString();
            description = data["description"].ToString();
            group = data["name"].ToString();
            creation = data.GetDateTime(data.GetOrdinal("CreationDate"));
            if (data["endDate"].ToString() != "") {
                end = data.GetDateTime(data.GetOrdinal("endDate"));
            }
            deadline = data.GetDateTime(data.GetOrdinal("deadline"));
            complete = data.GetBoolean(data.GetOrdinal("Complete"));
            L1.Content = title;
            L2.Content = deadline.ToString("dddd");
            L3.Content = deadline.ToString("dd MMMM yyyy");
            L4.Content = group;
            tex.Text = description;
            if (complete) {
                Background = new SolidColorBrush(Colors.Gray) { Opacity = 0.5 };
            } else {
                Background = Tools.GetTaskBrush(groupID, id);
            }
        }
    }
}
