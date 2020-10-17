package dbComp;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.AbstractMap;
import java.util.HashMap;
import java.util.List;
import java.util.Objects;
import java.util.Vector;
import java.util.Map.Entry;

class DbInfo
{
    String name;
    HashMap<String, TableInfo> tables = new HashMap<String, TableInfo>();
    Connection sqlconn;
    public DbInfo(String path) throws SQLException
    {
        this.name = path;
        this.sqlconn = DriverManager.getConnection("jdbc:sqlite:" + path);

        Statement stmt  = sqlconn.createStatement();
        String sql = "SELECT name FROM sqlite_master WHERE type =\'table\' AND name NOT LIKE \'sqlite_%\'";
        ResultSet rs = stmt.executeQuery(sql);

        List<String> tableNames = new Vector<String>();
        while (rs.next())
        {
            tableNames.add(rs.getString(1));
        }
        for (String tableName : tableNames)
        {
            tables.put(tableName, new TableInfo(sqlconn, tableName));
        }
    }
    AbstractMap.SimpleEntry<Boolean, Integer> compareTables(String tableName, int numColumns, DbInfo db)
            throws SQLException
    {
        String sql = "SELECT * FROM '" + tableName + "'";
        Statement stmt  = sqlconn.createStatement();
        Statement stmt2  = db.sqlconn.createStatement();
        ResultSet rs = stmt.executeQuery(sql);
        ResultSet rs2 = stmt2.executeQuery(sql);

        int nRow = 0;
        while (rs.next())
        {
            rs2.next();
            for (int nCol = 0; nCol < numColumns; ++nCol)
            {
                if (!Objects.equals(rs.getObject(nCol + 1), rs2.getObject(nCol + 1)))
                    return new AbstractMap.SimpleEntry<Boolean, Integer>(false, nRow);
            }
            ++nRow;
        }
        return new AbstractMap.SimpleEntry<Boolean, Integer>(true, 0);
    }
    public boolean compare(DbInfo db) throws SQLException
    {
        List<TableInfo> matched = new Vector<TableInfo>();
        List<String> onlyDb1 = new Vector<String>();
        List<String> onlyDb2 = new Vector<String>();
        boolean matches = true;

        for (Entry<String, TableInfo> entry : tables.entrySet()) {
            TableInfo tableInfo = db.tables.get(entry.getKey());
            if (tableInfo != null) {
                if (!entry.getValue().compare(tableInfo)) {
                    System.out.printf("Different Table: %s %s %s but in %s %s\n", entry.getKey(), name, entry.getValue().getDescription(), 
                            db.name, tableInfo.getDescription());
                    matches = false;
                } else {
                    if (tableInfo.numRows < 1000) {
                        AbstractMap.SimpleEntry<Boolean, Integer> res = compareTables(entry.getKey(), tableInfo.numColumns(), db);
                        if (!res.getKey()) {
                            System.out.printf("Different Data for Table: %s in row %d\n", entry.getKey(), res.getValue() + 1);
                            matches = false;
                        } else {
                            matched.add(entry.getValue());
                        }
                    } else {
                        matched.add(entry.getValue());
                    }
                }
            } else {
                onlyDb1.add(entry.getKey());
                matches = false;
            }
        }
        for (Entry<String, TableInfo> entry : db.tables.entrySet()) {
            if (!tables.containsKey(entry.getKey())) {
                onlyDb2.add(entry.getKey());
                matches = false;
            }
        }

        if (matched.size() > 0) {
            System.out.printf("*************** %d matched tables ****************\n", matched.size());
            for (TableInfo table : matched) {
                System.out.printf("Table: %s\n", table.getDescription());
            }
        }

        if (onlyDb1.size() > 0) {
            System.out.printf("*************** %d tables only in %s ****************\n", onlyDb1.size(), name);
            for (String table : onlyDb1) {
                System.out.printf("Table: %s\n", table);
            }
        }

        if (onlyDb2.size() > 0) {
            System.out.printf("*************** %d tables only in %s ****************\n", onlyDb2.size(), db.name);
            for (String table : onlyDb2) {
                System.out.printf("Table: %s\n", table);
            }
        }

        return matches;
    }
}
