using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OpenBaoHelper;

public static class ConfigurationRegistration
{
    public static IHostApplicationBuilder AddOpenBaoConfiguration(this IHostApplicationBuilder builder) =>
        builder.AddOpenBaoConfiguration(Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException("Unable to determine the entry assembly."));

    public static IHostApplicationBuilder AddOpenBaoConfiguration(
        this IHostApplicationBuilder builder,
        params Assembly[] assemblies)
    {
        builder.Configuration.AddOpenBao(builder.Configuration);

        Type[] types = assemblies.SelectMany(assembly => assembly.GetTypes()).ToArray();
        HashSet<Type> settingsTypes = [];

        foreach (Type type in types)
        {
            if (type.GetCustomAttribute<OpenBaoSectionAttribute>() is not { } attribute)
                continue;

            IConfigurationSection section = builder.Configuration.GetSection(attribute.Name);

            RequireAllKeys(type, section);

            object settings = section.Get(type) ?? Activator.CreateInstance(type)!;

            builder.Services.AddSingleton(type, settings);

            builder.Services.AddSingleton(
                typeof(IOptions<>).MakeGenericType(type),
                Activator.CreateInstance(typeof(OptionsWrapper<>).MakeGenericType(type), settings)!);

            settingsTypes.Add(type);
        }

        foreach (Type type in types.Where(type => ConsumesSettings(type, settingsTypes)))
            builder.Services.TryAddSingleton(type);

        return builder;
    }

    private static bool ConsumesSettings(Type type, HashSet<Type> settingsTypes) =>
        type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
        && !settingsTypes.Contains(type)
        && type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is null
        && type.GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Any(parameterType => settingsTypes.Contains(parameterType)
                || parameterType.IsGenericType
                && parameterType.GetGenericTypeDefinition() == typeof(IOptions<>)
                && settingsTypes.Contains(parameterType.GetGenericArguments()[0]));

    private static void RequireAllKeys(Type type, IConfigurationSection section)
    {
        HashSet<string> present = section
            .GetChildren()
            .Select(child => child.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        string[] missing = type
            .GetProperties()
            .Select(property => property.Name)
            .Where(name => !present.Contains(name))
            .ToArray();

        if (missing.Length > 0)
            throw new InvalidOperationException(
                $"Configuration section '{section.Key}' is missing key(s): {string.Join(", ", missing)}");
    }
}
