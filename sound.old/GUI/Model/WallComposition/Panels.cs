using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using Sound.Core;

namespace Sound.Model.WallComposition
{
    class Panels : Table
    {
        protected string _table = "panels";

        public DataRowCollection getAll()
        {
            string query = "SELECT ROWID, name, density, fb, fc, rb, rc, cl FROM " + _table;

            return _fetchAll(query);
        }
    }
}
