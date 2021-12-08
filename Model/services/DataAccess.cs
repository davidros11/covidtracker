using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace CovidTracker
{
    public class DataAccess
    {
        private static string ConnectionString;
        public static readonly Dictionary<string, string> LongQueries;

        static DataAccess()
        {
            ConnectionString = MyConfig.Configuration.GetSection("ConnectionString").Value;
            LongQueries = GetQueries();
        }

        public MySqlParameter[] CopyParams(MySqlParameter[] parameters)
        {
            var newParams = new MySqlParameter[parameters.Length];
            for(int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                newParams[i] = CreateParameter(param.ParameterName, param.MySqlDbType, param.Value);
            }
            return newParams;
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

        public static MySqlParameter CreateParameter(string name, MySqlDbType dbType, Object value)
        {
            var param = new MySqlParameter(name, dbType);
            param.Value = value ?? DBNull.Value;
            return param;
        }
        public static void SetNull(MySqlParameter[] parameters)
        {
            foreach(var param in parameters)
            {
                if(param.Value == null)
                {
                    param.Value = DBNull.Value;
                }
            }
        }

        public Object OrNull(Object obj)
        {
            if(obj == DBNull.Value)
            {
                return null;
            }
            return obj;
        }
        public T Query<T>(string sql, MySqlParameter[] parameters, Func<MySqlDataReader, T> convert) {
            SetNull(parameters);
            using(MySqlConnection cnn  = new MySqlConnection(ConnectionString))
            {
                cnn.Open();
                using (MySqlCommand command = new MySqlCommand(sql,cnn))
                {
                    command.Parameters.AddRange(parameters);
                    using(MySqlDataReader reader = command.ExecuteReader())
                    {
                        return convert(reader);
                    }
                }
            }
        }
        public T Query<T>(string sql, Func<MySqlDataReader, T> convert) {
            using(MySqlConnection cnn  = new MySqlConnection(ConnectionString))
            {
                cnn.Open();
                using (MySqlCommand command = new MySqlCommand(sql,cnn))
                {
                    using(MySqlDataReader reader = command.ExecuteReader())
                    {
                        return convert(reader);
                    }
                }
            }
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
            string query = LongQueries["ByDate"];
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
        public WorldDateData GetWorldData(DateTime date, int threshold)
        {
            string query = LongQueries["WorldDateData"];
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
        public CountryData GetCountryData(int countryID)
        {
            string sql = "SELECT name, continent FROM countries WHERE id=@ID";
            MySqlParameter[] parameters = { 
                CreateParameter("@ID", MySqlDbType.Int32, countryID)
            };
            Func<MySqlDataReader, CountryData> converter = (MySqlDataReader reader) => {
                if(reader.Read())
                {
                    var data = new CountryData
                    {
                        Name = reader.GetString(1),
                        Continent = reader.GetString(1)
                    };
                    return data;
                }
                return null;
            };
            CountryData data = Query(sql, parameters,converter);
            if(data is null) { return null; }
            Func<MySqlDataReader, List<CountryDiseaseData>> converter2 = (MySqlDataReader reader) => {
                var lst = new List<CountryDiseaseData>();
                while(reader.Read())
                {
                    var data = new CountryDiseaseData
                    {
                        Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        Confirmed = reader.GetInt32(1),
                        Recovered = (int?) OrNull(reader.GetValue(2)),
                        Deaths = reader.GetInt32(3)
                    };
                    lst.Add(data);
                }
                return lst;
            };
            sql = "SELECT date, confirmed, recovered, deaths FROM disease_reports WHERE country_id = @ID";
            parameters = CopyParams(parameters);
            data.DiseaseData = Query(sql, parameters, converter2);
            sql = "SELECT date, vaccinated, fully_vaccinated, number_of_boosters FROM vaccine_reports WHERE country_id = @ID";
            parameters = CopyParams(parameters);
            Func<MySqlDataReader, List<CountryVaccineData>> converter3 = (MySqlDataReader reader) => {
                var lst = new List<CountryVaccineData>();
                while(reader.Read())
                {
                    var data = new CountryVaccineData
                    {
                        Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        Vaccinated = reader.GetInt32(1),
                        FullyVaccinated = reader.GetInt32(2),
                        Boosters = reader.GetInt32(3)
                    };
                    lst.Add(data);
                }
                return lst;
            };
            data.VaccineData = Query(sql, parameters, converter3);
             sql = "SELECT year, population, density, median_age, poverty_rate, diabetes_rate FROM population_reports WHERE country_id = @ID";
            parameters = CopyParams(parameters);
            Func<MySqlDataReader, List<PopulationData>> converter4 = (MySqlDataReader reader) => {
                var lst = new List<PopulationData>();
                while(reader.Read())
                {
                    var data = new PopulationData
                    {
                        Year = reader.GetInt32(0),
                        Population = reader.GetInt32(1),
                        Density = reader.GetFloat(2),
                        MedianAge = (float?) OrNull(reader.GetValue(3)),
                        PovertyRate = (float?) OrNull(reader.GetValue(4)),
                        DiabetesRate = (float?) OrNull(reader.GetValue(5)),
                    };
                    lst.Add(data);
                }
                return lst;
            };
            data.PData = Query(sql, parameters, converter4);
            return data;
        }
    }
}

