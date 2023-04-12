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
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace API.Controllers
{
    public class AccountsController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountsController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;

        }        

        [HttpPost("register")]

        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await UserExists(registerDTO.userName)) return BadRequest("Username is taken");

            var user = _mapper.Map<AppUser>(registerDTO);
            using var hmac = new HMACSHA512();
            
                user.UserName = registerDTO.userName;
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
                user.PasswordSalt = hmac.Key;
            

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDTO {
                userName = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }

        [HttpPost("login")]

        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x=>x.UserName == loginDTO.username);
            if(user == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.password));

            for (int i=0;i<computedHash.Length;i++)
            {
                if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("invalid password");
            }

              return new UserDTO {
                userName = user.UserName,
                Token = _tokenService.CreateToken(user),
                photoUrl = user.Photos.FirstOrDefault(x =>x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        
        } 

        private async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(x=>x.UserName == userName.ToLower());
        }
    }
}