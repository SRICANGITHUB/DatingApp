using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        
        private readonly IUserRepository _useRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository useRepository, IMapper mapper)
        {
            _useRepository = useRepository;
            _mapper = mapper;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
             var users = await _useRepository.GetMembersAsync();

           
            
            return Ok(users);
        }

        
        [HttpGet("{username}")]
        //[HttpGet]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            var user = await _useRepository.GetMemberAsync(username);
            
            return _mapper.Map<MemberDTO>(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _useRepository.GetUserByUsernameAsync(username);
            if(user==null) return NotFound();

            _mapper.Map(memberUpdateDTO,user);

            if(await _useRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}