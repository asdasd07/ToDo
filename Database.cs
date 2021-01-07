using System;
using System.Data.SQLite;
using System.IO;

namespace ToDo {
    class Database {
        SQLiteConnection myConnection;
        /// <summary>
        /// Create connection with database file or create new database file
        /// </summary>
        public Database() {
            if (File.Exists("./ToDoBase.db")) {
                myConnection = new SQLiteConnection("Data Source=ToDoBase.db");
            } else {
                SQLiteConnection.CreateFile("ToDoBase.db");
                myConnection = new SQLiteConnection("Data Source=ToDoBase.db");
                OpenConnection();

                string query = "CREATE TABLE Groups (ID INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL)";
                SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
                myCommand.ExecuteNonQuery();

                query = "CREATE TABLE Tasks(ID INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT NOT NULL, Description TEXT, CreationDate NUMERIC, EndDate NUMERIC, Deadline NUMERIC, Complete NUMERIC, GroupID INTEGER, FOREIGN KEY(GroupID) REFERENCES Groups(ID))";
                myCommand = new SQLiteCommand(query, myConnection);
                myCommand.ExecuteNonQuery();
                CloseConnection();

                AddGroup("Developers");
                AddGroup("Graphics");
                AddGroup("Testers");
            }
        }
        /// <summary>
        /// Open database connection 
        /// </summary>
        public void OpenConnection() {
            if (myConnection.State != System.Data.ConnectionState.Open) {
                myConnection.Open();
            }
        }
        /// <summary>
        /// Close database connection 
        /// </summary>
        public void CloseConnection() {
            if (myConnection.State != System.Data.ConnectionState.Closed) {
                myConnection.Close();
            }
        }
        /// <summary>
        /// Add a new record to Groups table
        /// </summary>
        /// <param name="GroupName">New group name</param>
        public void AddGroup(string GroupName) {
            OpenConnection();
            string query = "INSERT INTO Groups('name') VALUES(@name)";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            myCommand.Parameters.AddWithValue("@name", GroupName);
            myCommand.ExecuteNonQuery();
            CloseConnection();
        }
        /// <summary>
        /// Get Groups ID and Groups Name from database
        /// </summary>
        /// <returns>Result of select</returns>
        public SQLiteDataReader SelectGroups() {
            string query = "SELECT * FROM groups";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            OpenConnection();
            SQLiteDataReader result = myCommand.ExecuteReader();
            return result;
        }
        /// <summary>
        /// Check is Group Name already exists in database
        /// </summary>
        /// <param name="name">Group Name</param>
        /// <returns>return true if exists</returns>
        public bool GroupExists(string name) {
            string query = "SELECT * FROM groups where name=@Gname";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            myCommand.Parameters.AddWithValue("@Gname", name);
            OpenConnection();
            SQLiteDataReader result = myCommand.ExecuteReader();
            bool res = result.HasRows;
            CloseConnection();
            return res;
        }
        /// <summary>
        /// Add a new record to Tasks table
        /// </summary>
        /// <param name="title">Title of task</param>
        /// <param name="deadline">Deadline of task</param>
        /// <param name="groupID">DroupID of task</param>
        /// <param name="description">Description of task</param>
        public void AddTask(string title, DateTime deadline, int groupID, string description = "") {
            DateTime now = DateTime.Now;
            OpenConnection();
            string query = "INSERT INTO Tasks('title','description','creationDate','deadline','complete','groupID') VALUES(@title, @description, @creationDate, @deadline, @complete, @groupID)";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            myCommand.Parameters.AddWithValue("@title", title);
            myCommand.Parameters.AddWithValue("@description", description);
            myCommand.Parameters.AddWithValue("@creationDate", now);
            myCommand.Parameters.AddWithValue("@deadline", deadline);
            myCommand.Parameters.AddWithValue("@complete", false);
            myCommand.Parameters.AddWithValue("@groupID", groupID);
            myCommand.ExecuteNonQuery();
            CloseConnection();
        }
        /// <summary>
        /// Mark Task as done or not
        /// </summary>
        /// <param name="id">ID of task</param>
        /// <param name="done">is task done</param>
        public void CompleteTask(int id, bool done = true) {
            DateTime now = DateTime.Now;
            OpenConnection();
            string query = "UPDATE Tasks SET complete = @done, endDate = @end WHERE (ID = @ID)";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            myCommand.Parameters.AddWithValue("@end", now);
            myCommand.Parameters.AddWithValue("@ID", id);
            myCommand.Parameters.AddWithValue("@done", done);
            myCommand.ExecuteNonQuery();
            CloseConnection();
        }
        /// <summary>
        /// Update Task record
        /// </summary>
        /// <param name="id">ID of task</param>
        /// <param name="title">new title of task</param>
        /// <param name="deadline">new deadline of task</param>
        /// <param name="done">new done mark of task</param>
        /// <param name="groupID">new groupID of task</param>
        /// <param name="description">new description of task</param>
        public void UpdateTask(int id, string title, DateTime deadline, bool done, int groupID, string description = "") {
            OpenConnection();
            string query = "UPDATE Tasks  SET title = @title, description = @description, deadline = @deadline, complete = @complete, groupID = @groupID WHERE (ID = @ID)";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            myCommand.Parameters.AddWithValue("@title", title);
            myCommand.Parameters.AddWithValue("@description", description);
            myCommand.Parameters.AddWithValue("@deadline", deadline);
            myCommand.Parameters.AddWithValue("@complete", done);
            myCommand.Parameters.AddWithValue("@groupID", groupID);
            myCommand.Parameters.AddWithValue("@ID", id);
            myCommand.ExecuteNonQuery();
            CloseConnection();
        }
        /// <summary>
        /// Delete Task record. If it was the last member of group, group also will be deleted
        /// </summary>
        /// <param name="id">ID of task</param>
        /// <param name="Gid">GroupID of task</param>
        public void DeleteTask(int id, int? Gid = null) {
            OpenConnection();
            string query = "delete from Tasks WHERE (ID = @ID)";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            myCommand.Parameters.AddWithValue("@ID", id);
            myCommand.ExecuteNonQuery();
            if (Gid != null) {
                query = "select * from Tasks WHERE (groupid = @Gid)";
                myCommand = new SQLiteCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@Gid", Gid);
                SQLiteDataReader result = myCommand.ExecuteReader();
                if (!result.HasRows) {
                    query = "delete from groups WHERE (id = @Gid)";
                    myCommand = new SQLiteCommand(query, myConnection);
                    myCommand.Parameters.AddWithValue("@Gid", Gid);
                    myCommand.ExecuteNonQuery();
                }
            }
            CloseConnection();
        }
        /// <summary>
        /// Get filtered info from database
        /// </summary>
        /// <param name="GID">Group ID filter. 0 - any group</param>
        /// <param name="day">Day filter. null - any day</param>
        /// <returns>Result of select</returns>
        public SQLiteDataReader Select(int GID = 0, DateTime? day = null) {
            string query = "SELECT tasks.id,Title,Description,CreationDate,EndDate,Deadline,Complete,GroupID,name FROM tasks, groups WHERE GroupID=groups.id";
            if (GID != 0) query += " and groupID=@Gid";
            if (day != null) query += " and Deadline BETWEEN date(@Day) and date(@Day,'+1 day')";
            query += " order by Deadline";
            SQLiteCommand myCommand = new SQLiteCommand(query, myConnection);
            if (GID != 0) myCommand.Parameters.AddWithValue("@Gid", GID);
            if (day != null) myCommand.Parameters.AddWithValue("@Day", day);
            OpenConnection();
            SQLiteDataReader result = myCommand.ExecuteReader();
            return result;
        }
    }
}
