# dbcomp

A little command line utility that can open 2 Sqlite databases and compare the tables inside.  
It lists tables in only one of the databases.  For tables whose names match, it then compares the number of rows and columns.  
If the number of rows and columns match and the table has less than 1000 rows, it then compares their data.  

I wrote this to get a feel for how different languages work and how fast they can do the same non-trivial task.  
Some features I wanted to test were:  
* Taking command line parameters and a simple validation (Except for C# .Net Core version which has a GUI)
* Using maps and arrays where needed and doing lookups and iteration
* Organizing the code into separate modules and objects where possible
* Using async/await where possible
* String formatting
  
* Some libraries had bugs which caused me to do things in an uglier way, but I found workarounds.
* The package manager configs are as simple as I could make them.

Feel free to add bug reports and comments!  

<img src=".\dbCompNet.png" alt=".Net Core 3.1" style="float: left; margin-right: 10px;" />

  
*Go*  
go run . ../TestData/SampleDb.s3db ../TestData/SampleDb1.s3db  
Different Table: Customers ../TestData/SampleDb.s3db has 6 rows and 18 columns but in ../TestData/SampleDb1.s3db has 5 rows and 18 columns  
Different Data for Table: Purchases in row 4  
*************** 4 matched tables ****************  
Table: CompositeProducts has 4 rows and 2 columns  
Table: Countries has 233 rows and 2 columns  
Table: Province_State has 82 rows and 3 columns  
Table: Products has 11 rows and 4 columns  
*************** 1 tables only in ../TestData/SampleDb.s3db ****************  
Table: Updates  
*************** 1 tables only in ../TestData/SampleDb1.s3db ****************  
Table: Extra  
2020/10/03 20:33:12 29.0017ms elapsed  
  
*Python*  
python.exe .\dbcomp.py ../TestData/SampleDb.s3db ../TestData/SampleDb1.s3db  
Different Table: Customers ../TestData/SampleDb.s3db has 6 rows and 18 columns but in ../TestData/SampleDb1.s3db has 5 rows and 18 columns  
Different Data for Table: Purchases in row 4  
*************** 4 matched tables ****************  
Table: CompositeProducts has 4 rows and 2 columns  
Table: Countries has 233 rows and 2 columns  
Table: Province_State has 82 rows and 3 columns  
Table: Products has 11 rows and 4 columns  
*************** 1 tables only in ../TestData/SampleDb.s3db ****************  
Table: Updates  
*************** 1 tables only in ../TestData/SampleDb1.s3db ****************  
Table: Extra  
0.040003 seconds elapsed  
  
*Rust*  
cargo run --release ../TestData/SampleDb.s3db ../TestData/SampleDb1.s3db  
    Finished release [optimized] target(s) in 0.24s  
     Running `target\release\dbcomp.exe SampleDb.s3db SampleDb1.s3db`
Different Data for Table: Purchases in row 4  
Different Table: Customers ../TestData/SampleDb.s3db has 6 rows and 18 columns but in ../TestData/SampleDb1.s3db has 5 rows and 18 columns  
*************** 4 matched tables ****************  
Table: Province_State has 82 rows and 3 columns  
Table: Products has 11 rows and 4 columns  
Table: Countries has 233 rows and 2 columns  
Table: CompositeProducts has 4 rows and 2 columns  
*************** 1 tables only in ../TestData/SampleDb.s3db ****************  
Table: Updates  
*************** 1 tables only in ../TestData/SampleDb1.s3db ****************  
Table: Extra  
0.050381 seconds elapsed  
  
*Java*  
mvn exec:java  
[INFO] Scanning for projects...  
[INFO]   
[INFO] ---------------------------< dbComp:dbComp >----------------------------  
[INFO] Building dbComp 1.0-SNAPSHOT  
[INFO] --------------------------------[ jar ]---------------------------------  
[INFO]   
[INFO] --- exec-maven-plugin:1.6.0:java (default-cli) @ dbComp ---  
Different Table: Customers ../TestData/SampleDb.s3db has 6 rows and 18 columns but in ../TestData/SampleDb1.s3db has 5 rows and 18 columns  
Different Data for Table: Purchases in row 4  
*************** 4 matched tables ****************  
Table: has 11 rows and 4 columns  
Table: has 233 rows and 2 columns  
Table: has 4 rows and 2 columns  
Table: has 82 rows and 3 columns  
*************** 1 tables only in ../TestData/SampleDb.s3db ****************  
Table: Updates  
*************** 1 tables only in ../TestData/SampleDb1.s3db ****************  
Table: Extra  
Elapsed time 0.285000  
  
*NodeJs*  
node ./dbcomp.mjs ../TestData/SampleDb.s3db ../TestData/SampleDb1.s3db  
(node:5328) ExperimentalWarning: The ESM module loader is experimental.  
0.010447 seconds elapsed  

Different Table: Customers ../TestData/SampleDb.s3db has 6 rows and 18 columns but in  ../TestData/SampleDb1.s3db has 5 rows and 18 columns  
Table: Purchases data is different in row 4  
*************** 4 matched tables ****************  
Table: CompositeProducts  
Table: Countries  
Table: Province_State  
Table: Products  
*************** 1 tables only in ../TestData/SampleDb.s3db ****************  
Table: Updates  
*************** 1 tables only in ../TestData/SampleDb1.s3db ****************  
Table: Extra  
