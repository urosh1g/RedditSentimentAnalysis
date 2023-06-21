namespace RedditProviders;

using Reddit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

public class RedditProvider
{
    static RedditClient? client;
    static IConfiguration? config;
    static object locker = new object();
    private RedditProvider() {}
    public static RedditClient GetProvider()
    {
        lock (locker)
        {
            if (client == null)
            {
                config = new ConfigurationBuilder().AddJsonFile(Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "appsettings.json"
                )).AddEnvironmentVariables().Build();
                client = new RedditClient(appId: config["Reddit:client-id"], refreshToken: config["Reddit:refresh-token"], accessToken: config["Reddit:access-token"], appSecret: config["Reddit:secret"]);
            }
        }
        return client;
    }
}