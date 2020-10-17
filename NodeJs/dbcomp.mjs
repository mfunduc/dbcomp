import sqlite3 from 'sqlite3'
import { open } from 'sqlite'
import { exit } from 'process'
import { DbInfo } from './dbinfo.mjs'

var args = process.argv.slice(2)
if (args.length < 2) {
    console.log("Please pass <dbPath1> <dbPath2>!");
    exit(1)
}

const startTime = process.hrtime();

(async () => {
  const [db1, db2] = await Promise.all([
    open({
      filename: args[0],
      driver: sqlite3.Database
    }),
    open({
      filename: args[1],
      driver: sqlite3.Database
    }),
  ])

  const dbInfo1 = new DbInfo(args[0])
  const dbInfo2 = new DbInfo(args[1])
  await dbInfo1.load(db1)
  await dbInfo2.load(db2)

  if (dbInfo1.compare(dbInfo2, db1, db2) === true) {
    console.log("Databases match!")
  }

  const hrtime = process.hrtime(startTime)
  var seconds = (hrtime[0] + (hrtime[1] / 1e9)).toFixed(6);
  console.log(seconds + " seconds elapsed\n")
})()
