using DatabaseService.Models.Postgres;
using DatabaseService.Models.Rabbit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseService.Services.Abstractions
{
    public interface IUserService
    {
        Task CreateUser(CreateUserRequest request);
        Task PatchCredentials(string name, Credentials credentials);
        Task<string?> GetCredentials(string name, ChannelType type);
        Task<string> UserCredential(string name, ChannelType type);
    }
}
