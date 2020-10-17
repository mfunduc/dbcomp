package dbComp;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.AbstractMap;
import java.util.Iterator;
import java.util.List;
import java.util.Vector;


class TableInfo
{
    String name;
    int numRows = 0;
    List<AbstractMap.SimpleEntry<String, String>> columns = new Vector<AbstractMap.SimpleEntry<String, String>>();

    public TableInfo(Connection sqlconn, String name) throws SQLException
    {
        this.name = name;
        numRows = getNumRows(sqlconn);
        Statement stmt  = sqlconn.createStatement();
        String sql = "PRAGMA table_info('" + this.name + "')";
        ResultSet rs = stmt.executeQuery(sql);

        while (rs.next())
        {
            columns.add(new AbstractMap.SimpleEntry<String, String>(rs.getString(1), rs.getString(2)));
        }
    }
    public int numColumns() {
        return columns.size();
    }
    int getNumRows(Connection sqlconn) throws SQLException
    {
        Statement stmt  = sqlconn.createStatement();
        String sql = "SELECT COUNT(*) FROM '" + this.name + "'";
        ResultSet rs = stmt.executeQuery(sql);
        if (rs.next())
            return rs.getInt(1);
        return 0;
    }
    public boolean compare(TableInfo table)
    {
        if ((numRows == table.numRows) && (columns.size() == table.columns.size()))
        {
            Iterator<AbstractMap.SimpleEntry<String, String>> it = table.columns.iterator();
            for (AbstractMap.SimpleEntry<String, String> colInfo : columns)
            {
                if (!colInfo.equals(it.next()))
                    return false;
            }
            return true;
        }
        else
            return false;
    }
    public String getDescription()
    {
        return String.format("has %d rows and %d columns", numRows, columns.size());
    }
}
