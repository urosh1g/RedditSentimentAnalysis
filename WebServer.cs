using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Newtonsoft.Json;
using System.Text;
using VaderSharp;

using Extensions;

namespace Webserver;

public class WebServer
{
    string address;
    short port;
    HttpListener listener;

    public WebServer(string address, short port = 5050)
    {
        this.address = address;
        this.port = port;
        listener = new HttpListener();
        listener.Prefixes.Add($"http://{address}:{port}/");
    }

    private void LogRequest(HttpListenerRequest request)
    {
        Console.Write($"{request.HttpMethod} {request.RawUrl}");
        Console.WriteLine(request.Headers);
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine($"Server started listening at http://{address}:{port}");
        while (true)
        {
            var context = listener.GetContext();
            ThreadPool.QueueUserWorkItem((state) =>
            {
                HandleRequest(context);
            });
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;
        int numComments;
        if (!int.TryParse(request.QueryString["numComments"], out numComments))
        {
            numComments = 10;
        }
        string[] subreddits;
        Dictionary<string, List<SentimentAnalysisResults>> comments = new Dictionary<string, List<SentimentAnalysisResults>>();
        CountdownEvent countdownEvent;
        SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();

        try
        {
            LogRequest(context.Request);
            var querySubs = request.QueryString["subreddits"];
            if(querySubs == null){
                throw new Exception("No subreddits provided!");
            }
            subreddits = querySubs.Split(',');
            countdownEvent = new CountdownEvent(subreddits.Length);
            foreach (string subreddit in subreddits)
            {
                CommentsObservable commentsStream = new CommentsObservable(subreddit, numComments);
                var observer = Observer.Create<SentimentAnalysisResults>(
                    (comment) =>
                    {
                        if (!comments.ContainsKey(subreddit))
                        {
                            comments.Add(subreddit, new List<SentimentAnalysisResults>());
                        }
                        comments[subreddit].Add(comment);
                    },
                    (error) =>
                    {
                        Console.WriteLine($"{subreddit} Error {error.Message}");
                    },
                    () =>
                    {
                        Console.WriteLine($"{subreddit} Completed!");
                        countdownEvent.Signal();
                    });
                commentsStream.SubscribeOn(Scheduler.NewThread)
                .Select(comment => comment.Body)
                .Select(commentBody =>
                {
                    var result = analyzer.PolarityScores(commentBody);
                    return result;
                })
                .Subscribe(observer);
            }
            countdownEvent.Wait();
            var bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(comments, Formatting.Indented));
            response.ContentLength64 = bytes.Length;
            response.ContentType = "application/json";
            response.OutputStream.Write(bytes);
        }
        catch (Exception ex)
        {
            response.OutputStream.Write(Encoding.ASCII.GetBytes(ex.Message));
        }
        finally{
            response.OutputStream.Close();
        }
    }
}