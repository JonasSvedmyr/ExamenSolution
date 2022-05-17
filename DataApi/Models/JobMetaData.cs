using System;

namespace DataApi.Models
{
    public class JobMetaData
    {
        public Guid Id { get; set; }
        public Guid Trigger_Id { get; set; }
        public Type JobType { get; }
        public string JobName { get; }
        public string CronExpression { get; }
        public JobMetaData(Guid guid, Guid trigger_id, Type jobType, string jobName, string cronExpression)
        {
            Id = guid;
            JobType = jobType;
            JobName = jobName;
            CronExpression = cronExpression;
            Trigger_Id = trigger_id;
        }
    }
}
