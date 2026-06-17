namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class PostFeedQueues
{
    public const string FeedPostCreated = "feed.post.created.queue";
    
    public const string FeedPostDeleted = "feed.post.deleted.queue";

    public const string FeedPostUpdatedAdded = "feed.post.updated.added.queue";

    public const string FeedPostUpdatedRemoved = "feed.post.updated.removed.queue";
}