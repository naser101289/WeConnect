using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Status
    {
        public Status()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public DateTime TimeOfUpdate { get; set; }

        public string StatusUpdate { get; set; }

        public string UpdatedByFullName { get; set; }

        public Guid UserWhomStatusBelongsTo { get; set; }
    }
}