using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Messenger.Helpers;
using Messenger.Models.Users;
using Messenger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Messenger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;

        public AccountsController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// api xác thực khi người dùng đăng nhập
        /// </summary>
        /// <param name="model">model client truyền lên</param>
        /// <returns>user và token nếu thành công</returns>
        /// <returns>message nếu thất bại</returns>
        /// created by Đào Đức Khiêm
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Email, model.Password);

            if (user == null)
                return BadRequest(new { message = "Email hoặc mật khẩu không chính xác!" });
            var token = _userService.GenerateJwtStringee(_appSettings.IsUser, _appSettings.Secret, user.Id.ToString(), user.Email, user.ImageUrl, user.FullName);

            // return basic user info and authentication token
            return Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Phone,
                user.ImageUrl,
                Token = token
            });
        }

        /// <summary>
        /// đăng ký tài khoản cho người dùng
        /// </summary>
        /// <param name="model">model client truyền lên</param>
        /// <returns>token nếu thành công</returns>
        /// <returns>message nếu thất bại</returns>
        /// created by Đào Đức Khiêm
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            try
            {
                //tạo account
                var user = _userService.CreateUser(model);
                var token = _userService.GenerateJwtStringee(_appSettings.IsUser, _appSettings.Secret, user.Id.ToString(), user.Email, user.ImageUrl, user.FullName);
                return Ok(new
                {
                    Token = token
                });
            }
            catch (AppException ex)
            {
                // trả về exception nếu lỗi
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// cập nhật user
        /// </summary>
        /// <param name="_user"></param>
        /// <returns>user + token</returns>
        /// <returns>message nếu thất bại</returns>
        [HttpPut("updateProfile")]
        public async Task<IActionResult> PutUserAsync([FromBody] UpdateProfileModel _user)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(_user);
                var token = _userService.GenerateJwtStringee(_appSettings.IsUser, _appSettings.Secret, user.Id.ToString(), user.Email, user.ImageUrl, user.FullName);
                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.Phone,
                    user.ImageUrl,
                    token
                });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// cập nhật password
        /// </summary>
        /// <param name="_user">thông tin mật khẩu</param>
        /// <returns>message</returns>
        [HttpPut("updatePassword")]
        public async Task<IActionResult> PutUserPasswordAsync([FromBody] UpdatePasswordModel _user)
        {
            try
            {
                var result = await _userService.UpdateUserPasswordAsync(_user);
                return Ok(new
                {
                    result,
                });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}