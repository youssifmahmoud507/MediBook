using MediBook.Application.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            var configuredPath = configuration["FileStorage:BasePath"]!;
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), configuredPath);

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(fileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(_basePath, storedFileName);

            using (var outputStream = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream, cancellationToken);
            }

            return storedFileName;
        }

        public Task<Stream?> GetFileAsync(string storedFileName, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_basePath, storedFileName);

            if (!File.Exists(fullPath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream?>(stream);
        }
    }
}

