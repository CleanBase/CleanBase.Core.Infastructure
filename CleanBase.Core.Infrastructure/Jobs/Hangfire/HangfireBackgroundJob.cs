using CleanBase.Core.Services.Jobs;
using Hangfire;
using Hangfire.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Jobs.Hangfire
{
    public class HangfireBackgroundJob : IBackgroundJob
    {
        private readonly IBackgroundJobClient _client;

        public HangfireBackgroundJob(IBackgroundJobClient client)
        {
            _client = client;
        }

        public string Enqueue(Expression<Action> action)
        {
            return _client.Enqueue(action);
        }

        public string Enqueue(Expression<Func<Task>> action)
        {
            return _client.Enqueue(action);
        }

        public void Recurring<TService>(
            string jobCode,
            Expression<Func<TService, Task>> function,
            string cron,
            TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            var options = new RecurringJobOptions
            {
                TimeZone = timeZone ?? TimeZoneInfo.Local,
                QueueName = queue
            };

            RecurringJob.AddOrUpdate(jobCode, function, cron, options);
        }

        public void Recurring(
            string jobCode,
            Expression<Func<Task>> function,
            string cron,
            TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            var options = new RecurringJobOptions
            {
                TimeZone = timeZone ?? TimeZoneInfo.Local,
                QueueName = queue
            };

            RecurringJob.AddOrUpdate(jobCode, function, cron, options);
        }
        public void Recurring(
            string jobCode,
            Expression<Action> function,
            string cron,
            TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            var options = new RecurringJobOptions
            {
                TimeZone = timeZone ?? TimeZoneInfo.Local,
                QueueName = queue
            };

            RecurringJob.AddOrUpdate(jobCode, function, cron, options);
        }

        // Schedule methods to schedule jobs with delays or specific times
        public string Schedule([InstantHandle, NotNull] Expression<Action> methodCall, TimeSpan delay)
        {
            return _client.Schedule(methodCall, delay);
        }

        public string Schedule([NotNull, InstantHandle] Expression<Func<Task>> methodCall, TimeSpan delay)
        {
            return _client.Schedule(methodCall, delay);
        }

        public string Schedule([InstantHandle, NotNull] Expression<Action> methodCall, DateTimeOffset enqueueAt)
        {
            return _client.Schedule(methodCall, enqueueAt);
        }

        public string Schedule([InstantHandle, NotNull] Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
        {
            return _client.Schedule(methodCall, enqueueAt);
        }

        public string Schedule<T>([NotNull, InstantHandle] Expression<Action<T>> methodCall, TimeSpan delay)
        {
            return _client.Schedule(methodCall, delay);
        }

        public string Schedule<T>([InstantHandle, NotNull] Expression<Func<T, Task>> methodCall, TimeSpan delay)
        {
            return _client.Schedule(methodCall, delay);
        }

        public string Schedule<T>([InstantHandle, NotNull] Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
        {
            return _client.Schedule(methodCall, enqueueAt);
        }

        public string Schedule<T>([NotNull, InstantHandle] Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        {
            return _client.Schedule(methodCall, enqueueAt);
        }

        public void RemoveJob(string jobId)
        {
            RecurringJob.RemoveIfExists(jobId);
        }

        public void Trigger(string jobCode)
        {
            RecurringJob.Trigger(jobCode);
        }

        public void CancelSchedule(string jobCode)
        {
            BackgroundJob.Delete(jobCode);
        }

        public string GetJobStatus(string jobCode)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var job = connection.GetJobData(jobCode);
                return job == null ? "Job not found" : job.State;
            }
        }

        public void PauseJob(string jobCode)
        {
            throw new NotSupportedException("Pausing a job is not supported.");
        }

        public void ResumeJob(string jobCode)
        {
            // Hangfire doesn't have built-in resume functionality.
            // You may need to re-add the job with the same details.
            // You can implement logic to re-schedule the job if you have its details stored.
            throw new NotSupportedException("Resuming a job is not supported.");
        }

        public void UpdateSchedule(string jobCode, TimeSpan newDelay)
        {
            // Remove the existing job
            BackgroundJob.Delete(jobCode);

            // Re-schedule the job with new delay
            // Assuming you have a way to re-create the job with its original details
            // Example: you might need to store job details somewhere to re-add it
            // Here, we're just providing a placeholder implementation.
            // In practice, you need to handle job details appropriately.
            throw new NotSupportedException("Updating a job schedule is not supported directly.");
        }
    }
}
