import sys
import sqlite3
from tableinfo import TableInfo

class DbInfo(object):
    def __init__(self, name):
        self.name = name
        self.conn = sqlite3.connect(name)
        self.tables = {}
        self.conn.text_factory = lambda x: str(x, 'utf-8', 'ignore')
        cursor = self.conn.cursor()
        cursor.execute("SELECT name FROM sqlite_master WHERE type =\'table\' AND name NOT LIKE \'sqlite_%\'")
        rows = cursor.fetchall()
        if len(rows) > 0:
            tableNames = []
            for row in rows:
                tableNames.append(row[0])

            for tableName in tableNames:
                self.tables[tableName] = TableInfo(self.conn, tableName)

    def compareTables(self, tableName, numColumns, db):
        cursor = self.conn.cursor()
        sql = "SELECT * FROM '" + tableName + "'"
        cursor.execute(sql)
        rows = cursor.fetchall()
        if len(rows) > 0:
            cursor2 = db.conn.cursor()
            cursor2.execute(sql)

            for rowNum, row in enumerate(rows):
                row2 = cursor2.fetchone()
                if (row is None) or (row2 is None):
                    return False, rowNum
                for col in range(numColumns):
                    if row[col] != row2[col]:
                        return False, rowNum

            return True, 0

        return False, 0

    def compare(self, db):
        matches = True
        matched = []
        onlyOne = []
        onlyTwo = []
        for tableName, tableInfo in self.tables.items():
            tableInfo2 = db.tables.get(tableName)
            if tableInfo2 is not None:
                if tableInfo.compare(tableInfo2):
                    if tableInfo.numRows < 1000:
                        dataMatched, rowNum = self.compareTables(tableName, len(tableInfo.columns), db)
                        if not dataMatched:
                            matches = False
                            sys.stdout.write('Different Data for Table: {} in row {}\n'.format(tableName, rowNum + 1))
                        else:
                            matched.append(tableInfo)
                    else:
                        matched.append(tableInfo)
                else:
                    matches = False
                    sys.stdout.write('Different Table: {} {} {} but in {} {}\n'.format(tableName, self.name, tableInfo.toStr(False), 
                        db.name, tableInfo2.toStr(False)))
            else:
                matches = False
                onlyOne.append(tableName)

        for tableName, tableInfo in db.tables.items():
            if tableName not in self.tables:
                matches = False
                onlyTwo.append(tableName)

        if len(matched) > 0:
            sys.stdout.write("*************** {} matched tables ****************\n".format(len(matched)))
            for table in matched:
                sys.stdout.write("Table: {}\n".format(table.toStr(True)))

        if len(onlyOne) > 0:
            sys.stdout.write("*************** {} tables only in {} ****************\n".format(len(onlyOne), self.name))
            for table in onlyOne:
                sys.stdout.write("Table: {}\n".format(table))

        if len(onlyTwo) > 0:
            sys.stdout.write("*************** {} tables only in {} ****************\n".format(len(onlyTwo), db.name))
            for table in onlyTwo:
                sys.stdout.write("Table: {}\n".format(table))

        return matches
