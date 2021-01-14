using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tweetinvi;

namespace TwitterLikeRemover
{
    public static class TwitterLikeRemover
    {
        [FunctionName("TwitterLikeRemover")]
        public static async Task Run([TimerTrigger("0 0 0 * * 0")] TimerInfo myTimer, ILogger logger, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true) 
                .AddEnvironmentVariables() 
                .Build();
				
            var userClient = new TwitterClient(
                consumerKey:    config["twitter_consumer_key"],
                consumerSecret: config["twitter_consumer_secret"],
                accessToken:    config["twitter_consumer_access_token"],
                accessSecret:   config["twitter_consumer_access_secret"]
            );

            var user = await userClient.Users.GetAuthenticatedUserAsync();
            var favoriteTweetsIterator = userClient.Tweets.GetUserFavoriteTweetsIterator(user.Name);
            while(!favoriteTweetsIterator.Completed)
            {
                var tweets = await favoriteTweetsIterator.NextPageAsync();
                var unfavoriteTasks = new List<Task>();
                foreach(var tweet in tweets){
                    unfavoriteTasks.Add(userClient.Tweets.UnfavoriteTweetAsync(tweet));
                }
                await Task.WhenAll(unfavoriteTasks);
            }
        }
    }
}
