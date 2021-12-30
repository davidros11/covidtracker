using MySql.Data.MySqlClient;
using System.Data;
using MySql.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
namespace CovidTracker
{
    public class DataAccess : DataBaseAccess
    {
        private readonly Dictionary<string, string> AlternateCountryName = new Dictionary<string, string> {
            ["Russian Federation"] = "Russia",
            ["Congo"] = "Congo (Brazzaville)",
            ["DR Congo"] = "Congo (Kinshasa)",
            ["Democratic Republic of the Congo"] = "Congo (Kinshasa)",
            ["Congo, Democratic Republic of"] = "Congo (Kinshasa)",
            ["Burkina"] = "Burkina Faso",
            ["Burma (Myanmar)"] = "Burma",
            ["Myanmar"] = "Burma",
            ["Cabo Verde"] = "Cape Verde",
            ["Cote d'Ivoire"] = "Ivory Coast",
            ["CZ"] = "Czech Republic",
            ["Czechia"] = "Czech Republic",
            ["Eswatini"] = "Swaziland",
            ["Holy See"] = "Vatican City",
            ["Macedonia"] = "North Macedonia",
            ["Timor-Leste"] = "East Timor",
            ["West Bank and Gaza"] = "Israel",
            ["Mainland China"] = "China",
            ["Korea, South"] = "South Korea",
            ["UK"] = "United Kingdom",
            ["Republic of Ireland"] = "Ireland",
            ["St. Martin"] = "Saint Martin",
            ["Iran (Islamic Republic of)"] = "Iran",
            ["Republic of Korea"] = "South Korea",
            ["Hong Kong SAR"] = "Hong Kong",
            ["Taipei and environs"] = "Taiwan",
            ["Viet Nam"] = "Vietnam",
            ["Republic of Moldova"] = "Moldova",
            ["Republic of the Congo"] = "Congo (Brazzaville)",
            ["Gambia, The"] = "Gambia",
            ["Bahamas, The"] = "Bahamas",
            ["The Bahamas"] = "Bahamas",
            ["United States of America"] = "United States",
            ["Bruinei Drusalam"] = "Bruinei",
            ["Saint BarthÃ©lemy"] = "Saint Barthelemy",
            ["RÃ©union"] = "Reunion",
            ["The Gambia"] = "Gambia",
            ["United Republic of Tanzania"] = "Tanzania",
            ["Syrain Arab Republic"] = "Syria",
            ["Venezuela (Bolivarian Republic of)"] = "Venezuela",
            ["Saint Martin (French part)"] = "Saint Martin",
            ["Micronesia (Fed. States of)"] = "Micronesia",
            ["Dem. People's Republic of Korea"] = "North Korea",
            ["CÃ´te d'Ivoire"] = "Ivory Coast",
            ["CuraÃ§ao"] = "Curacao",
            ["China, Hong Kong SAR"] = "Hong Kong",
            ["China, Taiwan Province of China"] = "Taiwan",
            ["China, Macao SAR"] = "Macau",
            ["Lao People's Democratic Republic"] = "Laos",
            ["US"] = "United States",
            ["Democratic Republic of Congo"] = "Congo (Kinshasa)",
            ["Faeroe Islands"] = "Faroe Islands",
            ["Bolivia (Plurinational State of)"] = "Bolivia",
            ["Brunei Darussalam"] = "Brunei",
            ["Syrian Arab Republic"] = "Syria",
            ["Vatican"] = "Vatican City"
        };
        private static readonly Dictionary<string, string> LongQueries;

        static DataAccess()
        {
            LongQueries = GetQueries();
        }

        private static Dictionary<string, string> GetQueries()
        {
            string file = File.ReadAllText(Path.Combine("Model", "Utils", "Queries.sql"));
            var dict = new Dictionary<string, string>();
            var queries = file.Split("---");
            for(int i = 1; i < queries.Length-1; i += 2)
            {
                dict.Add(queries[i], queries[i+1]);
            }
            return dict;
        }
        public List<Country> GetCountries()
        {
            string query = "SELECT * FROM countries";
            Func<MySqlDataReader, List<Country>> converter = (MySqlDataReader reader) => {
                List<Country> countries = new List<Country>();
                while(reader.Read())
                {
                    var country = new Country
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Continent = reader.GetString(2)
                    };
                    countries.Add(country);
                }
                return countries;
            };
            return Query(query, converter);
        }
        public List<CountryDateData> GetDateData(DateTime date, int threshold)
        {
            string query = LongQueries["CountriesByDate"];
            MySqlParameter[] parameters = { 
                CreateParameter("@DATE", MySqlDbType.Date, date),
                CreateParameter("@TH", MySqlDbType.Int32, threshold)
            };
            Func<MySqlDataReader, List<CountryDateData>> converter = (MySqlDataReader reader) => {
                List<CountryDateData> lst = new List<CountryDateData>();
                while(reader.Read())
                {

                    var data = new CountryDateData
                    {
                        CountryID = reader.GetInt32(0),
                        CountryName = reader.GetString(1),
                        ConfirmedCases = reader.GetInt32(2),
                        NewCases = reader.GetInt32(3),
                        Recovered = (int?) OrNull(reader.GetValue(4)),
                        Deaths = reader.GetInt32(5),
                        Population = reader.GetInt32(6),
                        Vaccinated = reader.GetInt32(7),
                        FullyVaccinated = reader.GetInt32(8),
                        Boosters = reader.GetInt32(9),
                    };
                    lst.Add(data);
                }
                return lst;
            };
            return Query(query, parameters,converter);
        }
        public List<ContinentDateData> GetContinentDateData(DateTime date, int threshold)
        {
            string query = LongQueries["ContinentsByDate"];
            MySqlParameter[] parameters = { 
                CreateParameter("@DATE", MySqlDbType.Date, date),
                CreateParameter("@TH", MySqlDbType.Int32, threshold)
            };
            Func<MySqlDataReader, List<ContinentDateData>> converter = (MySqlDataReader reader) => {
                var lst = new List<ContinentDateData>();
                while(reader.Read())
                {

                    var data = new ContinentDateData
                    {
                        Continent = reader.GetString(0),
                        ConfirmedCases = reader.GetInt64(1),
                        NewCases = reader.GetInt32(2),
                        Recovered = reader.GetInt64(3),
                        Deaths = reader.GetInt32(4),
                        Population = reader.GetInt64(5),
                        Vaccinated = reader.GetInt64(6),
                        FullyVaccinated = reader.GetInt64(7),
                        Boosters = reader.GetInt32(8)
                    };
                    lst.Add(data);
                }
                return lst;
            };
            return Query(query, parameters,converter);
        }
        public WorldDateData GetWorldData(DateTime date, int threshold)
        {
            string query = LongQueries["WorldByDate"];
            MySqlParameter[] parameters = { 
                CreateParameter("@DATE", MySqlDbType.Date, date),
                CreateParameter("@TH", MySqlDbType.Int32, threshold)
            };
            Func<MySqlDataReader, WorldDateData> converter = (MySqlDataReader reader) => {
                if(reader.Read())
                {
                    var data = new WorldDateData
                    {
                        ConfirmedCases = reader.GetInt32(0),
                        NewCases = reader.GetInt32(1),
                        Recovered = reader.GetInt32(2),
                        Deaths = reader.GetInt32(3),
                        Population = reader.GetInt64(4),
                        Vaccinated = reader.GetInt64(5),
                        FullyVaccinated = reader.GetInt64(6),
                        Boosters = reader.GetInt32(7),
                    };
                    return data;
                }
                return null;
            };
            return Query(query, parameters,converter);
        }
        private List<AreaVaccineData> GetAreaVaccineData(MySqlDataReader reader) {
            var lst = new List<AreaVaccineData>();
            while(reader.Read())
            {
                var data = new AreaVaccineData
                {
                    Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                    Vaccinated = reader.GetInt32(1),
                    FullyVaccinated = reader.GetInt32(2),
                    Boosters = reader.GetInt32(3)
                };
                lst.Add(data);
            }
            return lst;
        }
        private List<AreaDiseaseData> AreaDiseaseData(MySqlDataReader reader)  {
            var lst = new List<AreaDiseaseData>();
            //int i = 0;
            while(reader.Read())
            {
                // i++;
                // if (i % 2 == 0)
                // {
                //     continue;
                // }
                var data = new AreaDiseaseData
                {
                    Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                    Confirmed = reader.GetInt32(1),
                    Deaths = reader.GetInt32(2),
                };
                var a = OrNull(reader.GetValue(3));
                data.Recovered = a is Decimal ? Decimal.ToInt32((Decimal) a) : (int?) a;
                lst.Add(data);
            }
            return lst;
        }
        public CountryData GetCountryData(int countryID, DateTime start, DateTime end)
        {
            string sql = "SELECT name, continent FROM countries WHERE id=@ID";
            MySqlParameter[] parameters = { 
                CreateParameter("@ID", MySqlDbType.Int32, countryID),
            };
            Func<MySqlDataReader, CountryData> converter = (MySqlDataReader reader) => {
                if(reader.Read())
                {
                    var data = new CountryData
                    {
                        Name = reader.GetString(0),
                        Continent = reader.GetString(1)
                    };
                    return data;
                }
                return null;
            };
            CountryData data = Query(sql, parameters,converter);
            if(data is null) { return null; }
            sql = "SELECT date, confirmed, deaths, recovered FROM disease_reports WHERE country_id = @ID AND date BETWEEN @START AND @END";
            parameters = new MySqlParameter[] { 
                CreateParameter("@ID", MySqlDbType.Int32, countryID),
                CreateParameter("@START", MySqlDbType.Date, start),
                CreateParameter("@END", MySqlDbType.Date, end)
            };
            data.DiseaseData = Query(sql, parameters, AreaDiseaseData);
            sql = "SELECT date, vaccinated, fully_vaccinated, number_of_boosters FROM vaccine_reports WHERE country_id = @ID AND date BETWEEN @START AND @END";
            parameters = CopyParams(parameters);
            data.VaccineData = Query(sql, parameters, GetAreaVaccineData);
             sql = @"SELECT year, population, density, median_age, poverty_rate, diabetes_rate FROM population_reports WHERE country_id = @ID
                        AND year BETWEEN YEAR(@START) AND YEAR(@END) ORDER BY year";
            parameters = CopyParams(parameters);
            Func<MySqlDataReader, Dictionary<string, PopulationData>> GetPopulationData = (MySqlDataReader reader) => {
                var dict = new Dictionary<string, PopulationData>();
                while(reader.Read())
                {
                    string year = reader.GetValue(0).ToString();
                    var data = new PopulationData
                    {
                        
                        Population = reader.GetInt32(1),
                        Density = reader.GetFloat(2),
                        MedianAge = (float?) OrNull(reader.GetValue(3)),
                        PovertyRate = (float?) OrNull(reader.GetValue(4)),
                        DiabetesRate = (float?) OrNull(reader.GetValue(5)),
                    };
                    dict[year] = data;
                }
                return dict;
            };
            data.PData = Query(sql, parameters, GetPopulationData);
            return data;
        }
        
        public Dictionary<string, DemographicData> GetPopulationData(int year)
        {
            string sql = "SELECT country_id, density, median_age, poverty_rate, diabetes_rate FROM population_reports WHERE year=@YEAR";
            MySqlParameter[] parameters = { CreateParameter("@YEAR", MySqlDbType.Int32, year) };
            Func<MySqlDataReader, Dictionary<string, DemographicData>> GetPopulationData = (MySqlDataReader reader) => {
                var dict = new Dictionary<string, DemographicData>();
                while(reader.Read())
                {
                    string key = reader.GetValue(0).ToString();
                    var data = new DemographicData
                    {
                        Density = reader.GetFloat(1),
                        MedianAge = (float?) OrNull(reader.GetValue(2)),
                        PovertyRate = (float?) OrNull(reader.GetValue(3)),
                        DiabetesRate = (float?) OrNull(reader.GetValue(4)),
                    };
                    dict.Add(key, data);
                }
                return dict;
            };
            return Query(sql, parameters, GetPopulationData);
        }
        private List<AreaVaccineData> MacroVaccineData(MySqlDataReader reader)
        {
            int numOfParts = 2;
            var lst = new List<AreaVaccineData>();
            var dict = new Dictionary<int, AreaVaccineData>();
            int lastPart = -1;
            DateTime currentDate = DateTime.MinValue;
            while(reader.Read())
            {
                int id = reader.GetInt32(0);
                int part = reader.GetInt32(1);
                int month = reader.GetInt32(2);
                int year = reader.GetInt32(3);
                if(lastPart != -1 && part != lastPart)
                {
                    var data = new AreaVaccineData {
                        Date = $"{currentDate.Year}-{currentDate.Month}-{currentDate.Day}",
                        Vaccinated = dict.Values.Sum((data) => data.Vaccinated),
                        FullyVaccinated = dict.Values.Sum((data) => data.FullyVaccinated),
                        Boosters = dict.Values.Sum((data) => data.Boosters)
                    };
                    lst.Add(data);
                }
                lastPart = part;
                int day = DateTime.DaysInMonth(year, month)*part/numOfParts;
                currentDate = new DateTime(year, month, day);
                dict[id] = new AreaVaccineData {
                    Vaccinated = reader.GetInt64(4),
                    FullyVaccinated = reader.GetInt64(5),
                    Boosters = reader.GetInt32(6)
                };
            }
            return lst;
        }
        public Dictionary<string, long> GetPopulationData(MySqlDataReader reader)
        {
            var dict = new Dictionary<string, long>();
            while(reader.Read())
            {
                dict[reader.GetValue(0).ToString()] = reader.GetInt64(1);
            }
            return dict;
        }
        public AggregatedData GetContinentData(string name, DateTime start, DateTime end)
        {
            var data = new AggregatedData();
            MySqlParameter[] parameters = {
                CreateParameter("@START", MySqlDbType.Date, start),
                CreateParameter("@END", MySqlDbType.Date, end),
                CreateParameter("@CONT", MySqlDbType.VarChar, name)
            };
            string sql = LongQueries["ContinentDiseaseData"];
            data.DiseaseData = Query(sql, parameters, AreaDiseaseData);
            sql = LongQueries["ContinentVaccineData"];
            parameters = CopyParams(parameters);
            data.VaccineData = Query(sql, parameters, MacroVaccineData);
            sql = LongQueries["ContinentPopulation"];
            parameters = new MySqlParameter[] {
                CreateParameter("@START", MySqlDbType.Int32, start.Year),
                CreateParameter("@END", MySqlDbType.Int32, end.Year),
                CreateParameter("@CONT", MySqlDbType.VarChar, name)
            };
            data.PopulationsByYear  = Query(sql, parameters, GetPopulationData);
            return data;
        }
        public AggregatedData GetWorldData(DateTime start, DateTime end)
        {
            var data = new AggregatedData();
            MySqlParameter[] parameters = {
                CreateParameter("@START", MySqlDbType.Date, start),
                CreateParameter("@END", MySqlDbType.Date, end)
            };
            string sql = LongQueries["WorldDiseaseData"];
            data.DiseaseData = Query(sql, parameters, AreaDiseaseData);
            sql = LongQueries["WorldVaccineData"];
            parameters = CopyParams(parameters);
            data.VaccineData = Query(sql, parameters, MacroVaccineData);
            parameters = CopyParams(parameters);
            parameters = new MySqlParameter[] {
                CreateParameter("@START", MySqlDbType.Int32, start.Year),
                CreateParameter("@END", MySqlDbType.Int32, end.Year)
            };
            sql = LongQueries["WorldPopulation"];
            data.PopulationsByYear  = Query(sql, parameters, GetPopulationData);
            return data;
        }
        public static void BulkInsertCovid(List<CSVCovidData> data, MySqlConnection cnn)
        {
            StringBuilder sCommand = new StringBuilder($"INSERT INTO temp_disease_reports VALUES ");           
            List<string> Rows = new List<string>();
            foreach (var row in data)
            {
                Object recovered = row.Recovered is null ? "NULL" : row.Recovered;
                Rows.Add(string.Format("('{0}',{1},{2},{3})",
                row.CountryName, row.ConfirmedCases, row.Deaths, recovered));
            }
            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(";");
            using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), cnn))
            {
                myCmd.CommandType = CommandType.Text;
                myCmd.ExecuteNonQuery();
            }
        }
        public static void BulkInsertVac(List<CSVVaccineData> data, MySqlConnection cnn)
        {
            StringBuilder sCommand = new StringBuilder($"INSERT INTO temp_vaccine_reports VALUES ");           
            List<string> Rows = new List<string>();
            foreach (var row in data)
            {
                Rows.Add(string.Format("('{0}','{1}',{2},{3},{4})",
                row.CountryName, row.Date.ToString("yyyy-MM-dd"), row.Vaccinated, row.FullyVaccinated, row.Boosters));
            }
            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(";");
            using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), cnn))
            {
                myCmd.CommandType = CommandType.Text;
                myCmd.ExecuteNonQuery();
            }
        }
        public void InsertCovidReports(DateTime date, Stream fileStream)
        {
            var sw = new Stopwatch();
            sw.Start();
            var dict = new Dictionary<string, CSVCovidData>();
            using (TextFieldParser parser = new TextFieldParser(fileStream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                var headers = parser.ReadFields();
                var columnNames = headers.Select((str, index) => new { str = str, index = index })
                                .ToDictionary(x => x.str, x => x.index);
                string countryRegion = "Country_Region";
                if(!columnNames.ContainsKey(countryRegion))
                {
                    countryRegion = "Country/Region";
                }
                while(!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    string countryName = fields[columnNames[countryRegion]];
                    if(AlternateCountryName.ContainsKey(countryName))
                    {
                        countryName = AlternateCountryName[countryName];
                    }
                    int confirmed = Int32.Parse(fields[columnNames["Confirmed"]]);
                    int deaths = Int32.Parse(fields[columnNames["Deaths"]]);
                    string recoveredStr = fields[columnNames["Recovered"]];
                    int? recovered = recoveredStr == "" ? null : Int32.Parse(recoveredStr);
                    if(dict.ContainsKey(countryName))
                    {
                        dict[countryName].ConfirmedCases += confirmed;
                        dict[countryName].Deaths += deaths;
                        dict[countryName].Recovered = dict[countryName].Recovered is null ?
                            recovered : dict[countryName].Recovered + recovered;
                    }
                    else
                    {
                        dict[countryName] = new CSVCovidData {
                            CountryName = countryName,
                            ConfirmedCases = confirmed,
                            Deaths = deaths,
                            Recovered = recovered
                        };
                    }
                }
            }        
            Debug.WriteLine("Dataset processing " + sw.Elapsed);
            using(MySqlConnection cnn  = new MySqlConnection(ConnectionString))
            {
                cnn.Open();
                using(MySqlTransaction transaction = cnn.BeginTransaction())
                {   
                    try
                    {
                        string sql = LongQueries["TempDiseaseTable"];
                        using (MySqlCommand command = new MySqlCommand(sql, cnn))
                        command.ExecuteNonQuery();
                        Debug.WriteLine("temp table  " + sw.Elapsed);
                        BulkInsertCovid(dict.Values.ToList(), cnn);
                        Debug.WriteLine("add to temp table "  + sw.Elapsed);
                        sql = LongQueries["AddDiseaseReports"];
                        using (MySqlCommand command = new MySqlCommand(sql, cnn))
                        {
                            command.Parameters.AddWithValue("@DATE", date);
                            command.ExecuteNonQuery();
                        }
                        Debug.WriteLine("add to table " + sw.Elapsed);
                        sql = "DROP TEMPORARY TABLE temp_disease_reports";
                        using (MySqlCommand command = new MySqlCommand(sql, cnn))
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch(Exception e)
                    {
                        transaction.Rollback();
                        Debug.WriteLine(e.Message);
                    }
                } 
            }
            Debug.WriteLine("Done " + sw.Elapsed);
        }
        public void InsertVaccineReports(Stream fileStream)
        {
            string sql = "SELECT DATE_SUB(MAX(date), INTERVAL 14 DAY) FROM vaccine_reports";
            DateTime date = Query(sql, (reader) => { reader.Read(); return reader.GetDateTime(0); });
            InsertVaccineReports(date, fileStream);
        }
        public void InsertVaccineReports(DateTime minDate, Stream fileStream)
        {
            var sw = new Stopwatch();
            var dictus = new Dictionary<string, string>();
            sw.Start();
            var lst = new List<CSVVaccineData>();
            var latest = new Dictionary<string, (int, int)>();
            using (TextFieldParser parser = new TextFieldParser(fileStream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                var headers = parser.ReadFields();
                var columnNames = headers.Select((str, index) => new { str = str, index = index })
                                .ToDictionary(x => x.str, x => x.index);
                while(!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    string countryName = fields[columnNames["location"]];
                    if(AlternateCountryName.ContainsKey(countryName))
                    {
                        countryName = AlternateCountryName[countryName];
                    }
                    int vaccinated;
                    if(!Int32.TryParse(fields[columnNames["people_vaccinated"]], out vaccinated))
                    {
                        continue;
                    }
                    if(!latest.ContainsKey(countryName))
                    {
                        latest[countryName] = (0, 0);
                    }
                    int fullyVaccinated;
                    if(!Int32.TryParse(fields[columnNames["people_fully_vaccinated"]], out fullyVaccinated))
                    {
                        fullyVaccinated = latest[countryName].Item1;
                    }
                    latest[countryName] = (fullyVaccinated, latest[countryName].Item2);
                    int boosters;
                    if(!Int32.TryParse(fields[columnNames["total_boosters"]], out boosters))
                    {
                        boosters = latest[countryName].Item2;
                    }
                    latest[countryName] = (fullyVaccinated, latest[countryName].Item2);
                    DateTime date = DateTime.ParseExact(fields[columnNames["date"]], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if(date < minDate)
                    {
                        continue;
                    }
                    lst.Add(new CSVVaccineData {
                        CountryName = countryName,
                        Vaccinated = vaccinated,
                        FullyVaccinated = fullyVaccinated,
                        Boosters = boosters,
                        Date = date
                    });
                }
            }        
            Debug.WriteLine("Dataset processing " + sw.Elapsed);
            using(MySqlConnection cnn  = new MySqlConnection(ConnectionString))
            {
                cnn.Open();
                using(MySqlTransaction transaction = cnn.BeginTransaction())
                {   
                    try
                    {
                        string sql = LongQueries["TempVaccineTable"];
                        using (MySqlCommand command = new MySqlCommand(sql, cnn))
                        command.ExecuteNonQuery();
                        Debug.WriteLine("temp table  " + sw.Elapsed);
                        BulkInsertVac(lst, cnn);
                        Debug.WriteLine("add to temp table "  + sw.Elapsed);
                        sql = LongQueries["AddVaccineReports"];
                        using (MySqlCommand command = new MySqlCommand(sql, cnn))
                        command.ExecuteNonQuery();
                        Debug.WriteLine("add to table " + sw.Elapsed);
                        sql = "DROP TEMPORARY TABLE temp_vaccine_reports";
                        using (MySqlCommand command = new MySqlCommand(sql, cnn))
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch(Exception e)
                    {
                        transaction.Rollback();
                        Debug.WriteLine(e.Message);
                    }
                } 
            }
            Debug.WriteLine("Done " + sw.Elapsed);
        }
    }
}

