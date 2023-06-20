using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace Webserver;

public class WebServer {
    string address;
    short  port;
    HttpListener listener;

    public WebServer(string address, short port = 5050) {
        this.address = address;
        this.port = port;
        listener = new HttpListener();
        listener.Prefixes.Add($"http://{address}:{port}/");
    }

    private void LogRequest(HttpListenerRequest request){
        Console.Write($"{request.HttpMethod} {request.RawUrl}");
        Console.WriteLine(request.Headers);
    }

    public void Start() {
        listener.Start();
        Console.WriteLine($"Server started listening at http://{address}:{port}");
        while(true) {
            var context = listener.GetContext();
            ThreadPool.QueueUserWorkItem((state) => {
                HandleRequest(context);
            });
        }
    }

    private void HandleRequest(HttpListenerContext context) {
        var response = context.Response;
        string[] subreddits;

        LogRequest(context.Request);
        subreddits = context.Request.RawUrl!.Split(',');
        subreddits[0] = subreddits[0].Substring(1);
        response.OutputStream.Close();
    }

}