using MediBook.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[FAKE EMAIL] To: {toEmail} | Subject: {subject} | Body: {body}");
            return Task.CompletedTask;
        }
    }
}
