using Microsoft.Extensions.Configuration;

namespace OpenBaoHelper;

public static class OpenBaoConfigurationExtensions
{
    public static IConfigurationBuilder AddOpenBao(this IConfigurationBuilder builder, IConfiguration bootstrap)
    {
        string address = NonEmpty(bootstrap["BAO_ADDR"])
            ?? throw new InvalidOperationException("BAO_ADDR is not set.");

        string path = NonEmpty(bootstrap["BAO_PATH"])
            ?? throw new InvalidOperationException("BAO_PATH is not set.");

        string? roleId = NonEmpty(bootstrap["BAO_ROLE_ID"]);
        string? secretId = NonEmpty(bootstrap["BAO_SECRET_ID"]);
        string? username = NonEmpty(bootstrap["BAO_USERNAME"]);
        string? password = NonEmpty(bootstrap["BAO_PASSWORD"]);

        bool appRole = roleId is not null || secretId is not null;
        bool userPass = username is not null || password is not null;

        if (appRole == userPass)
            throw new InvalidOperationException(
                "Set either BAO_ROLE_ID and BAO_SECRET_ID, or BAO_USERNAME and BAO_PASSWORD.");

        if (appRole && (roleId is null || secretId is null))
            throw new InvalidOperationException("BAO_ROLE_ID and BAO_SECRET_ID must both be set.");

        if (userPass && (username is null || password is null))
            throw new InvalidOperationException("BAO_USERNAME and BAO_PASSWORD must both be set.");

        return builder.Add(new OpenBaoConfigurationSource
        {
            Address = address,
            Path = path,
            MountPoint = NonEmpty(bootstrap["BAO_MOUNT"]) ?? "kv",
            RoleId = roleId,
            SecretId = secretId,
            Username = username,
            Password = password,
            UserPassMountPoint = NonEmpty(bootstrap["BAO_USERPASS_MOUNT"]) ?? "userpass",
        });
    }

    private static string? NonEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
