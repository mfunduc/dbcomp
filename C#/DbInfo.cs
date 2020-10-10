using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace dbComp
{
    class DbInfo
    {
        String name;
        Dictionary<String, TableInfo> tables = new Dictionary<String, TableInfo>();
        DbConnection sqlconn;
        public DbInfo(String path)
        {
            this.name = path;
            this.sqlconn = new SqliteConnection(String.Format("Data Source={0}", path));
            sqlconn.Open();

            var command = sqlconn.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type =\'table\' AND name NOT LIKE \'sqlite_%\'";
            using var reader = command.ExecuteReader();

            List<string> tableNames = new List<string>();
            while (reader.Read())
            {
                tableNames.Add(reader.GetString(0));
            }
            foreach (string tableName in tableNames)
            {
                tables.Add(tableName, new TableInfo(sqlconn, tableName));
            }
       }
        Tuple<bool, int> CompareTables(String tableName, DbInfo db)
        {
            var command = sqlconn.CreateCommand();
            command.CommandText = "SELECT * FROM '" + tableName + "'";
            using var reader = command.ExecuteReader();
            var command2 = db.sqlconn.CreateCommand();
            command2.CommandText = "SELECT * FROM '" + tableName + "'";
            using var reader2 = command2.ExecuteReader();

            Object[] values = new Object[reader.FieldCount];
            Object[] values2 = new Object[reader2.FieldCount];
            int nRow = 0;
            while (reader.Read())
            {
                reader2.Read();
                reader.GetValues(values);
                reader2.GetValues(values2);
                for (int nCol = 0; nCol < reader.FieldCount; ++nCol)
                {
                    if (!values[nCol].Equals(values2[nCol]))
                        return new Tuple<bool, int>(false, nRow);
                }
                ++nRow;
            }
            return new Tuple<bool, int>(true, 0);
        }
        public bool Compare(DbInfo db, ref List<TableInfo> tablesIn1, ref List<TableInfo> tablesIn2,
                        ref List<Tuple<TableInfo, TableInfo>> tablesMatched, ref List<Tuple<TableInfo, TableInfo, int>> tablesUnmatched)
        {
            foreach (var table in tables)
            {
                if (db.tables.ContainsKey(table.Key))
                {
                    var tableInfo = db.tables[table.Key];
                    if (table.Value.Compare(tableInfo))
                    {
                        if (tableInfo.NumRows < 1000)
                        {
                            var result = CompareTables(table.Key, db);
                            if (result.Item1)
                                tablesMatched.Add(new Tuple<TableInfo, TableInfo>(table.Value, tableInfo));
                            else
                                tablesUnmatched.Add(new Tuple<TableInfo, TableInfo, int>(table.Value, tableInfo, result.Item2));
                        }
                    }
                    else
                        tablesUnmatched.Add(new Tuple<TableInfo, TableInfo, int>(table.Value, tableInfo, -1));
                }
                else
                    tablesIn1.Add(table.Value);
            }
            foreach (var table in db.tables)
            {
                if (!tables.ContainsKey(table.Key))
                    tablesIn2.Add(table.Value);
            }
            return (tablesIn1.Count == 0) && (tablesIn2.Count == 0) && (tablesUnmatched.Count == 0);
        }
    }
}
