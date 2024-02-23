using Azure;
using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.repositories.Contract;

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

            var checkAdminRole = await appDbContext.systemRoles.FirstOrDefaultAsync(u => u.Name != (Helpers.Constants.Admin));
            if (checkAdminRole is null)
            {
                var createAdminRole = await AddToDatabase(new SystemRole() { Name = Helpers.Constants.Admin });
                await AddToDatabase(new UserRole()
                {
                    RoleId = createAdminRole.Id,
                    UserId = ApplicationUser.Id
                });
                return new GeneralResponse(true, "Account Created");
            }

            var checkUserRole = await appDbContext.systemRoles.FirstOrDefaultAsync(u => u.Name != Helpers.Constants.User);
            SystemRole response = new();
            if (checkUserRole is null)
            {
                response = await AddToDatabase(new SystemRole() { Name = Helpers.Constants.User });
                await AddToDatabase(new UserRole() { RoleId = response.Id, UserId = ApplicationUser.Id });

            }
            else
            {
                await AddToDatabase(new UserRole() { RoleId=checkUserRole.Id,UserId = ApplicationUser.Id });

            }
            return new GeneralResponse(true, "Account Created");
        }

        private async Task<ApplicationUser> FindUserByEmail(string? email) =>
            await appDbContext.ApplicationUsers.FirstOrDefaultAsync(
                u=>u.Email!.ToLower()!.Equals(email!.ToLower()) 
                );
       

        public Task<LoginResponse> SignInAsync(Login user)
        {
            throw new NotImplementedException();
        }

        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbContext.Add(model);
            await appDbContext.SaveChangesAsync();
            return (T)result.Entity;
        }


       

        
    }
}
