﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class ProfileStatusVM
    {
        public Profile Profile { get; set; }
        public List<Status> StatusList { get; set; }

        public List<Profile> ProfileCollection { get; set; }
    }
}