﻿namespace PollSystemApp.Application.Common.Dto.UserDtos
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}