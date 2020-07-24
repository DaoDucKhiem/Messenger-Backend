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

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
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

        //PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
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
    }
}