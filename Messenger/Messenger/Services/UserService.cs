﻿using Messenger.Entities;
using Messenger.Helpers;
using Messenger.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Services
{
    public interface IUserService
    {
        //xử lý xác thực khi đăng nhập
        User Authenticate(string email, string password);

        //xử lý lấy tất cả người dùng trên hệ thống
        IEnumerable<User> GetAll(int amount);

        //lấy thông tin người dùng theo id
        User GetUserById(Guid id);

        //tìm kiếm người dùng theo tên
        IEnumerable<User> GetUserByName(string name);

        //đăng ký tài khoản
        User CreateUser(RegisterModel model);

        //tạo token
        string GenerateJwtStringee(string keyID, string keySecret, string id, string email, string avatar, string fullName);

        //cập nhật profile
        Task<User> UpdateUserAsync(UpdateProfileModel model);

        //cập nhật password
        Task<string> UpdateUserPasswordAsync(UpdatePasswordModel model);
    }

    public class UserService : IUserService
    {
        private DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// service để xác thực người dùng khi đăng nhập.
        /// </summary>
        /// <param name="email">email client</param>
        /// <param name="password">password client</param>
        /// <returns>null nếu email rỗng</returns>
        /// <returns>null nếu chưa tồn tại tài khoản</returns>
        /// <returns>null nếu password không chính xác</returns>
        /// <returns>user nếu xác thực thành công</returns>
        /// create by Đào Đức Khiêm
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

            //nếu email và mật khẩu đúng thì return user
            return user;
        }

        /// <summary>
        /// trả về tất cả các user có trong database
        /// </summary>
        /// <param name="amount">truyền vào số lượng user cần lấy</param>
        /// <returns>danh sách user</returns>
        public IEnumerable<User> GetAll(int amount)
        {
            return _context.Users.Take(amount).ToList() ;
        }

        /// <summary>
        /// lấy người dùng theo id
        /// </summary>
        /// <param name="id">id truyền từ client</param>
        /// <returns>user nếu tồn tại</returns>
        /// <returns>null nếu không tồn tại</returns>
        public User GetUserById(Guid id)
        {
            return _context.Users.Find(id);
        }

        /// <summary>
        /// Hàm tạo token
        /// </summary>
        /// <param name="keyID">Key ID stringee</param>
        /// <param name="keySecret">key Secret của stringee</param>
        /// <param name="id">id user</param>
        /// <param name="email">email user</param>
        /// <param name="avatar">avatar user</param>
        /// <param name="fullName">username</param>
        /// <returns></returns>
        public string GenerateJwtStringee(string keyID, string keySecret, string id, string email, string avatar, string fullName)
        {
            // tạo token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(keySecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                   new Claim("userId", id),
                   new Claim("email", email),
                   new Claim("avatar", avatar),
                   new Claim("fullName", fullName),
                }),
                Issuer = keyID,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// hàm xác thực password
        /// </summary>
        /// <param name="password">password người dùng nhập</param>
        /// <param name="passwordHash">password băm</param>
        /// <param name="passwordSalt">password trọng số vài byte ban đầu</param>
        /// <returns>Đào Đức Khiêm</returns>
        private static bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password không thể trống hoặc chỉ chứa khoảng trắng.", "password");
            if (passwordHash.Length != 64) throw new ArgumentException("Chiều dài của password không chính xác (64 bytes).", "passwordHash");

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
                throw new AppException("Password không thể bỏ trống");

            if (_context.Users.Any(x => x.Email == user.Email))
                throw new AppException("Email \"" + user.Email + "\" đã tồn tại");

            if (_context.Users.Any(x => x.FullName == user.FullName))
                throw new AppException("Tên \"" + user.FullName + "\" đã tồn tại");

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

        /// <summary>
        /// Băm password
        /// </summary>
        /// <param name="password">password dạng string</param>
        /// <param name="passwordHash">password được băm</param>
        /// <param name="passwordSalt">key để giải mã</param>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password không thể trống hoặc chỉ chứa khoảng trắng.", "password");

            // băm sử dụng thuật toán PBKDF2
            // thêm một chuỗi byte ngẫu nhiên vào mật khẩu của người dùng tránh trường hợp trùng email password
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// lấy người dùng theo tên truyền vào, dùng khi search trong danh bạ
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<User> GetUserByName(string name)
        {
            return _context.Users.Where(data => data.FullName.Contains(name)).ToList();
        }

        /// <summary>
        /// cập nhật thông tin của người dùng
        /// </summary>
        /// <param name="model">model client truyền lên khi gọi api</param>
        /// <returns>null nếu model không có dữ liệu, không tồn tại tài khoản</returns>
        /// <returns>user sau update nếu thành công</returns>
        public async Task<User> UpdateUserAsync(UpdateProfileModel model)
        {

            if (model == null)
                throw new AppException("Không có dữ liệu update");

            var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);

            if(user == null)
            {
                throw new AppException("Người dùng không tồn tại");
            }
            else
            {
                if(user.Email != model.Email)
                {
                    //kiểm tra xem đã có tài khoản nào dùng email này chưa
                    if (_context.Users.Any(x => x.Email == model.Email))
                        throw new AppException("Email \"" + model.Email + "\" đã tồn tại");
                }

                if(user.FullName != model.FullName)
                {
                    //kiểm tra xem đã có tài khoản nào dùng tên này chưa
                    if (_context.Users.Any(x => x.FullName == model.FullName))
                        throw new AppException("Tên \"" + model.FullName + "\" đã tồn tại");
                }
            }

            user.Email = model.Email;
            user.FullName = model.FullName;
            user.ImageUrl = model.ImageUrl;
            user.Phone = model.Phone;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("Cập nhật thất bại!");
            }
            return user;
        }

        /// <summary>
        /// cập nhật password
        /// </summary>
        /// <param name="model">UpdatePasswordModel</param>
        /// <returns>null nếu model rỗng, người dùng không tồn tại</returns>
        public async Task<string> UpdateUserPasswordAsync(UpdatePasswordModel model)
        {
            if (model == null)
                throw new AppException("Không có dữ liệu update");

            var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);

            if (user == null)
            {
                throw new AppException("Người dùng không tồn tại");
            }
            else
            {
                //xác thực mật khẩu
                var checkPass = VerifyPassword(model.OldPass, user.PasswordHash, user.PasswordSalt);
                if (!checkPass)
                {
                        throw new AppException("Mật khẩu cũ không chính xác!");
                }
            }

            //tạo mật khẩu mới
            CreatePasswordHash(model.NewPass, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            //cập nhật mật khẩu trong database
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                //nếu cập nhật thành công
                return "Cập nhật mật khẩu thành công";
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("Cập nhật mật khẩu thất bại!");
            }
        }
    }
}
