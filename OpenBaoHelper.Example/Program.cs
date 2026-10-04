using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyTest;
using OpenBaoHelper;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddOpenBaoConfiguration();

using IHost host = builder.Build();

host.Services.GetRequiredService<Example>().Main();
