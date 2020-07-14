using Messenger.Entities;
using Messenger.Helpers;
using Messenger.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Messenger.Services
{
    public interface IUserService
    {
        User Authenticate(string email, string password);
        IEnumerable<User> GetAll();
        User GetUserById(Guid id);
        User CreateUser(RegisterModel model);
    }

    public class UserService : IUserService
    {
        private DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = _context.Users.SingleOrDefault(u => u.Email == email);

            // kiểm tra người dùng tồn tại
            if (user == null) return null;

            // kiểm tra nếu sai password thì return null
            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return null;

            //nếu email và mật khẩu đúng thì return true
            return user;
        }

        /*
         * trả về tất cả user có trong bảng
         */
        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        /*
         * trả về user nếu trùng id
         */
        public User GetUserById(Guid id)
        {
            return _context.Users.Find(id);
        }

        /*
         * hàm xác thực password
         * trả về true nếu đúng, false nếu sai
         */
        private static bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password không thể trống hoặc chỉ chứa khoảng trắng.", "password");
            if (passwordHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");

            using var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt);
            var pass = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < pass.Length; i++)
            {
                if (pass[i] != passwordHash[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Tạo user trong database
        /// Nếu đã tồn tại email trả về null
        /// </summary>
        /// <param name="user">RegisterModel</param>
        /// <returns>User</returns>
        /// create by Duc Khiem
        public User CreateUser(RegisterModel user)
        {
            if (string.IsNullOrWhiteSpace(user.Password))
                throw new AppException("Password is required");

            if (_context.Users.Any(x => x.Email == user.Email))
                throw new AppException("Username \"" + user.Email + "\" is already taken");

            CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var _user = new User
            {
                Id = new Guid(),
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                ImageUrl = user.ImageUrl,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };

            _context.Users.Add(_user);
            _context.SaveChanges();

            return _user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            // băm sử dụng thuật toán PBKDF2
            // thêm một chuỗi byte ngẫu nhiên vào mật khẩu của người dùng tránh trường hợp trùng email password
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key; 
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
