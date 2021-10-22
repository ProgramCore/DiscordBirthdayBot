using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BirthdayBot.Models;

namespace BirthdayBot.Modules
{
    public class Entertainment : ModuleBase
    {
        private readonly IConfiguration config;

        public Entertainment(IServiceProvider _provider)
        {
            config = _provider.GetService(typeof(IConfiguration)) as IConfiguration;
        }


        /*[Command("meme")]
        [Alias("reddit")]
        public async Task Meme(string subr = null)
        {
            var client = new HttpClient();
            var data = await client.GetStringAsync($"https://reddit.com/r/{subr ?? "dankmemes"}/random.json?limit=1?obey_over18=true");
            
            if(!data.StartsWith('['))
            {
                await Context.Channel.SendMessageAsync("This subreddit does not exist");
                return;
            }
            
            var jar = JArray.Parse(data);
            JObject post = JObject.Parse(jar[0]["data"]["children"][0]["data"].ToString());

            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithTitle(post["title"].ToString())
                .WithUrl($"https://reddit.com{post["permalink"]}")
                .WithFooter($"🗨 {post["num_comments"]}   |   👍 {post["ups"]}");

            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }*/

        /*[Command("gif")]
        [Alias("giphy")]
        public async Task Giphy(params string[] arr)
        {
            var client = new HttpClient();
            string data = null;
            string query = null;
            string imageurl = null;
            string url = null;
            string token = config["tokens:giphy"];

            if (arr.Length == 0)
            {
                query = string.Empty;
                data = await client.GetStringAsync($"https://api.giphy.com/v1/gifs/random?api_key={token}&tag=&rating=g");
                JObject images = JObject.Parse(data);
                imageurl = images["data"]["images"]["original"]["url"].ToString();
                url = images["data"]["url"].ToString();
            }
            else
            {
                var rand = new Random().Next(0, 25);

                query = string.Join("+", arr);
                data = await client.GetStringAsync($"https://api.giphy.com/v1/gifs/search?api_key={token}&q={query}&limit=1&offset={rand}&rating=g&lang=en");
                query = query.Replace("+", " ");
                JObject images = JObject.Parse(data);
                imageurl = images["data"][0]["images"]["original"]["url"].ToString();
                url = images["data"][0]["url"].ToString();
            }

            var builder = new EmbedBuilder()
                .WithImageUrl(imageurl)
                .WithTitle(query)
                .WithUrl(url)
                .WithFooter("Powered by GIPHY");

            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }*/

        public static async Task<GiphyData> GetBirthdayUrls(string token)
        {
            var giphy = new GiphyData();
            var client = new HttpClient();
            string data = null;
            var rand = new Random().Next(0, 25);

            try
            {
                data = await client.GetStringAsync($"https://api.giphy.com/v1/gifs/search?api_key={token}&q=happy+birthday&limit=1&offset={rand}&rating=g&lang=en");
            }
            catch (HttpRequestException)
            {
                data = null;
                giphy.URL = giphy.ImageURL = string.Empty;
            }


            if(data != null)
            {
                JObject image = JObject.Parse(data);
                giphy.ImageURL = image["data"][0]["images"]["original"]["url"].ToString();
                giphy.URL = image["data"][0]["url"].ToString();
            }

            return giphy;
        }

        [Command("pick")]
        public async Task Pick(params string[] arr)
        {
            if(arr.Length == 0)
            {
                var prefix = config["prefix"];
                await Context.Channel.SendMessageAsync($"🚫 To have me pick something for you, execute [{prefix}pick <\"option1\"> <\"option2\"> <\"option3\">] or however many you would like. Be sure to wrap each option in \"\" marks ");
                return; 
            }

            if(arr.Length == 1)
            {
                await Context.Channel.SendMessageAsync("Are you really asking me this with only one choice? You're just like my Mother Bot.");
                return;
            }

            await Context.Channel.SendMessageAsync($"I think you should go with **{arr[new Random().Next(0, arr.Length)]}**");
        }
    }
}
