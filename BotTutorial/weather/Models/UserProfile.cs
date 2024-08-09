using Microsoft.Bot.Schema;

namespace WeatherBot.Models;

public class UserProfile
{
    public string Transport { get; set; }

    public string Name { get; set; }

    public int Age { get; set; }

    public Attachment Picture { get; set; }
}