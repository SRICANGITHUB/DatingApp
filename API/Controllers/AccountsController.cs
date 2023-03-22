using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace API.Controllers
{
    public class AccountsController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountsController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;

        }        

        [HttpPost("register")]

        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await UserExists(registerDTO.userName)) return BadRequest("Username is taken");
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDTO.userName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDTO {
                userName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]

        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.UserName == loginDTO.username);
            if(user == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.password));

            for (int i=0;i<computedHash.Length;i++)
            {
                if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("invalid password");
            }

              return new UserDTO {
                userName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        
        } 

        private async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(x=>x.UserName == userName.ToLower());
        }
    }
}