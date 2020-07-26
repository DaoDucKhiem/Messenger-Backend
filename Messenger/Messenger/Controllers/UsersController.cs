using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

using Messenger.Helpers;
using Messenger.Entities;
using Messenger.Services;
using Messenger.Models.Users;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Messenger.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;

        public UsersController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// api lấy danh sách user theo số lượng
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            var amount = 30;
            var users = _userService.GetAll(amount);
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<User, UserInfoModel>();
            });

            IMapper mapper = config.CreateMapper();

            var model = mapper.Map<IList<UserInfoModel>>(users);
            return Ok(model);
        }

        /// <summary>
        /// api lấy thông tin user theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public IActionResult GetUser(Guid id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    phone = user.Phone,
                    imageUrl = user.ImageUrl
                });
            }
        }

        /// <summary>
        /// api lấy user theo tên
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("search")]
        public IActionResult GetUsers([FromQuery] string name)
        {
            var users = _userService.GetUserByName(name);

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<User, UserInfoModel>();
            });

            IMapper mapper = config.CreateMapper();

            var model = mapper.Map<IList<UserInfoModel>>(users);
            return Ok(model);
        }
    }
}
