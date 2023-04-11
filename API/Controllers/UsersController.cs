using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
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
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository useRepository, IMapper mapper, IPhotoService photoService)
        {
            _useRepository = useRepository;
            _mapper = mapper;
            _photoService=photoService;
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
            var username = User.GetUsername();
            var user = await _useRepository.GetUserByUsernameAsync(username);
            if(user==null) return NotFound();

            _mapper.Map(memberUpdateDTO,user);

            if(await _useRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]

        public async Task<ActionResult<PhotoDTO>> AddPhoto (IFormFile file)
        {
            var user=await _useRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);
            if(result.Error!=null) return BadRequest(result.Error.Message);

            var photo = new Photo {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await _useRepository.SaveAllAsync()) //return _mapper.Map<PhotoDTO>(photo);
            {
                return CreatedAtAction(nameof(GetUser), 
                new {username = user.UserName}, _mapper.Map<PhotoDTO>(photo));
            }

            return BadRequest("Something wrong while adding photo");
        }

        [HttpPut("set-main-photo/{photoId}") ]

        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _useRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo== null) return NotFound();

            if(photo.IsMain) return BadRequest("this is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain!=null) currentMain.IsMain= false;
            photo.IsMain = true;

            if(await _useRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletPhoto(int photoId)
        {

            var user = await _useRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

           if(photo== null) return NotFound();

            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null)
            {
                var results = await _photoService.DeletPhotoAsync(photo.PublicId);
                if (results.Error!=null) return BadRequest(results.Error.Message);
                
            }

            user.Photos.Remove(photo);

            if(await _useRepository.SaveAllAsync()) return Ok();
            return BadRequest("Problem in deleting photo");
        }

    }
}