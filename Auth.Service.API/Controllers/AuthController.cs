using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth.Service.API.Entities;
using Auth.Service.API.Entities.Context;
using Auth.Service.API.Interfaces;
using Auth.Service.API.Models;
using Auth.Service.API.ResponseModels;
using Auth.Service.API.ViewModels;
using AutoMapper;
using CaseSolutionsTokenValidationParameters.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Auth.Service.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public IAuthService AuthService { get; }
        public IAccountService AccountService { get; }
        public IMapper Mapper { get; }
        public UserContext Context { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public IJwtService JwtService { get; }
        public ILogger<AuthController> Logger { get; }
        public AppSettings AppSettings { get; }

        private readonly JwtIssuerOptions _jwtOptions;

        public AuthController(
            IAuthService authService,
            IAccountService accountService,
            IMapper mapper,
            UserContext context,
            RoleManager<IdentityRole> roleManager,
            IJwtService jwtService,
            IOptions<JwtIssuerOptions> jwtOptions,
            ILogger<AuthController> logger,
            IOptions<AppSettings> options)
        {
            AuthService = authService;
            AccountService = accountService;
            Mapper = mapper;
            Context = context;
            RoleManager = roleManager;
            JwtService = jwtService;
            Logger = logger;
            AppSettings = options.Value;
            _jwtOptions = jwtOptions.Value;         
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Ping()
        {
            Logger.LogInformation("AuthController was pinged.");
            return new JsonResult(new { message = "AuthController is online!" });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.AuthAPIAdmin)]
        [HttpGet]
        public IActionResult AdminAuthTest()
        {
            Logger.LogInformation("AdminAuthTest action has been requested.");

            return new OkObjectResult(new { message = "Adimn authentication works" });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.AuthAPIEditUser)]
        [HttpGet]
        public IActionResult EditAuthTest()
        {
            Logger.LogInformation("EditAuthTest action has been requested.");

            return new OkObjectResult(new { message = "Edit authentication works" });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.AuthAPICommonUser)]
        [HttpGet]
        public IActionResult CommonAuthTest()
        {
            Logger.LogInformation("CommonAuthTest action has been requested.");

            return new OkObjectResult(new { message = "Common authentication works" });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SeedRoles()
        {
            bool result = await RoleManager.RoleExistsAsync(TokenValidationConstants.Roles.AdminAccess);
            List<IdentityRole> listOfRoles = null;

            if (!result)
            {
                List<RolesSeedModel> seedData = null;
                using (StreamReader reader = new StreamReader(AppSettings.AuthSeedData))
                {
                    string json = reader.ReadToEnd();
                    seedData = JsonConvert.DeserializeObject<List<RolesSeedModel>>(json);
                }

                listOfRoles = seedData.Select(x => new IdentityRole()
                {
                    Name = x.Role

                }).ToList();

                foreach (IdentityRole role in listOfRoles)
                {
                    await RoleManager.CreateAsync(role);
                    Context.SaveChanges();
                }

            }

            Logger.LogInformation("Roles have been seeded successfully: ", listOfRoles);

            return new JsonResult(new { message = "Roles have been seeded successfully.", Roles = listOfRoles });
        }

        //Signup
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Signup([FromBody] RegistrationViewModel model)
        {
            Logger.LogInformation("Signup action has been requested.");

            if (!ModelState.IsValid)
            {
                return new OkObjectResult(new SignupResponseModel()
                {
                    Content = new { },
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "bad_request",
                    Description ="Username or password cannot be empty"
                });
            }

            string userEmail = model.Email.Trim();
            if (await AccountService.UserExistByUserName(userEmail))
            {
                Logger.LogWarning("The supplyed user already exists");

                return new JsonResult(new SignupResponseModel()
                {
                    Content = new { },
                    StatusCode = HttpStatusCode.Conflict,
                    Error = "user_exists",
                    Description = "User already exists. Please use another email."
                });
            }

            string userRole = model.Role.Trim();
            if (!await AccountService.RoleExists(userRole))
            {
                Logger.LogWarning("Role to add to user does not exists");

                return new JsonResult(new SignupResponseModel()
                {
                    Content = new { },
                    StatusCode = HttpStatusCode.NotFound,
                    Error = "role_not_found",
                    Description = "Role is not found."
                });
            }

            string password = model.Password.Trim();
            User userIdentity = Mapper.Map<User>(model);
            IdentityResult addUserResult = await AccountService.CreateUser(userIdentity, password);
            if (!addUserResult.Succeeded)
            {
                Logger.LogWarning("Creating a user account faild");

                return new JsonResult(new SignupResponseModel()
                {
                    Content = new { },
                    StatusCode = HttpStatusCode.UnprocessableEntity,
                    Error = "unprocessable_entity",
                    Description = "User could not be created."
                });
            }

            IdentityResult addRoleResult = await AccountService.AddRoleToUser(userIdentity, userRole);
            if (!addRoleResult.Succeeded)
            {
                Logger.LogWarning("Linking role to user faild");

                return new JsonResult(new SignupResponseModel()
                {
                    Content = new { },
                    StatusCode = HttpStatusCode.UnprocessableEntity,
                    Error = "unprocessable_entity",
                    Description = "Unable to link role to user."
                });
            }

            await Context.SaveChangesAsync();

            IList<string> userRoles = await AccountService.GetRolesForUser(userIdentity);

            EndUser endUser = Mapper.Map<EndUser>(userIdentity);

            Logger.LogInformation("User has successfully been created and role has been linked");

            return new OkObjectResult(new SignupResponseModel()
            {
                Content = new { user = endUser, userRoles = userRoles },
                StatusCode = HttpStatusCode.OK,
                Error = "no_error",
                Description = "User has successfully been created."
            });
        }

        //Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Logger.LogInformation("Login action has been requested.");

            if (!ModelState.IsValid)
            {
                Logger.LogWarning("Bad request, ModelState is invalid");

                return new OkObjectResult(new LoginResponseModel()
                {
                    Content = new { },
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = "bad_request",
                    Description = "Bad Request."
                });
            }

            ClaimsIdentity identity = await JwtService.GetClaimsIdentity(model.UserName, model.Password);
            if (identity == null)
            {
                Logger.LogWarning("Faild to get user claims.");

                return new JsonResult(new LoginResponseModel()
                {
                    Content = new { },
                    StatusCode = HttpStatusCode.Unauthorized,
                    Error = "login_failure",
                    Description = "Faild to login. Please verify your username or password."

                });
            }

            string jwtResponse = await JwtService
                .GenerateJwt(identity, JwtService, model.UserName, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });

            Logger.LogInformation("JwtToken has succesfully been created");

            return new OkObjectResult(jwtResponse);
        }
    }
}