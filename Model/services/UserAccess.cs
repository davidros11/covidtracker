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
        /// <summary>
        /// Checks if the user and password match
        /// </summary>
        /// <param name="login">login data</param>
        /// <returns>true if they match, false otherwise</returns>
        public bool AuthenticateUser(Login login)
        {
            string sql = "SELECT password FROM managers WHERE name=@NAME";
            MySqlParameter[] parameters = { CreateParameter("@NAME", MySqlDbType.VarChar, login.Name) };
            string hash = Query(sql, parameters, (reader) => {
                if(!reader.Read())
                {
                    return null;
                }
                return reader.GetString(0);
            });
            if(hash == null)
            {
                return false;
            }
            return Verify(login.Password, hash);
        }
        /// <summary>
        /// Hashes a password using PBKDF2-HMAC-SHA1
        /// </summary>
        /// <param name="password">password string</param>
        /// <param name="salt">salt</param>
        /// <param name="iter">number of iterations</param>
        /// <returns>Hash in Base64 form</returns>
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
        public static bool Verify(string password, string storedHash)
        {
            string[] components = storedHash.Split(SEP);
            if(components.Length != 3) { return false; }
            string inputHash = Hash(password, Convert.FromBase64String(components[1]), Int32.Parse(components[2]));
            return inputHash == storedHash;
        }
    }
}

