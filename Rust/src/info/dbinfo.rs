use sqlx::{ Connect, Error, Cursor, row::Row };
use sqlx::sqlite::SqliteConnection;
use std::collections::HashMap;
use crate::info::tableinfo::TableInfo;

pub struct DbInfo {
    name: String,
    tables: HashMap<String, TableInfo>,
}


impl DbInfo {
    pub async fn load(&mut self, path: &str) -> Result<(), Error> {
        self.name = path.to_string();
        let mut connection = SqliteConnection::connect(self.get_url()).await?;
        let mut rows = sqlx::query("SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%';")
                            .fetch(&mut connection);
        let mut table_names: Vec<String> = Vec::new();  // Load the table names first so we can re-use the connection
        while let Some(row) = rows.next().await? {

            let name: &str = row.get(0);
            table_names.push(name.to_string());
        } 
        // Now load each table info                       
        for table_name in table_names {
            let mut table: TableInfo = Default::default();
            table.load_from_db(&table_name, &mut connection).await?;
            self.tables.insert(table_name, table);    
        }
        Ok(())
    }

    fn get_url(&self) -> String {
        String::from("sqlite:") + &self.name
    }

    async fn compare_tables(&self, table_name: &str, num_columns: usize, db2: &DbInfo) -> Result<(bool, usize), Error> {
        let mut matches = true;
        let mut connection = SqliteConnection::connect(self.get_url()).await?;
        let mut connection2 = SqliteConnection::connect(db2.get_url()).await?;
        let sql = format!("SELECT * FROM {}", &table_name);
        let mut rows = sqlx::query(&sql).fetch(&mut connection);
        let mut rows2 = sqlx::query(&sql).fetch(&mut connection2);  
        let mut row_num: usize = 0;  
        while let Some(row) = rows.next().await? {
            if let Some(row2) = rows2.next().await? {
                for col in 0..num_columns {
                    let cell: Option<&str> = row.get(col);
                    let cell2: Option<&str> = row2.get(col);
                    if  cell != cell2 {
                        matches = false;
                        break;
                    }
                }
                if matches == false {
                    break;
                } else {
                    row_num += 1;
                }
            } else {
                matches = false;
                break;
            }
        }

        Ok((matches, row_num))
    }

    pub async fn compare(&self, db2: &DbInfo) -> Result<bool, Error> {
        let mut matches = true;

        let mut matched: Vec<&TableInfo> = Vec::new();
        let mut only_db1: Vec<&str> = Vec::new();
        let mut only_db2: Vec<&str> = Vec::new();
    
        for (table_name, table_info) in &self.tables {
            if let Some(table2) = db2.tables.get(table_name) {
                if !table_info.compare_tables(table2) {
                    println!("Different Table: {} {} {} but in {} {}", &table_name, &self.name, table_info.display_str(false), 
                            &db2.name, table2.display_str(false));
                    matches = false;
                } else {
                    if table_info.get_num_rows() < 1000 {
                        let (res, row_num) = self.compare_tables(&table_name, table_info.get_num_columns(), db2).await?;
                        if !res {
                            println!("Different Data for Table: {} in row {}", &table_name, row_num + 1);
                            matches = false;
                        } else {
                            matched.push(&table_info);
                        }
                    } else {
                        matched.push(&table_info);
                    }
                }
            } else {
                only_db1.push(&table_name);
                matches = false;
            }
        }
        for (table_name, _table_info) in &db2.tables {
            if !self.tables.contains_key(table_name) {
                only_db2.push(&table_name);
                matches = false;
            }
        }

        if matched.len() > 0 {
            println!("*************** {} matched tables ****************", matched.len());
            for table in matched {
                println!("Table: {} ", table.display_str(true));
            }
        }

        if only_db1.len() > 0 {
            println!("*************** {} tables only in {} ****************", only_db1.len(), &self.name);
            for table in only_db1 {
                println!("Table: {} ", table);
            }
        }

        if only_db2.len() > 0 {
            println!("*************** {} tables only in {} ****************", only_db2.len(), &db2.name);
            for table in only_db2 {
                println!("Table: {} ", table);
            }
        }

        Ok(matches)
    }
}

impl Default for DbInfo {
    fn default() -> Self {
        DbInfo {
            name: String::from(""),
            tables: HashMap::new(),
        }
    }
}
