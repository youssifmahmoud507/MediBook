using MediBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class Attachment : BaseEntity
    {
        public Guid MedicalRecordId { get; set; }
        public string FileName { get; set; } = default!;
        public string StoredFileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public DateTimeOffset UploadedAt { get; set; }
    }
}
