using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace dbComp
{
    class TableInfo
    {
        String name;
        int numRows = 0;
        List<Tuple<string, string>> columns = new List<Tuple<string, string>>();

        public int NumRows
        {
            get { return numRows; }
        }
        public String Name
        {
            get { return name; }
        }
        public List<Tuple<string, string>> Columns
        {
            get { return columns;  }
        }
        public TableInfo(DbConnection sqlconn, String name)
        {
            this.name = name;
            numRows = getNumRows(sqlconn);
            var command = sqlconn.CreateCommand();
            command.CommandText = "PRAGMA table_info('" + this.name + "')";
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                columns.Add(new Tuple<string, string>(reader.GetString(1), reader.GetString(2)));
            }
        }
        int getNumRows(DbConnection sqlconn)
        {
            var command = sqlconn.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM '" + this.name + "'";
            using var reader = command.ExecuteReader();
            if (reader.Read())
                return reader.GetInt32(0);
            return 0;
        }
        public bool Compare(TableInfo table)
        {
            if ((numRows == table.numRows) && (columns.Count == table.columns.Count))
            {
                var it = table.columns.GetEnumerator();
                foreach (var colInfo in columns)
                {
                    it.MoveNext();
                    if (!colInfo.Equals(it.Current))
                        return false;
                }
                return true;
            }
            else
                return false;
        }
        public string GetDescription()
        {
            return String.Format("has {0} rows and {1} columns", numRows, columns.Count);
        }
    }
}
