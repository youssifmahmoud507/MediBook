using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);
        Task<Stream?> GetFileAsync(string storedFileName, CancellationToken cancellationToken);
    }
}
