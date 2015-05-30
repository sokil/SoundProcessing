using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using Sound.Core;

namespace Sound.Model.WallComposition
{
    class Materials : Table
    {
        protected string _table = "materials";

        public DataRowCollection getAll()
        {
            string query = "SELECT ROWID, name, density, k, fb FROM " + _table;

            return _fetchAll(query);
        }
    }
}
