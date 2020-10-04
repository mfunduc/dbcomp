# dbcomp

A little command line utility that can open 2 Sqlite databases and compare the tables inside.  
It lists tables in only one of the databases.  For tables whose names match, it then compares the number of rows and columns.  
If the number of rows and columns match and the table has less than 1000 rows, it then compares their data.  

go run . ..\SampleDb.s3db ..\SampleDb1.s3db
Different Table: Customers ..\SampleDb.s3db has 6 rows and 18 columns but in ..\SampleDb1.s3db has 5 rows and 18 columns
Different Data for Table: Purchases in row 4
*************** 4 matched tables ****************
Table: CompositeProducts has 4 rows and 2 columns
Table: Countries has 233 rows and 2 columns
Table: Province_State has 82 rows and 3 columns
Table: Products has 11 rows and 4 columns
*************** 1 tables only in ..\SampleDb.s3db ****************
Table: Updates
*************** 1 tables only in ..\SampleDb1.s3db ****************
Table: Extra
2020/10/03 20:33:12 29.0017ms elapsed

python.exe .\dbcomp.py ..\SampleDb.s3db ..\SampleDb1.s3db
Different Table: Customers ..\SampleDb.s3db has 6 rows and 18 columns but in ..\SampleDb1.s3db has 5 rows and 18 columns
Different Data for Table: Purchases in row 4
*************** 4 matched tables ****************
Table: CompositeProducts has 4 rows and 2 columns
Table: Countries has 233 rows and 2 columns
Table: Province_State has 82 rows and 3 columns
Table: Products has 11 rows and 4 columns
*************** 1 tables only in ..\SampleDb.s3db ****************
Table: Updates
*************** 1 tables only in ..\SampleDb1.s3db ****************
Table: Extra
0.040003 seconds elapsed

cargo run --release ..\SampleDb.s3db ..\SampleDb1.s3db
    Finished release [optimized] target(s) in 0.24s
     Running `target\release\dbcomp.exe SampleDb.s3db SampleDb1.s3db`
Different Data for Table: Purchases in row 4
Different Table: Customers ..\SampleDb.s3db has 6 rows and 18 columns but in ..\SampleDb1.s3db has 5 rows and 18 columns
*************** 4 matched tables ****************
Table: Province_State has 82 rows and 3 columns
Table: Products has 11 rows and 4 columns
Table: Countries has 233 rows and 2 columns
Table: CompositeProducts has 4 rows and 2 columns
*************** 1 tables only in ..\SampleDb.s3db ****************
Table: Updates
*************** 1 tables only in ..\SampleDb1.s3db ****************
Table: Extra
0.050381 seconds elapsed