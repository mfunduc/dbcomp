
class TableInfo(object):
    def __init__(self, conn, name):
        self.name = name
        self.numRows = 0
        self.columns = []
        self.getNumRows(conn)
        self.loadColumns(conn)

    def loadColumns(self, conn):
        cursor = conn.cursor()
        cursor.execute("PRAGMA table_info('" + self.name + "')")
        rows = cursor.fetchall()
        if len(rows) > 0:
            for row in rows:
                self.columns.append((row[0], row[1]))

    def getNumRows(self, conn):
        cursor = conn.cursor()
        cursor.execute("SELECT COUNT(*) FROM '" + self.name + "'")
        row = cursor.fetchone()
        if not row is None:
            self.numRows = row[0]

    def compare(self, table):
        if (self.numRows == table.numRows) and (len(self.columns) == len(table.columns)):
            for index, colInfo in enumerate(self.columns):
                if (colInfo[0] != table.columns[index][0]) or (colInfo[1] != table.columns[index][1]):
                    return False
            return True
        return False

    def toStr(self, includeName):
        if includeName:
            return "{} has {} rows and {} columns".format(self.name, self.numRows, len(self.columns))
        return "has {} rows and {} columns".format(self.numRows, len(self.columns))
