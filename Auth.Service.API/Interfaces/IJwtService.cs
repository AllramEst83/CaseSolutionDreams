using Auth.Service.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auth.Service.API.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity);
        ClaimsIdentity GenerateClaimsIdentity(string userName, string id, List<string> roles);
        Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password);
        Task<string> GenerateJwt(ClaimsIdentity identity, IJwtService jwtService, string userName, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings);
    }
}
