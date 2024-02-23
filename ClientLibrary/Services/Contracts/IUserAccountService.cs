﻿using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ClientLibrary.Services.Contracts
{
    public interface IUserAccountService
    {
        Task<GeneralResponse> CreateAsync(Register user);

        Task<LoginResponse> SignInAsync(Login User);
        Task<LoginResponse> RefreshTokenAsync(RefreshToken refreshToken);

        Task<WeatherForecast[]> GetWeatherForecasts();

       
    }
}
