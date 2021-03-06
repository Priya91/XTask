﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Metrics
{
    using XTask.Services;
    using XTask.Tasks;
    using XTask.Utility;

    /// <summary>
    /// Simple wrapper for tasks to track usage
    /// </summary>
    public class TrackedTask : ImplementedServiceProvider, IUsageTrackedTask, ITask
    {
        private IFeatureUsageTracker usageTracker;
        private ITask task;

        public TrackedTask(ITask task, int featureIdentifier, IFeatureUsageTracker usageTracker)
        {
            this.task = task;
            this.usageTracker = usageTracker;
            this.TaskFeatureIdentifier = featureIdentifier;
        }

        public int TaskFeatureIdentifier { get; private set; }

        public ExitCode Execute(ITaskInteraction interaction)
        {
            this.usageTracker.RecordUsage(this.TaskFeatureIdentifier);
            ITaskExecutor executor = this.task.GetService<ITaskExecutor>();
            if (executor != null)
            {
                return executor.Execute(interaction);
            }

            return ExitCode.Success;
        }

        public override T GetService<T>()
        {
            return base.GetService<T>() ?? this.task.GetService<T>();
        }
    }
}
