using AutoMapper;
using ExpMedia.Application.AccountFolder;
using ExpMedia.Application.Helper;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static ExpMedia.Application.AccountFolder.AccountDTO;

namespace ExpMediaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly DataContext context;
        private readonly IMapper mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IFileStorageService fileStorageService;
        private string container = "users";

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            DataContext context,
            IMapper mapper, RoleManager<IdentityRole> roleManager,
            IFileStorageService fileStorageService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.context = context;
            this.mapper = mapper;
            _roleManager = roleManager;
            this.fileStorageService = fileStorageService;
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login(
             [FromBody] LoginVM userCredentials)
        {
            var result = await signInManager.PasswordSignInAsync(userCredentials.Email,
                userCredentials.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await BuildTokendd(userCredentials);
            }
            else
            {
                return BadRequest("Incorrect Login");
            }
        }
        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponse>> Register(
          [FromForm] RegisterVM userCredentials)
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            var user = new AppUser
            {
                UserName = userCredentials.Email,
                Email = userCredentials.Email,
                Role = userCredentials.RoleName,
                FirstName = userCredentials.FirstName,
                LastName = userCredentials.LastName,
      
            };
            if (userCredentials.ImageUrl != null)
            {
                user.ImageUrl = await fileStorageService.SaveFile(container, userCredentials.ImageUrl);
            }

            var result = await userManager.CreateAsync(user, userCredentials.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, userCredentials.RoleName);

                return await BuildTokendd(userCredentials);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        private async Task<AuthenticationResponse> BuildTokendd(RegisterVM userCredentials)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", userCredentials.Email),

            };

            var user = await userManager.FindByNameAsync(userCredentials.Email);
            var claimsDB = await userManager.GetClaimsAsync(user);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["keyjwt"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiration, signingCredentials: creds);

            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                RoleName = userCredentials.RoleName
            };
        }

        private async Task<AuthenticationResponse> BuildTokendd(LoginVM userCredentials)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", userCredentials.Email),

            };

            var user = await userManager.FindByNameAsync(userCredentials.Email);

            var claimsDB = await userManager.GetClaimsAsync(user);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["keyjwt"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiration, signingCredentials: creds);

            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                RoleName = user.Role
            };
        }

    }
}
