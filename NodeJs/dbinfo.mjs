import {TableInfo} from './tableinfo.mjs'

class DbInfo {
    constructor(name) {
        this.name = name
        this.tables = new Map()
    }
    load = async (db) => {
        await db.each(`SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%'`, [], (err, row) => {
            if (err) {
                throw err;
            }
            this.tables.set(row.name, new TableInfo(row.name));
        })
        for (let tableInfo of this.tables.values()) {
            await tableInfo.load(db)
        }
    }

    compareTables = async (tableInfo, db1, db2) => {
        const sql = "SELECT * FROM '" + tableInfo.name + "'"
        let rows = []
        await db1.each(sql, [], (err, row) => {
            if (err) {
                throw err;
            }
            rows.push(row)
        })

        let rows2 = []
        await db2.each(sql, [], (err, row) => {
            if (err) {
                throw err;
            }
            rows2.push(row)
        })

        for (let rowNum = 0; rowNum < rows.length; ++rowNum) {
            for (let colName of tableInfo.columns.keys()) {
                if (rows[rowNum][colName] !== rows2[rowNum][colName]) {
                    return rowNum
                }
            }
        }

        return -1
    }

    compare = async (dbInfo, db1, db2) => {
        let same = true
        const matched = []
        const onlyOne = []
        const onlyTwo = []
        for (let [tableName, tableInfo] of this.tables.entries()) {
            const tableInfo2 = dbInfo.tables.get(tableName)
            if (tableInfo2) {
                if (tableInfo.compare(tableInfo2)) {
                    if (tableInfo.numRows < 1000) {
                        const rowNum = await this.compareTables(tableInfo, db1, db2)
                        if (rowNum === -1) {
                            matched.push(tableInfo)
                        } else {
                            console.log(`Table: ${tableName} data is different in row ${rowNum + 1}`)
                            same = false
                        }
                    } else {
                        matched.push(tableInfo)
                    }
                } else {
                    const desc1 = tableInfo.toStr()
                    const desc2 = tableInfo2.toStr()
                    console.log(`Different Table: ${tableName} ${this.name} ${desc1} but in  ${dbInfo.name} ${desc2}`)
                    same = false
                }
            } else {
                onlyOne.push(tableName)
                same = false
            }
        }
        dbInfo.tables.forEach((tableInfo, tableName) => {
            if (!this.tables.get(tableName))
                onlyTwo.push(tableName)
                same = false
        });

        if (matched.length > 0) {
            console.log(`*************** ${matched.length} matched tables ****************`)
            matched.forEach((tableInfo) => {
                console.log(`Table: ${tableInfo.name}`)
            });
        }

        if (onlyOne.length > 0) {
            console.log(`*************** ${onlyOne.length} tables only in ${this.name} ****************`)
            onlyOne.forEach((tableName) => {
                console.log(`Table: ${tableName}`)
            });
        }

        if (onlyTwo.length > 0) {
            console.log(`*************** ${onlyTwo.length} tables only in ${dbInfo.name} ****************`)
            onlyTwo.forEach((tableName) => {
                console.log(`Table: ${tableName}`)
            });
        }

        return same
    }
}

export {DbInfo}