using Microsoft.Extensions.Configuration;

namespace OpenBaoHelper;

public sealed class OpenBaoConfigurationSource : IConfigurationSource
{
    public required string Address { get; init; }
    public required string Path { get; init; }
    public string MountPoint { get; init; } = "kv";
    public string? RoleId { get; init; }
    public string? SecretId { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string UserPassMountPoint { get; init; } = "userpass";

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new OpenBaoConfigurationProvider(this);
}
