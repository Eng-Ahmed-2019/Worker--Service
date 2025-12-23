using Quartz;
using MyWorkerService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DoPaymentJob");
    q.AddJob<MyJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DoPaymentJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(1)
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();