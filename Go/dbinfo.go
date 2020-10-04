package main

import (
	"fmt"

	"github.com/jmoiron/sqlx"
	_ "github.com/mattn/go-sqlite3"
)

type DbInfo struct {
	name   string
	conn   *sqlx.DB
	tables map[string]TableInfo
}

func getTableNames(conn *sqlx.DB) *[]string {
	var tableNames []string
	rows, err := conn.Queryx("SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%'")
	for rows.Next() {
		var tableName string
		err = rows.Scan(&tableName)
		if err == nil {
			tableNames = append(tableNames, tableName)
		}
	}
	return &tableNames
}

func loadTables(conn *sqlx.DB) *map[string]TableInfo {
	tables := make(map[string]TableInfo)
	tableNames := getTableNames(conn)
	for _, tableName := range *tableNames {
		tables[tableName] = *MakeTableInfo(conn, tableName)
	}
	return &tables
}

func MakeDbInfo(name string) *DbInfo {
	var db *sqlx.DB
	var err error
	db, err = sqlx.Open("sqlite3", name)
	if err != nil {
		return nil
	}
	info := DbInfo{name, db, *loadTables(db)}
	return &info
}

func compareTables(db1 *DbInfo, tableName string, columns *map[string]string, db2 *DbInfo) (bool, int) {
	sql := "SELECT * FROM '" + tableName + "'"
	rows, err := db1.conn.Queryx(sql)
	rows2, err2 := db2.conn.Queryx(sql)
	if (err != nil) || (err2 != nil) {
		return false, 0
	}
	results1 := make(map[string]interface{})
	results2 := make(map[string]interface{})
	rowNum := 0
	for rows.Next() {
		rows2.Next()
		errRow := rows.MapScan(results1)
		errRow2 := rows2.MapScan(results2)
		if (errRow != nil) || (errRow2 != nil) {
			return false, 0
		}
		for col, data := range results1 {
			data2 := results2[col]
			if data != data2 {
				return false, rowNum
			}
		}
		rowNum++
	}
	return true, 0
}

func DbCompare(db1 *DbInfo, db2 *DbInfo) bool {
	matches := true
	var matched []TableInfo
	var onlyOne []string
	var onlyTwo []string
	for tableName, tableInfo := range db1.tables {
		tableInfo2, found := db2.tables[tableName]
		if found {
			if Compare(&tableInfo, &tableInfo2) {
				if tableInfo.numRows < 1000 {
					dataMatched, rowNum := compareTables(db1, tableName, &tableInfo.columns, db2)
					if dataMatched {
						matched = append(matched, tableInfo)
					} else {
						matches = false
						fmt.Printf("Different Data for Table: %s in row %d\n", tableName, rowNum+1)
					}
				} else {
					matched = append(matched, tableInfo)
				}
			} else {
				matches = false
				fmt.Printf("Different Table: %s %s %s but in %s %s\n", tableName, db1.name, ToStr(&tableInfo, false),
					db2.name, ToStr(&tableInfo2, false))
			}
		} else {
			matches = false
			onlyOne = append(onlyOne, tableName)
		}
	}

	for tableName, _ := range db2.tables {
		_, found := db1.tables[tableName]
		if !found {
			matches = false
			onlyTwo = append(onlyTwo, tableName)
		}
	}

	if len(matched) > 0 {
		fmt.Printf("*************** %d matched tables ****************\n", len(matched))
		for _, tableInfo := range matched {
			fmt.Printf("Table: %s\n", ToStr(&tableInfo, true))
		}
	}
	if len(onlyOne) > 0 {
		fmt.Printf("*************** %d tables only in %s ****************\n", len(onlyOne), db1.name)
		for _, table := range onlyOne {
			fmt.Printf("Table: %s\n", table)
		}
	}
	if len(onlyTwo) > 0 {
		fmt.Printf("*************** %d tables only in %s ****************\n", len(onlyTwo), db2.name)
		for _, table := range onlyTwo {
			fmt.Printf("Table: %s\n", table)
		}
	}
	return matches
}
