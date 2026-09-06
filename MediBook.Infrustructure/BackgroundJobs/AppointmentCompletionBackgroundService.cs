using MediBook.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.BackgroundJobs
{
    public class AppointmentCompletionBackgroundService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();

                    var pastAppointments = await appointmentRepository.GetConfirmedAppointmentsPastEndTimeAsync(
                        DateTimeOffset.UtcNow, stoppingToken);

                    foreach (var appointment in pastAppointments)
                    {
                        appointment.TryComplete();
                    }

                    if (pastAppointments.Count > 0)
                    {
                        await appointmentRepository.UpdateRangeAsync(pastAppointments, stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
