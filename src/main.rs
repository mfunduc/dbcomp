use sqlx::{ Error };
use std::env;
mod info;
use self::info::dbinfo::DbInfo;

#[async_std::main]
async fn main() -> Result<(), Error> {

    let args: Vec<String> = env::args().collect();
    if args.len() < 2 {
        println!("Please pass <dbPath1> <dbPath2>!");
        return Err(Error::Io(std::io::Error::from(std::io::ErrorKind::NotFound)));
    }

    let mut db1: DbInfo = Default::default();
    db1.load(&args[1]).await?;

    let mut db2: DbInfo = Default::default();
    db2.load(&args[2]).await?;

    if db1.compare(&db2).await? {
        println!("All tables match!");
    }

    Ok(())
}
