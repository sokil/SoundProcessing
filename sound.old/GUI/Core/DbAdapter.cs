using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SQLite;


namespace Sound.Core
{
    class DbAdapter
    {
        private SQLiteConnection _connection;

        public SQLiteConnection getConnection()
        {
            if (_connection != null)
                return _connection;

            _connection = new SQLiteConnection("Data Source=" + Properties.Settings.Default.DBFilename + ";Version=3;");
            _connection.Open();

            return _connection;
        }

        
    }
}
