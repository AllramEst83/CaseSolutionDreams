using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Service.API.ResponseModels
{
    public class JwtToken
    {
        public string Id { get; set; }
        public string Auth_Token { get; set; }
        public int Expires_In { get; set; }


    }
}
