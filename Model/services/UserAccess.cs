using MySql.Data.MySqlClient;
using System.Data;
using MySql.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
namespace CovidTracker
{
    public class UserAccess : DataBaseAccess
    {
        private const string SEP = "$";
        private const string FORMAT = "{0}" + SEP + "{1}" + SEP +"{2}";
        public bool AuthenticateUser(Login login)
        {
            string sql = "SELECT id, name, password FROM managers WHERE name=@NAME";
            MySqlParameter[] parameters = { CreateParameter("@NAME", MySqlDbType.VarChar, login.Name) };
            User user = Query(sql, parameters, (reader) => {
                if(!reader.Read())
                {
                    return null;
                }
                return new User {
                    ID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    PasswordHash = reader.GetString(2)
                };
            });
            if(user == null)
            {
                return false;
            }
            return KDFVerify(login.Password, user.PasswordHash);
        }
        private static string Hash(string password, byte[] salt, int iter)
        {
            byte[] hash;
            using (var hasher = new Rfc2898DeriveBytes(password, salt, iter))
            {
                hash = hasher.GetBytes(20);
            }
            return String.Format(FORMAT, Convert.ToBase64String(hash), Convert.ToBase64String(salt), iter.ToString());
        }
        /// <summary>
        /// Comapres the hash of an input password with the hash in the database
        /// </summary>
        /// <param name="password">The input password</param>
        /// <param name="storedHash">The database hash</param>
        /// <returns>True if they equal, false otherwise</returns>
        public static bool KDFVerify(string password, string storedHash)
        {
            string[] components = storedHash.Split(SEP);
            if(components.Length != 3) { return false; }
            string inputHash = Hash(password, Convert.FromBase64String(components[1]), Int32.Parse(components[2]));
            return inputHash == storedHash;
        }
    }
}

