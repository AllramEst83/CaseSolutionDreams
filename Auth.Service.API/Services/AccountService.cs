using Auth.Service.API.Entities;
using Auth.Service.API.Entities.Context;
using Auth.Service.API.Interfaces;
using CaseSolutionsTokenValidationParameters.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Service.API.Services
{
    public class AccountService : IAccountService
    {
        public UserManager<User> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public UserContext Context { get; }
        public ILogger<AccountService> Logger { get; }

        public AccountService(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            UserContext context,
            ILogger<AccountService> logger)
        {
            UserManager = userManager;
            RoleManager = roleManager;
            Context = context;
            Logger = logger;
        }

        public async Task<bool> UserExistByUserName(string userEmail)
        {
            User result = null;
            if (!string.IsNullOrEmpty(userEmail))
            {
                result = await UserManager.FindByEmailAsync(userEmail);
            }

            Logger.LogInformation($"UserExistsByUesrName: {result}");

            return result == null ? false : true;
        }

        public async Task<bool> RoleExists(string userRole)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(userRole))
            {
                result = await RoleManager.RoleExistsAsync(userRole);
            }

            Logger.LogInformation($"RoleExists: {result}");

            return result;
        }

        public async Task<IdentityResult> CreateUser(User userIdentity, string password)
        {
            IdentityResult addUserResult = null;
            if (userIdentity != null && !string.IsNullOrEmpty(password))
            {
                addUserResult = await UserManager.CreateAsync(userIdentity, password);
                SaveChages();
            }

            return addUserResult;
        }

        public async Task<IdentityResult> AddRoleToUser(User userIdentity, string userRole)
        {
            IdentityResult addRoleToUserResult = null;
            if (userIdentity != null && !string.IsNullOrEmpty(userRole))
            {
                addRoleToUserResult = await UserManager.AddToRoleAsync(userIdentity, userRole);
                SaveChages();
            }

            return addRoleToUserResult;
        }

        public async Task<IdentityResult> CreateRole(string role)
        {
            IdentityRole identityRole = null;
            IdentityResult addRoleResult = null;

            if (!string.IsNullOrEmpty(role))
            {
                identityRole = new IdentityRole()
                {
                    Name = role
                };

                addRoleResult = await RoleManager.CreateAsync(identityRole);
                SaveChages();
            }

            return addRoleResult;
        }

        private async void SaveChages()
        {
            await Context.SaveChangesAsync();
        }

        public async void SeedRoles()
        {
            bool result = await RoleManager.RoleExistsAsync(TokenValidationConstants.Roles.AdminAccess);

            if (!result)
            {
                List<IdentityRole> listOfRoles = new List<IdentityRole>()
                {
                    new IdentityRole(){Name = TokenValidationConstants.Roles.AdminAccess},
                    new IdentityRole(){Name = TokenValidationConstants.Roles.EditUserAccess},
                    new IdentityRole(){Name = TokenValidationConstants.Roles.CommonUserAccess},
                };

                foreach (IdentityRole role in listOfRoles)
                {
                    await RoleManager.CreateAsync(role);              
                }
                SaveChages();

            }
        }
        
        public async Task<IList<string>> GetRolesForUser(User user)
        {
            IList<string> userRoles = null;
            if (user != null)
            {
                userRoles = await UserManager.GetRolesAsync(user);
            }

            Logger.LogInformation($"GetRolesForUser: {userRoles.Select(x => x)}");

            return userRoles;
        }
    }
}
