using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ToDo {
    class CustomButton : UserControl2 {
        public int id, groupID;
        public string title="", description="", group="";
        public DateTime creation, end, deadline;
        public bool complete;
        public CustomButton() {
        }
        public CustomButton(System.Data.SQLite.SQLiteDataReader data) {
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
                Background= new SolidColorBrush(Colors.Gray) { Opacity = 0.5 };
            }
        }

    }
}
