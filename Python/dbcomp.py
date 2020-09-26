import sys
import datetime
from dbinfo import DbInfo

def main():
    if len(sys.argv) < 3:
        sys.stderr.write("Please pass <dbPath1> <dbPath2>!\n")
        sys.exit(1)

    StartTime = datetime.datetime.now()

    db = DbInfo(sys.argv[1])
    db2 = DbInfo(sys.argv[2])
    if db.compare(db2):
        sys.stdout.write("All tables match!")

    EndTime = datetime.datetime.now()
    TimeDuration = EndTime - StartTime
    sys.stderr.write(str(TimeDuration.total_seconds())+" seconds elapsed\n")

if __name__ == "__main__":
    main()

# EOF

