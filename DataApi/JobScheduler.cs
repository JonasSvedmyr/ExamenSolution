using DataApi.Job;
using DataApi.Models;
using Microsoft.Extensions.Hosting;
using Quartz;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DataApi
{
    public class JobScheduler : IHostedService
    {
        public IScheduler Scheduler { get; set; }

        public JobScheduler(IScheduler scheduler)
        {
            Scheduler = scheduler;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var jobMetaData = new JobMetaData(Guid.NewGuid(), Guid.NewGuid(), typeof(NewsAPIJob), "GoogleNews", "0 0/1 * * * ?");
            IJobDetail jobDetail = CreateJob(jobMetaData);
            ITrigger trigger = CreateTrigger(jobMetaData);
            await Scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
            Debug.WriteLine("starting jobs");
            await Scheduler.Start(cancellationToken);

        }
        private ITrigger CreateTrigger(JobMetaData jobMetaData)
        {
            return TriggerBuilder.Create().WithIdentity(jobMetaData.Trigger_Id.ToString()).WithCronSchedule(jobMetaData.CronExpression).WithDescription(jobMetaData.JobName).Build();
        }
        private IJobDetail CreateJob(JobMetaData jobMetaData)
        {
            return JobBuilder.Create(jobMetaData.JobType).WithIdentity(jobMetaData.Id.ToString()).WithDescription(jobMetaData.JobName).Build();
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
