﻿// ==========================================================================
//  ConsoleLogProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log.Internal
{
    public class ConsoleLogProcessor : IDisposable
    {
        private readonly IConsole console;
        private const int MaxQueuedMessages = 1024;
        private readonly BlockingCollection<LogMessageEntry> messageQueue = new BlockingCollection<LogMessageEntry>(MaxQueuedMessages);
        private readonly Task outputTask;

        public ConsoleLogProcessor()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                console = new WindowsLogConsole(false);
            }
            else
            {
                console = new AnsiLogConsole(false);
            }

            outputTask = Task.Factory.StartNew(ProcessLogQueue, this, TaskCreationOptions.LongRunning);
        }

        public void EnqueueMessage(LogMessageEntry message)
        {
            messageQueue.Add(message);
        }

        private void ProcessLogQueue()
        {
            foreach (var entry in messageQueue.GetConsumingEnumerable())
            {
                console.WriteLine(entry.IsError, entry.Message);
            }
        }

        private static void ProcessLogQueue(object state)
        {
            var processor = (ConsoleLogProcessor)state;

            processor.ProcessLogQueue();
        }

        public void Dispose()
        {
            messageQueue.CompleteAdding();

            try
            {
                outputTask.Wait(1500);
            }
            catch (TaskCanceledException)
            {
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            {
            }
        }
    }
}
