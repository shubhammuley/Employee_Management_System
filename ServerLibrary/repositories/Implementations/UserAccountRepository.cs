using Azure;
using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using ServerLibrary.Data;
using ServerLibrary.Data.Migrations;
using ServerLibrary.Helpers;
using ServerLibrary.repositories.Contract;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerLibrary.repositories.Implementations
{
    public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccount
    {
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            if (user is null) return new GeneralResponse(false, "Model is empty");

            var checkUser = await FindUserByEmail(user.Email!);

            if (checkUser != null) return new GeneralResponse(false, "User registered already.");

            //Save User
            var ApplicationUser = await AddToDatabase(new ApplicationUser()
            {
                FullName = user.FullName,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            }
            );

            //check, create and assign role

            var checkAdminRole = await appDbContext.systemRoles.FirstOrDefaultAsync(u => u.Name!.Equals(Helpers.Constants.Admin));
            if (checkAdminRole is null)
            {
                var createAdminRole = await AddToDatabase(new SystemRole() { Name = Helpers.Constants.Admin });
                await AddToDatabase(new UserRole(){RoleId = createAdminRole.Id,UserId = ApplicationUser.Id});
                return new GeneralResponse(true, "Account Created");
            }

            var checkUserRole = await appDbContext.systemRoles.FirstOrDefaultAsync(u => u.Name!.Equals (Helpers.Constants.User));
            SystemRole response = new();
            if (checkUserRole is null){
                response = await AddToDatabase(new SystemRole() { Name = Helpers.Constants.User });
                await AddToDatabase(new UserRole() { RoleId = response.Id, UserId = ApplicationUser.Id });
            }
            else
            {
                await AddToDatabase(new UserRole() { RoleId=checkUserRole.Id,UserId = ApplicationUser.Id });

            }
            return new GeneralResponse(true, "Account Created");
        }

       

        public async Task<LoginResponse> SignInAsync(Login user)
        {
            if (user is null) return new LoginResponse(false, "Model is empty");

            var applicationUser = await FindUserByEmail(user.Email);
            if(applicationUser is null) return new LoginResponse(false, "User Not Found");

            // Verify Password
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse(false, "Email/Password is invalid.");

            var getUserRole = await FindUserRole(applicationUser.Id);
            if (getUserRole is null) return new LoginResponse(false, "User not found.");

            var getRoleName = await FindRoleName(getUserRole.RoleId);
            if (getRoleName is null) return new LoginResponse(false, "User not found.");

            //Token Generation
            string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
            string refreshToken = GenerateRefreshToken();

            var findUser = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(u=>u.UserId == applicationUser.Id);
            if(findUser is not null)
            {
                findUser!.Token = refreshToken;
                await appDbContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UserId = applicationUser.Id });
            }

            return new LoginResponse(true,"Login Successful", jwtToken, refreshToken);

        }

        private string GenerateToken(ApplicationUser user, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var UserClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Role,role),
                new Claim(ClaimTypes.Name, user.FullName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };
            var token = new JwtSecurityToken(
                issuer : config.Value.Issuer,
                audience :config.Value.Audience,
                claims: UserClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private async Task<UserRole> FindUserRole(int userId) => await appDbContext.userRoles.FirstOrDefaultAsync(u => u.Id == userId);
        private async Task<SystemRole> FindRoleName(int roleId) => await appDbContext.systemRoles.FirstOrDefaultAsync(u => u.Id == roleId);
        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));


        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
        {
            if (token is null) return new LoginResponse(false,"Model is Empty");

            var findToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(u => u.Token!.Equals(token.Token));
            if (findToken is null) return new LoginResponse(false, "Token is required");

            //get user details
            var user = await appDbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == findToken.UserId);
            if (user is null) return new LoginResponse(false, "Token cannot be generated as user not found.");

            var userRole = await FindUserRole(user.Id);
            var roleName = await FindRoleName(userRole.Id);
            string jwtToken = GenerateToken(user, roleName.Name!);
            string refreshToken = GenerateRefreshToken();

            var updateRefreshToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (updateRefreshToken is null) return new LoginResponse(false, "Refresh token could not be generated as user has not signedIn");
        
            updateRefreshToken.Token = refreshToken;
            await appDbContext.SaveChangesAsync();
            return new LoginResponse(true, "Token refreshed successfully", jwtToken, refreshToken);
        }




        private async Task<ApplicationUser> FindUserByEmail(string? email)
        {
            return await appDbContext.ApplicationUsers.FirstOrDefaultAsync(
                u => u.Email!.ToLower()!.Equals(email!.ToLower())
                );
        }
        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbContext.Add(model);
            await appDbContext.SaveChangesAsync();
            return (T)result.Entity;
        }


       

        
    }
}
