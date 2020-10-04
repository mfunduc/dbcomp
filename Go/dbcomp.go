package main

import (
	"fmt"
	"log"
	"os"
	"time"

	_ "github.com/mattn/go-sqlite3"
)

func main() {

	if len(os.Args) < 3 {
		fmt.Println("Please pass <dbPath1> <dbPath2>!")
		return
	}
	start := time.Now()

	var db1 *DbInfo = MakeDbInfo(os.Args[1])
	if db1 == nil {
		fmt.Printf("Could not open %s!\n", os.Args[1])
		return
	}
	var db2 *DbInfo = MakeDbInfo(os.Args[2])
	if db2 == nil {
		fmt.Printf("Could not open %s!\n", os.Args[2])
		return
	}
	if DbCompare(db1, db2) {
		fmt.Println("All tables match!")
	}

	elapsed := time.Since(start)
	log.Printf("%s elapsed", elapsed)
}
