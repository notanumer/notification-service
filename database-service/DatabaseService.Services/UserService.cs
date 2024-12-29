using DatabaseService.DataAccess.Abstractions;
using DatabaseService.DataAccess.RabbitMq;
using DatabaseService.Models.Postgres;
using DatabaseService.Models.Rabbit;
using DatabaseService.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseService.Services
{
    public class UserService : IUserService
    {
        private readonly IAppDbContext _appDbContext;

        public UserService(IAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task CreateUser(CreateUserRequest request)
        {
            var user = new User()
            {
                UserName = request.UserName,
                Credentials = request.Credentials
            };

            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<string?> GetCredentials(string name, ChannelType type)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(user => user.UserName == name);
            if (user == null) return null;

            switch (type)
            {
                case ChannelType.Telegram:
                    return user.Credentials?.TelegramChatId;
                case ChannelType.Email:
                    return user.Credentials?.Email;
                default:
                    throw new NotImplementedException();
            }
        }

        public async Task PatchCredentials(string name, Credentials credentials)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(user => user.UserName == name);

            if (user == null) return;
            if (user.Credentials == null) user.Credentials = new Credentials();

            if (credentials.TelegramChatId != null)
                user.Credentials.TelegramChatId = credentials.TelegramChatId;
            if (credentials.Email != null)
                user.Credentials.Email = credentials.Email;

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<string?> UserCredential(string name, ChannelType type)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(user => user.UserName == name);
            if (user == null) return null;

            switch(type)
            {
                case ChannelType.Email:
                    return user.Credentials?.Email;
                case ChannelType.Telegram:
                    return user.Credentials?.TelegramChatId;
                default:
                    return null;
            }
        }
    }
}
