using MySql.Data.MySqlClient;
using System.Data;
using MySql.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Globalization;
namespace CovidTracker
{
    public class DataBaseAccess
    {
        protected static readonly string ConnectionString;

        static DataBaseAccess()
        {
            ConnectionString = MyConfig.Configuration.GetSection("ConnectionString").Value;
        }
        /// <summary>
        /// Makes a copy of an array of MySqlParameters so they can be used in another query
        /// </summary>
        /// <param name="parameters">the original MySqlParameter array</param>
        /// <returns>>the new MySqlParameter array</returns>
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
        /// <summary>
        /// Create a MySqlParameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="dbType">parameter type</param>
        /// <param name="value">parameter value</param>
        /// <returns>the MySqlParameter</returns>
        public static MySqlParameter CreateParameter(string name, MySqlDbType dbType, Object value)
        {
            var param = new MySqlParameter(name, dbType);
            param.Value = value ?? DBNull.Value;
            return param;
        }
        /// <summary>
        /// Sets all parameters with value null to DBNull
        /// </summary>
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
        /// <summary>
        /// If the object is DBNull, converts it to null
        /// </summary>
        /// <param name="obj">the object</param>
        /// <returns>the object, or null</returns>
        public Object OrNull(Object obj)
        {
            if(obj == DBNull.Value)
            {
                return null;
            }
            return obj;
        }
        /// <summary>
        /// Performs a MySql query and returns the results.
        /// </summary>
        /// <param name="sql">the SQL command string</param>
        /// <param name="parameters">the parameters for the command</param>
        /// <param name="convert">a function that takes the data from the MySqlDataReader and converts it into the desired object</param>
        /// <typeparam name="T">The type of object that should be returned</typeparam>
        /// <returns>The query results put inside the given object type</returns>
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
        /// <summary>
        /// Overload of Query without parametrs
        /// </summary>
        public T Query<T>(string sql, Func<MySqlDataReader, T> convert) {
            return Query(sql, new MySqlParameter[0], convert);
        } 
    }
}

