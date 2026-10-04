# lib-openbao-helper
An annotation based helper for injecting open bao configurations into a .net app

### Benefits
Allows you to quickly map openbao variables to dependency injection registered classes in c#.  
Every field must exist or the app throws on startup

### Usage

Prerequsite Env vars
```json
BAO_ADDR // The app url
BAO_PATH // The route for your secrets within openbao

// Choose one authentication scheme
// Either Role/Secret
BAO_ROLE_ID
BAO_SECRET_ID

// Or Username/Password
BAO_USERNAME
BAO_PASSWORD
```

Sample settings files
```csharp
[OpenBaoSection("Example")]
public class ExampleSettings
{
    public string Greeting { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public int RetryCount { get; set; }
}
```

In your startup class
```csharp
builder.AddOpenBaoConfiguration();
```

In OpenBao use json data
```json
{
  "Example": {
    "ApiKey": "here's my \"key\"",
    "RetryCount": 5,
    "greeting": "salutations"
  }
}
```