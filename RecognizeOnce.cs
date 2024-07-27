using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class RecognizeOnce
{
    public static async Task Test()
    {
        var builder = new ConfigurationBuilder();
        builder.AddUserSecrets<Program>();

        var configuration = builder.Build();

        string azureKey = configuration.GetSection("kk-ig-ai:apiKey")?.Value ?? throw new NullReferenceException("missing app config");
        string azureLocation = "eastus";
        var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);
        await FromSoundFile(speechConfig);
        await FromMic(speechConfig);
    }

    async static Task FromMic(SpeechConfig speechConfig)
    {
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        Console.WriteLine("Speak into your microphone.");
        var result = await speechRecognizer.RecognizeOnceAsync();
        Console.WriteLine($"RECOGNIZED: Text={result.Text}");
    }

    async static Task FromSoundFile(SpeechConfig speechConfig)
    {
        string textFile = "Shakespeare.txt";
        string waveFile = "Shakespeare.wav";

        try
        {
            FileInfo fileInfo = new FileInfo(waveFile);
            if (fileInfo.Exists)
            {
                Console.WriteLine("Speech recognition started.");
                using var audioConfig = AudioConfig.FromWavFileInput(fileInfo.FullName);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                var result = await speechRecognizer.RecognizeOnceAsync();

                FileStream fileStream = File.OpenWrite(textFile);
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                streamWriter.WriteLine(result.Text);
                streamWriter.Close();
                Console.WriteLine("Speech recognition stopped.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}