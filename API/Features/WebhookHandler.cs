namespace GRPP.API.Features;

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;

public class WebhookHandler
{
    // // int threads = Environment.ProcessorCount; // just in case we ever need to modify this to optimize it for better-suited systems in the future, i'll keep this here - as i access this file often.
    private static readonly HttpClient Client  = new HttpClient();
    
    public async Task UseWebhook(string webhookNameToUse, string webhookUrl, string arg3, string arg4, string description, string title, string color, bool inline, bool timestamps, bool experimental = false)
    {
        if (timestamps && !experimental)
        {
            var embed = new
            {
                username = webhookNameToUse,
                embeds = new[]
                {
                    new
                    {
                        title,
                        description,
                        color = uint.Parse(color),
                        fields = new[] 
                        {
                            new { name = arg3, value = arg4, inline},
                        },
                        timestamp = DateTime.UtcNow.ToString("O"),
                    } 
                }
            };
            await PostWebhook(webhookUrl, embed);
        }
        else if (!experimental)
        {
            var embed = new
            {
                username = webhookNameToUse,
                embeds = new[]
                {
                    new
                    {
                        title,
                        description,
                        color = uint.Parse(color),
                        fields = new[]
                        {
                            new { name = $"{arg3}", value = $"{arg3}", inline},
                        }
                    }
                }
            };
            await PostWebhook(webhookUrl, embed);
        }

        if (experimental)
        {
            var embed = new
            {
                username = webhookNameToUse,
                embeds = new[]
                {
                    new
                    {
                        title,
                        description,
                        color = uint.Parse(color),
                    }
                }
            };
            await PostWebhook(webhookUrl, embed);
        }
    }

    private static async Task PostWebhook(string url, object payload)
    {
        var jsonPayload = JsonConvert.SerializeObject(payload);

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await Client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
                Log.Info("WOOO SUCCESS ON WEBHOOK!!!");
            else
                Log.Error(
                    $"Failed. \"{response.ReasonPhrase}\", {response.StatusCode}, {await response.Content.ReadAsStringAsync()}"); // $""" text """ // MAY be more reasonable?? so edit this whenever if you think it's better !
        }
        catch (HttpRequestException e)
        {
            Log.Error($"HTTP Request Exception. \"{e.Message}\", Stacktrace: \"{e.StackTrace}\"");
        }
        catch (TaskCanceledException e)
        {
            Log.Error($"Task failed to complete its course. \"{e.Task}\", \"{e.Message}\"");
        }
        catch (Exception exception)
        {
            Log.Debug($"General exception. {exception}");
        }
        finally
        {
            Log.Info("Requested. Cleaning up.."); // nothing to clean up rn, but good to know there's a `finally` !!!
        }
    }
}