﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Service.API.Models
{
    public class AppSettings
    {
        public string AuthDbConnectionString { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public string AuthSeedData { get; set; }
    }
}
