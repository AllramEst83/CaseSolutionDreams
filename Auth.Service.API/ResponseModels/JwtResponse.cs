using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Auth.Service.API.ResponseModels
{
    public class JwtResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Error { get; set; }
        public string Description { get; set; }
        public JwtToken Token { get; set; }
    }
}
