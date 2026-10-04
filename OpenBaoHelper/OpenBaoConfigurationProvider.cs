using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.UserPass;
using VaultSharp.V1.Commons;

namespace OpenBaoHelper;

public sealed class OpenBaoConfigurationProvider(OpenBaoConfigurationSource source) : ConfigurationProvider
{
    public IReadOnlyCollection<string> Keys => Data.Keys.Order(StringComparer.OrdinalIgnoreCase).ToArray();

    public override void Load() => LoadAsync().GetAwaiter().GetResult();

    private async Task LoadAsync()
    {
        IAuthMethodInfo auth = source switch
        {
            { RoleId.Length: > 0, SecretId.Length: > 0 } => new AppRoleAuthMethodInfo(source.RoleId, source.SecretId),
            { Username.Length: > 0, Password.Length: > 0 } =>
                new UserPassAuthMethodInfo(source.UserPassMountPoint, source.Username, source.Password),
            _ => throw new InvalidOperationException(
                "Set either BAO_ROLE_ID and BAO_SECRET_ID, or BAO_USERNAME and BAO_PASSWORD."),
        };

        VaultClientSettings settings = new(source.Address, auth);

        settings.PostProcessHttpClientHandlerAction = handler =>
        {
            if (handler is HttpClientHandler clientHandler)
                clientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        };

        IVaultClient client = new VaultClient(settings);

        Console.WriteLine($"OpenBao: reading secret '{source.MountPoint}/{source.Path}' from {source.Address}");

        Secret<SecretData> secret = await client.V1.Secrets.KeyValue.V2
            .ReadSecretAsync(path: source.Path, mountPoint: source.MountPoint);

        byte[] json = JsonSerializer.SerializeToUtf8Bytes(secret.Data.Data);

        IConfigurationRoot parsed = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(json))
            .Build();

        Data = parsed.AsEnumerable()
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }
}
