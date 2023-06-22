namespace Utils;

using VaderSharp;
using Newtonsoft.Json;

public class AnalysisResult {
    [JsonProperty(Order = 1, PropertyName = "subreddit")]
    public string SubredditName { get; set; }
    [JsonProperty(Order = 2, PropertyName = "overall_result")]
    public SentimentAnalysisResults OverallResult { get {
        var res = Comments.Select(x => x.Item2)
        .Aggregate(new SentimentAnalysisResults(), (total, current) => {
            total.Compound += current.Compound;
            total.Negative += current.Negative;
            total.Neutral += current.Neutral;
            total.Positive += current.Positive;
            return total;
        });
        res.Compound /= Comments.Count;
        res.Negative /= Comments.Count;
        res.Neutral /= Comments.Count;
        res.Positive /= Comments.Count;
        res.Compound = Double.Round(res.Compound, 3);
        res.Negative = Double.Round(res.Negative, 3);
        res.Neutral = Double.Round(res.Neutral, 3);
        res.Positive = Double.Round(res.Positive, 3);
        return res;
    } }
    [JsonProperty(Order = 3, PropertyName = "comments")]
    public List<(string text, SentimentAnalysisResults result)> Comments;
    public AnalysisResult(string subName) {
        SubredditName = subName;
        Comments = new List<(string text, SentimentAnalysisResults result)>();
    }


    public void AddComment(string text, SentimentAnalysisResults result) {
        Comments.Add(new (text, result));
    }
}