using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SQLite;

namespace Sound.Core
{
    abstract class Table : DataTable
    {
        public DbAdapter getAdaper()
        {
            return Sound.Core.ServiceLocator.getDbAdapter();
        }

        protected DataRowCollection _fetchAll(string query)
        {
            SQLiteCommand command = new SQLiteCommand(getAdaper().getConnection());
            command.CommandText = query;

            try
            {
                SQLiteDataReader reader = command.ExecuteReader();
                this.Load(reader);
                reader.Close();
            }
            catch
            {
                return null;
            }

            return this.Rows;
        }
    }
}
