using System.ComponentModel.DataAnnotations;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Enums;

namespace FiapCloudGames.Users.Application.DTOs
{
    public class UserDto
    {
        public required int Code { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required UserRole Role { get; set; }

        public static UserDto FromDomainEntity(User user)
        {
            return new UserDto
            {
                Code = user.Code,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
