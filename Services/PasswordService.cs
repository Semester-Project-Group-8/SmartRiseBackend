using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services
{
    public class PasswordService
    {
        public string HashPassword<T>(T user, string password) where T : class//to be used at password creation
        {
            var hasher = new PasswordHasher<T>();
            return hasher.HashPassword(user, password);//return hashed password string pwd -> hashing -> string hashedpassword
        }
        public bool VerifyPassword<T>(T user,string hashedPassword,string password) where T : class//used at every signin
        {
            var hasher = new PasswordHasher<T>();
            return hasher.VerifyHashedPassword(user, hashedPassword, password) == PasswordVerificationResult.Success;//return true or false if password succeded
        }
    }
}
