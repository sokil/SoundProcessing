using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using Sound.Core;

namespace Sound.Model.WallComposition
{
    /// <summary>
    ///     <link>http://www.mpal.ru/products/partition/document64.shtml</link>
    /// </summary>
    class Covering : Table
    {
        protected string _table = "covering";

        protected DataRowCollection _materials;

        protected SortedList<int, SortedList<double, double>> _absorbCoefs = new SortedList<int, SortedList<double, double>>();

        public DataRowCollection getAll()
        {
            if (_materials != null)
                return _materials;

            string query = "SELECT ROWID, * FROM " + _table;

            _materials = _fetchAll(query);

            return _materials;
        }

        public SortedList<double,double> getRow(int id)
        {
            if (_absorbCoefs.Count != 0 && _absorbCoefs.ContainsKey(id))
                return _absorbCoefs[id];

            getAll();

            // get fields
            Dictionary<string, double> columnNames = new Dictionary<string, double>();
            foreach(DataColumn c in _materials[0].Table.Columns)
            {
                if (!c.ColumnName.Contains("freq_"))
                    continue;

                columnNames[c.ColumnName] = Convert.ToDouble(c.ColumnName.Substring(5));
            }

            // get data
            int rowid;
            foreach (DataRow row in _materials)
            {
                rowid = Convert.ToInt32(row["ROWID"]);

                _absorbCoefs[rowid] = new SortedList<double,double>();
                foreach(KeyValuePair<string, double> columnName in columnNames)
                {
                    _absorbCoefs[rowid][columnName.Value] = (double) row[columnName.Key];
                }
            }

            return _absorbCoefs[id];
        }
    }
}
