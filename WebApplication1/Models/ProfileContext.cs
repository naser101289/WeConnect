using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class ProfileContext : DbContext
    {
        public DbSet<Profile> Profile { get; set; }
        public DbSet<Status> Status { get; set; }
    }
}