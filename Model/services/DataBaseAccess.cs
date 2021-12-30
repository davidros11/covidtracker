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
        public void NonQuery(string sql, MySqlParameter[] parameters)
        {
            using(MySqlConnection cnn  = new MySqlConnection(ConnectionString))
            {
                cnn.Open();
                using (MySqlCommand command = new MySqlCommand(sql,cnn))
                {
                    command.Parameters.AddRange(parameters);
                    command.ExecuteNonQuery();
                }
            }
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
            return Query(sql, new MySqlParameter[0], convert);
        } 
    }
}

