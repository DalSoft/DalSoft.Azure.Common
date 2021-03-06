﻿using Microsoft.ServiceBus.Messaging;

namespace DalSoft.Azure.ServiceBus
{
    public interface INamespaceManager
    {
        bool QueueExists(string path);
        bool TopicExists(string path);
        bool SubscriptionExists(string path, string subscriptionName);
        QueueDescription CreateQueue(string path, Settings settings);
        TopicDescription CreateTopic(string path);
        SubscriptionDescription CreateSubscription(string path, string subscriptionName);
        QueueDescription GetQueue(string path);
        TopicDescription GetTopic(string path);
        void DeleteQueue(string path);
        void DeleteTopic(string path);
        void DeleteSubscription(string path, string subscriptionName);
        string ConnectionString { get; }
    }
}
