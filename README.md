# dbcomp

A little command line utility that can open 2 Sqlite databases and compare the tables inside.
It lists tables in only one of the databases.  For tables whose names match, it then compares the number of rows and columns.
If the number of rows and columns match and the table has less than 1000 rows, it then compares their data.
