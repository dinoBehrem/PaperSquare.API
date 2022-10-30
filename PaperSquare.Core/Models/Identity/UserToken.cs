﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperSquare.Core.Models.Identity
{
    public class UserToken: IdentityUserToken<string>
    {
        public User User { get; set; }
    }
}
