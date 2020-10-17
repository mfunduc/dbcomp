class TableInfo {
    constructor(name) {
        this.numRows = 0
        this.name = name
        this.columns = new Map()
    }

    load = async (db) => {
        await db.each("SELECT COUNT(*) rowcount FROM '" + this.name + "'", [], (err, row) => {
            if (err) {
                throw err;
            }
            this.numRows = row.rowcount
        })

        await db.each("PRAGMA table_info('" + this.name + "')", [], (err, row) => {
            if (err) {
                throw err;
            }
            this.columns.set(row.name, row.type)
        })
    }    

    compare = (table) => {
        if ((this.numRows === table.numRows) && (this.columns.size === table.columns.size)) {
            this.columns.forEach((type, name) => {
                if (table.columns.get(name) !== type)
                    return false
            })
            return true
        }
        return false
    }

    toStr = () => {
        return `has ${this.numRows} rows and ${this.columns.size} columns`
    }
}

export {TableInfo}