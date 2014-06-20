﻿using System;
using System.Diagnostics;
using DalSoft.Azure.Common.ServiceBus.Topic;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.Azure.Common.Test.Integration.ServiceBus.Topic
{
    /* If these intergation tests become too slow you can use one queue per test so that you can run in parallel without the locking. */
    [TestFixture]
    public class TopicTests
    {
        private static readonly object Lock = new object();
        private const int TestTimeout = 1500;
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

        [SetUp]
        public void SetUp()
        {
            Monitor.Enter(Lock); //Access the TestQueue one test at a time
        }

        [TearDown]
        public void TearDown()
        {
           new Topic<TestTopic>(ConnectionString, Guid.NewGuid().ToString()).DeleteTopic(); //Ensure queue is delete at the end of each test
           Monitor.Exit(Lock); //Access the TestQueue one test at a time
        }

        [Test]
        public async void Subscribe_ProvidedWithMessage_MessageIsReceivedByEachSubscriberAndRemoved()
        {
            TestMessage receievedMessage = null;
            var receivedCount = 0;

            var topic = new Topic<TestTopic>(ConnectionString, Guid.NewGuid().ToString());
            topic.Subscribe(async message => //First Subscriber
            {
                receievedMessage = message;
                receivedCount++;
                await Task.FromResult(0);
            }, new CancellationTokenSource(TestTimeout)); //Give it time to process the message

            var topic2 = new Topic<TestTopic>(ConnectionString, Guid.NewGuid().ToString());
            topic2.Subscribe(async message => //second Subscriber
            {
                receievedMessage = message;
                receivedCount++;
                await Task.FromResult(0);
            }, new CancellationTokenSource(TestTimeout)); 
            
            await topic2.Publish(new TestMessage { Id = 1, Name = "My Test" });
            await Task.Delay(TestTimeout * 2); //Give it time to process the message
            
            Assert.That(receivedCount, Is.EqualTo(2));
            Assert.That(receievedMessage.Id, Is.EqualTo(1));
            Debug.WriteLine(NamespaceManager.CreateFromConnectionString(ConnectionString).GetSubscription(topic.TopicName, topic.SubscriptionId).MessageCount);
            Debug.WriteLine(NamespaceManager.CreateFromConnectionString(ConnectionString).GetSubscription(topic2.TopicName, topic2.SubscriptionId).MessageCount);

            topic.Dispose();
            topic2.Dispose();
        }
    }
}
