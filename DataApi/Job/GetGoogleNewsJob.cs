using Quartz;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DataApi.Job
{
    public class GetGoogleNewsJob : IJob
    {
        //Can only be called 3 times an hour or we have to pay so this will job will have a timmer of one fetch an hour
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://google-news.p.rapidapi.com/v1/top_headlines?lang=en&country=US"),
                    Headers =
                {
                    { "X-RapidAPI-Host", "google-news.p.rapidapi.com" },
                    { "X-RapidAPI-Key", "" },
                },
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadFromJsonAsync<Models.GoogleNews>();
                    Debug.WriteLine(body.feed.title);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
