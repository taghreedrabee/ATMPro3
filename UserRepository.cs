using System.Linq;
using BCrypt.Net;
using System;

namespace ATMapp
{
    internal class UserRepository : IUserRepository
    {
        private readonly AtmDbContext _context;
        private const int WORK_FACTOR = 11; // Consistent work factor for BCrypt

        public UserRepository(AtmDbContext context)
        {
            _context = context;
        }

        public void AddUser(IUser user)
        {
            var userEntity = (User)user;
            // Hash the password only once during user creation
            userEntity.PasswordHash = HashPassword(userEntity.PasswordHash);
            _context.Users.Add(userEntity);
            _context.SaveChanges();
        }

        public IUser GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public IUser GetUserById(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == userId);
        }

        public void UpdateUser(IUser user)
        {
            var userEntity = (User)user;
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == userEntity.UserId);

            if (existingUser != null)
            {
                // Only hash the password if it has been changed
                if (!string.IsNullOrEmpty(userEntity.PasswordHash) &&
                    !userEntity.PasswordHash.StartsWith("$2")) // Check if it's not already a BCrypt hash
                {
                    userEntity.PasswordHash = HashPassword(userEntity.PasswordHash);
                }
                else
                {
                    // If password hasn't changed, keep the existing hash
                    userEntity.PasswordHash = existingUser.PasswordHash;
                }

                _context.Users.Update(userEntity);
                _context.SaveChanges();
            }
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public bool DeleteUser(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null && ValidatePassword(password, user.PasswordHash))
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool VerifyPassword(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                return ValidatePassword(password, user.PasswordHash);
            }
            return false;
        }

        // Private helper methods for consistent password hashing
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WORK_FACTOR);
        }

        private bool ValidatePassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                // If verification fails due to invalid hash format
                return false;
            }
        }
    }
}