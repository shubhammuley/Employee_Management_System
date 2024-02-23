using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Services.Implementations
{
    public class UserAccountService : IUserAccountService
    {
        public Task<GeneralResponse> CreateAsync(Register user)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponse> SignInAsync(Login User)
        {
            throw new NotImplementedException();
        }


        public Task<LoginResponse> RefreshTokenAsync(RefreshToken refreshToken)
        {
            throw new NotImplementedException();
        }     



        public Task<WeatherForecast[]> GetWeatherForecasts()
        {
            throw new NotImplementedException();
        }
    }
}
