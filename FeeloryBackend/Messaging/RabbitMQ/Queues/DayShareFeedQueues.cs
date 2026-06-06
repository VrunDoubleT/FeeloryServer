namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class DayShareFeedQueues
{
    public const string FeedPostCreated = "feed.dayshare.created.queue";
    
    public const string FeedPostDeleted = "feed.dayshare.deleted.queue";

    public const string FeedPostUpdatedAdded = "feed.dayshare.updated.added.queue";

    public const string FeedPostUpdatedRemoved = "feed.dayshare.updated.removed.queue";
}