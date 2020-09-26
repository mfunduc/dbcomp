use sqlx::{ Error };
use std::env;
use std::time::Instant;
mod info;
use self::info::dbinfo::DbInfo;
use futures::join;

#[async_std::main]
async fn main() -> Result<(), Error> {

    let args: Vec<String> = env::args().collect();
    if args.len() < 2 {
        println!("Please pass <dbPath1> <dbPath2>!");
        return Err(Error::Io(std::io::Error::from(std::io::ErrorKind::NotFound)));
    }

    let now = Instant::now();
    let mut db1: DbInfo = Default::default();
    let mut db2: DbInfo = Default::default();
    let (err1, err2) = join!(db1.load(&args[1]), db2.load(&args[2]));
    if !err1.is_ok() || !err2.is_ok() {
        println!("Error opening databases!");
        return Err(Error::Io(std::io::Error::from(std::io::ErrorKind::NotFound)));
    }

    if db1.compare(&db2).await? {
        println!("All tables match!");
    }
    println!("{} seconds elapsed", now.elapsed().as_micros() as f64 / 1e6);

    Ok(())
}
