use sqlx::sqlite::SqliteConnection;
use sqlx::{ Error, Cursor, row::Row };
use std::collections::HashMap;

pub struct TableInfo {
    name: String,
    num_rows: i32,
    columns: HashMap<String, String>
}

impl TableInfo {
    async fn read_rows(&mut self, connection: &mut SqliteConnection) -> Result<(), Error> {
        let sql = format!(r"SELECT COUNT(*) FROM '{}'", &self.name);
        let mut rows = sqlx::query(&sql).fetch(connection);

        if let Some(row) = rows.next().await? {
            self.num_rows = row.get(0);
        }                        
        Ok(())
    }
    async fn read_columns(&mut self, connection: &mut SqliteConnection) -> Result<(), Error> {
        let sql = format!(r"PRAGMA table_info('{}')", &self.name);
        let mut rows = sqlx::query(&sql).fetch(connection);

        while let Some(row) = rows.next().await? {
            self.columns.insert(row.get(1), row.get(2));
        }                        
        Ok(())
    }
    pub async fn load_from_db(&mut self, table_name: &str, connection: &mut SqliteConnection) -> Result<(), Error> {
        self.name = String::from(table_name);
        self.read_rows(connection).await?;               
        self.read_columns(connection).await?;               
        Ok(())
    }
    pub fn compare_tables(&self, table_to_check: &TableInfo) -> bool {
        if self.num_rows != table_to_check.num_rows {
            return false;
        }
        if self.columns.len() == table_to_check.columns.len() {
            for (col_name, col_type) in &self.columns {
                let col_type2 = table_to_check.columns.get(col_name);
                if col_type2.is_none() || (col_type2 != Some(col_type)) {
                    return false;
                }
            }
        } else {
            return false;
        }
        true
    }
    pub fn display_str(&self, include_name: bool) -> String {
        if include_name {
            return format!("{} has {} rows and {} columns", self.name, self.num_rows, self.columns.len());
        } else {
            return format!("has {} rows and {} columns", self.num_rows, self.columns.len());
        }
    }
    pub fn get_num_rows(&self) -> i32 { self.num_rows }
    pub fn get_num_columns(&self) -> usize { self.columns.len() }
}

impl Default for TableInfo {
    fn default() -> Self {
        TableInfo {
            num_rows: 0,
            name: String::from(""),
            columns: HashMap::new()
        }
    }
}
