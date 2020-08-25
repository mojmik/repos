using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovickaSQL {
    class DbException : Exception {
        public DbException(string mess) : base(mess) {
            
        }
    }
    class DbManager {
        SqlConnection dbCon;
        public string GetConnectionString() {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
            csb.DataSource = @"(LocalDB)\MSSQLLocalDB";
            csb.InitialCatalog = @"J:\MIK\PRG\REPOS\SLOVICKASQL\SLOVICKADB.MDF";
            csb.IntegratedSecurity = true;
            string pripojovaciRetezec = csb.ConnectionString;
            return pripojovaciRetezec;
        }
        public void OpenConnection() {
            string connectionString = GetConnectionString();
            //connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = J:\mik\prg\repos\SlovickaSQL\SlovickaDB.mdf; Integrated Security = True";
            dbCon = new SqlConnection(connectionString);
            dbCon.Open();
        }
        public void CloseConnection() {
            dbCon.Close();
            dbCon = null;
        }
        public int DoInsert(string tableName,string[] values,string[] colNames) {
            if (dbCon == null) throw new DbException("neni spojeni voe");
            string colStringNames = string.Join(", ", colNames);
            for (int n=0;n<colNames.Length;n++) {
                colNames[n] = "@"+colNames[n];
            }
            string colString = string.Join(", ", colNames);

            string dotaz = $"INSERT INTO {tableName} ({colStringNames}) VALUES ({colString})";            
            using (SqlCommand sqlDotaz = new SqlCommand(dotaz, dbCon)) {
                for (int n=0;n<colNames.Length;n++) {
                    sqlDotaz.Parameters.AddWithValue(colNames[n], values[n]);
                }
                int radku = sqlDotaz.ExecuteNonQuery();
                return radku;
            }            
        }
        public void GetRowsSdatasetem() {
            string connectionString = GetConnectionString();
            using (SqlConnection spojeni = new SqlConnection(connectionString)) {
                spojeni.Open();

                string dotaz = "SELECT * FROM Word";
                using (SqlDataAdapter adapter = new SqlDataAdapter(dotaz, spojeni))
                using (System.Data.DataSet vysledky = new System.Data.DataSet()) {
                    adapter.Fill(vysledky);

                    foreach (System.Data.DataRow radek in vysledky.Tables[0].Rows) {
                        Console.WriteLine("Id: " + radek[0] + ", česky: " + radek["Czech"] + ", anglicky: " + radek["English"]);
                    }
                }
                spojeni.Close();
                Console.ReadKey();
            }
        }
        public List<List<string>> GetRows(string table) {
            if (dbCon == null) throw new DbException("neni spojeni voe");
            SqlCommand prikaz = new SqlCommand($"SELECT * FROM {table}", dbCon);
            SqlDataReader dataReader = prikaz.ExecuteReader();
            List<List<string>> tab=new List<List<string>>();
            List<string> radek;
            
            while (dataReader.Read()) // dokud neprojdeme všechny záznamy
            {
                radek = new List<string>();
                for (int n=0;n<dataReader.FieldCount;n++) {
                    radek.Add(dataReader[n].ToString());
                }
                tab.Add(radek);
            }
            dataReader.Close();
            return tab;
        }
        public void PrintRows(string table) {
            List<List<string>> tab = GetRows(table);
            foreach (List<string> row in tab) {
                foreach(string s in row) {
                    Console.Write(s + " ");
                }
                Console.WriteLine();
            }
        }
        public void DoDelete(string tableName, string idCol, string idVal) {
            
            string dotaz = $"DELETE FROM {tableName} WHERE {idCol}=@{idCol}";
            if (dbCon == null) throw new DbException("neni spojeni voe");
            using (SqlCommand sqlDotaz = new SqlCommand(dotaz, dbCon)) {
                sqlDotaz.Parameters.AddWithValue($"@{idCol}", idVal);
                int radku = sqlDotaz.ExecuteNonQuery();
                Console.WriteLine(radku);
            }
        }
        public void DoUpdate(string tableName, string id, string idCol, string value, string valueCol) {            
            string dotaz = $"UPDATE {tableName} SET {valueCol}=@{valueCol} WHERE {idCol}=@{idCol}";
            using (SqlCommand sqlDotaz = new SqlCommand(dotaz, dbCon)) {
                sqlDotaz.Parameters.AddWithValue($"@{idCol}", id);
                sqlDotaz.Parameters.AddWithValue($"@{valueCol}", value);
                int radku = sqlDotaz.ExecuteNonQuery();
                Console.WriteLine(radku);
            }
        }
        public void TestConnection() {
            //string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SlovickaDB;Integrated Security=True";
            string connectionString = GetConnectionString();
            //string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=J:\MIK\PRG\REPOS\SLOVICKASQL\SLOVICKADB.MDF;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            //Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = J:\MIK\PRG\REPOS\SLOVICKASQL\SLOVICKADB.MDF; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False
            //string connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = J:\mik\prg\repos\SlovickaSQL\SlovickaDB.mdf; Integrated Security = True";
            using (SqlConnection pripojeni = new SqlConnection(connectionString)) {
                pripojeni.Open();
                Console.WriteLine("Aplikace se úspěšně připojila k databázi.");

                //exectute scalar
                SqlCommand prikaz = new SqlCommand();
                prikaz.Connection = pripojeni;
                prikaz.CommandText = "SELECT COUNT(*) FROM Word";
                int pocetSlovicek = (int)prikaz.ExecuteScalar();  // metoda vrací typ object - je potřeba převést na int                
                Console.WriteLine("Počet slovíček v DB je {0}", pocetSlovicek);

                //executereader example
                prikaz = new SqlCommand("SELECT Id, Czech, English FROM Word", pripojeni);                
                SqlDataReader dataReader = prikaz.ExecuteReader();
                while (dataReader.Read()) // dokud neprojdeme všechny záznamy
                {
                    Console.WriteLine("{0} {1} {2}",
                        dataReader[0],                      // index sloupce (Id)
                        dataReader["Czech"],                // název sloupce
                        dataReader.GetString(2));           // index sloupce (English) s převedením na požadovaný datový typ
                }
                dataReader.Close();

                //prepared statements, predavani dotazu
                Console.WriteLine("Zadej anglické slovíčko k překladu");
                string slovo = Console.ReadLine();

                prikaz = new SqlCommand("SELECT Czech FROM Word WHERE English=@slovo", pripojeni);
                prikaz.Parameters.AddWithValue("@slovo", slovo);

                dataReader = prikaz.ExecuteReader();
                while (dataReader.Read()) // dokud neprojdeme vsechny zaznamy
                {
                    Console.WriteLine("Překlad: {0}", dataReader["Czech"]);
                }
                dataReader.Close();


                pripojeni.Close();
            }
            Console.ReadKey();
        }
        
    }
}
