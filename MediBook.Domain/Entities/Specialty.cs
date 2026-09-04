using MediBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class Specialty : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
