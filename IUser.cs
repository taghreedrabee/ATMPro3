using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMapp
{
    internal interface IUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        string Email { get; set; }
        DateTime BirthDate { get; set; }
        UserType Type { get; set; }
        double Balance { get; set; }
    }
    public enum UserType
    {
        Ordinary,
        VIP
    }
}
