using OpenBaoHelper;

namespace MyTest;

[OpenBaoSection("Example")]
public class ExampleSettings
{
    public string Greeting { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public int RetryCount { get; set; }
}
