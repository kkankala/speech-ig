using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class ContinuousSpeech
{
    static async Task Test()
    {
        var builder = new ConfigurationBuilder();
        builder.AddUserSecrets<Program>();

        var configuration = builder.Build();

        string azureKey = configuration.GetSection("kk-ig-ai:apiKey")?.Value ?? throw new NullReferenceException("missing app config");
        string azureLocation = "eastus";

        var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);
        await FromFile(speechConfig);
    }


    async static Task FromFile(SpeechConfig speechConfig)
    {

        string textFile = "Shakespeare.txt";
        string waveFile = "Shakespeare.wav";
        try
        {
            FileInfo fileInfo = new FileInfo(waveFile);
            if (fileInfo.Exists)
            {


                using var audioConfig = AudioConfig.FromWavFileInput(fileInfo.FullName);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

                var stopRecognition = new TaskCompletionSource<int>();
                FileStream fileStream = File.OpenWrite(textFile);
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                var phraseList = PhraseListGrammar.FromRecognizer(speechRecognizer);
                phraseList.AddPhrase("thou seest");

                speechRecognizer.Recognized += (s, e) =>
                {
                    switch (e.Result.Reason)
                    {
                        case ResultReason.RecognizedSpeech:
                            streamWriter.WriteLine(e.Result.Text);
                            break;
                        case ResultReason.NoMatch:
                            Console.WriteLine("Speech could not be recognized.");
                            break;
                    }
                };

                speechRecognizer.Canceled += (s, e) =>
                {
                    if (e.Reason != CancellationReason.EndOfStream)
                    {
                        Console.WriteLine("Speech recognition canceled.");
                    }
                    stopRecognition.TrySetResult(0);
                    streamWriter.Close();
                };

                speechRecognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("Speech recognition stopped.");
                    stopRecognition.TrySetResult(0);
                    streamWriter.Close();
                };

                Console.WriteLine("Speech recognition started.");
                await speechRecognizer.StartContinuousRecognitionAsync();
                //TODO: SessionStopped not firing, check this.
                Task.WaitAny(new[] { stopRecognition.Task });
                await speechRecognizer.StopContinuousRecognitionAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


    }
}