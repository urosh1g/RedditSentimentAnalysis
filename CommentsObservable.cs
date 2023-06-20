namespace Extensions;

using Reddit;
using Reddit.Controllers;
using RedditProviders;
using System.Reactive.Disposables;

public class CommentsObservable : IObservable<Comment>
{
    string subreddit;
    RedditClient client;
    public CommentsObservable(string subreddit)
    {
        this.subreddit = subreddit;
        client = RedditProvider.GetProvider();
    }
    public IDisposable Subscribe(IObserver<Comment> observer)
    {
        List<Comment>? comments = null;
        try
        {
            comments = client.Subreddit(subreddit).Comments.GetComments(limit: 100, sort: "top");
        }
        catch (Exception ex)
        {
            observer.OnError(ex);
        }
        if (comments == null)
        {
            observer.OnError(new Exception("no comments"));
        }
        else
        {
            foreach (var comm in comments)
            {
                observer.OnNext(comm);
            }
            observer.OnCompleted();
        }
        return Disposable.Empty;
    }
}