using System;
namespace CovidTracker
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string PasswordHash { get; set; }
    }
}