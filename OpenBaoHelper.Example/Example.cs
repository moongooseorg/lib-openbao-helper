using Microsoft.Extensions.Logging;

namespace MyTest;

public class Example(ExampleSettings settings, ILogger<Example> logger)
{
    public void Main()
    {
        foreach (var property in typeof(ExampleSettings).GetProperties())
        {
            logger.LogInformation("{Property}: {Value}", property.Name, property.GetValue(settings));
        }
    }
}
