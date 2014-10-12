using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class ProfileInitializer : DropCreateDatabaseIfModelChanges<ProfileContext>
    {
        protected override void Seed(ProfileContext context)
        {
            //empty
        }
    }
}