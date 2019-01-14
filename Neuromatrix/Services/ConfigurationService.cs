using System;
using Microsoft.Extensions.DependencyInjection;

using Neuromatrix.Models;

namespace Neuromatrix.Services
{
    public class ConfigurationService
    {
        private readonly IServiceProvider _services;

        public ConfigurationService(IServiceProvider services)
        {
            _services = services;
        }

        public void Configure()
        {
            Configuration.Token = _services.GetRequiredService<Settings>().Token;
            Configuration.Owner = _services.GetRequiredService<Settings>().Owner;
            Configuration.Guild = _services.GetRequiredService<Settings>().Guild;
            Configuration.XurChannel = _services.GetRequiredService<Settings>().XurChannel;
            Configuration.Version = _services.GetRequiredService<Settings>().Version;
        }
    }
}
