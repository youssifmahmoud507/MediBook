using MediBook.Application.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IAttachmentService
    {
        Task<Result<Guid>> UploadAttachmentAsync(Guid medicalRecordId, IFormFile file, Guid currentUserId, CancellationToken cancellationToken);
        Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadAttachmentAsync(Guid attachmentId, Guid currentUserId, CancellationToken cancellationToken);
    }
}
