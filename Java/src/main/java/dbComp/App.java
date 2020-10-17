package dbComp;

public class App 
{
    public static void main(String[] arg) {
        if (arg.length < 2) {
            System.out.println("Please pass <dbPath1> <dbPath2>!");
        } else {
            long start;
            try {
                Class.forName("org.sqlite.JDBC");
                start = System.currentTimeMillis();
                DbInfo db1 = new DbInfo(arg[0]);
                DbInfo db2 = new DbInfo(arg[1]);
                if (db1.compare(db2)) {
                    System.out.println("All tables match!");
                }
                long end = System.currentTimeMillis();
                System.out.printf("Elapsed time %f\n", (double)(end - start) / 1000.0);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }    
}
