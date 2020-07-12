using Messenger.Entities;
using Messenger.Helpers;
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
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = _context.Users.SingleOrDefault(u => u.Email == email);

            // kiểm tra người dùng tồn tại
            if (user == null) return null;

            // kiểm tra nếu sai password thì return null
            if (!VerifyPassword(password, user.PasswordHash))
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
        private static bool VerifyPassword(string password, string passwordHash)
        {
            if(password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password không thể trống hoặc chỉ chứa khoảng trắng.", "password");

            if (!password.Equals(passwordHash)) return false;

            return true;
        }
    }
}
