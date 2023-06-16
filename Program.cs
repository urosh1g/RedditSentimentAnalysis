// See https://aka.ms/new-console-template for more information
using Reddit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using VaderSharp;

SentimentIntensityAnalyzer sentimentAnalyzer = new SentimentIntensityAnalyzer();

IConfiguration config = new ConfigurationBuilder().AddJsonFile(Path.Combine(
    Directory.GetCurrentDirectory(),
    "appsettings.json"
)).AddEnvironmentVariables().Build();

var redditClient = new RedditClient(appId: config["Reddit:client-id"], refreshToken: config["Reddit:refresh-token"], accessToken: config["Reddit:access-token"], appSecret: config["Reddit:secret"]);
Console.WriteLine(redditClient.Account.Me.Name);
var sub = redditClient.Subreddit("confessions");
var comments = sub.Comments.GetComments();
var results = new List<SentimentAnalysisResults>();
for(int i = 0; i < comments.Count; i++) {
    Console.WriteLine($"Author: {comments[i].Author}\nText: {comments[i].Body}");
    var result = sentimentAnalyzer.PolarityScores(comments[i].Body);
    results.Add(result);
    Console.WriteLine($"Positive: {result.Positive}\nNegative: {result.Negative}\nNeutral: {result.Neutral}\nCompount: {result.Compound}");
}
