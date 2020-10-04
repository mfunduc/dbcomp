package main

import (
	"fmt"
	"log"

	"github.com/jmoiron/sqlx"
	_ "github.com/mattn/go-sqlite3"
)

type TableInfo struct {
	name    string
	numRows int
	columns map[string]string
}

func LoadColumns(conn *sqlx.DB, name string) *map[string]string {
	columns := make(map[string]string)
	rows, err := conn.Queryx("PRAGMA table_info('" + name + "')")
	if err != nil {
		return &columns
	}

	results := make(map[string]interface{})
	for rows.Next() {
		errRow := rows.MapScan(results)
		if errRow == nil {
			colName, _ := results["name"].(string)
			colType, _ := results["type"].(string)
			columns[colName] = colType
		} else {
			log.Printf("%v while loading columns for table: %v", err, name)
		}
	}
	return &columns
}

func MakeTableInfo(conn *sqlx.DB, name string) *TableInfo {
	rows, err := conn.Queryx("SELECT COUNT(*) FROM '" + name + "'")
	var numRows int
	if rows.Next() {
		err = rows.Scan(&numRows)
		if err != nil {
			return nil
		}
	}
	info := TableInfo{name, numRows, *LoadColumns(conn, name)}
	return &info
}

func Compare(table1 *TableInfo, table2 *TableInfo) bool {
	if (table1.numRows == table2.numRows) && (len(table1.columns) == len(table2.columns)) {
		for colName, colType := range table1.columns {
			if table2.columns[colName] != colType {
				return false
			}
		}
		return true
	}
	return false
}

func ToStr(table *TableInfo, includeName bool) string {
	if includeName {
		return fmt.Sprintf("%s has %d rows and %d columns", table.name, table.numRows, len(table.columns))
	}
	return fmt.Sprintf("has %d rows and %d columns", table.numRows, len(table.columns))
}
