using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure;
using Azure.AI.Language.Conversations;
using Azure.Core;
using System.Text.Json;

public class ContinuousRecognition
{
    private static string SubscriptionKey = "YourSubscriptionKey";
    private static string ServiceRegion = "YourServiceRegion";
    private static string CluEndpoint = "https://your-clu-endpoint.cognitiveservices.azure.com/";
    private static string CluKey = "YourCLUKey";
    private static string OutputTextFilePath = "continuousTranscription.txt";

    public static async Task Main(string[] args)
    {
        var speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, ServiceRegion);
        using var recognizer = new SpeechRecognizer(speechConfig);

        // Set up the transcription file
        using var fileStream = new FileStream(OutputTextFilePath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(fileStream, Encoding.UTF8);

        // Event handlers for speech recognition events
        recognizer.Recognized += async (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string text = e.Result.Text;
                Console.WriteLine($"Recognized: {text}");

                // Write the recognized text to the file asynchronously
                await writer.WriteLineAsync(text);

                // Analyze the recognized text using CLU
                await AnalyzeTextWithCLU(text);
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine("No speech could be recognized.");
            }
        };

        recognizer.Canceled += (s, e) =>
        {
            Console.WriteLine($"CANCELED: Reason={e.Reason}");
            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                Console.WriteLine("Did you update the subscription info?");
            }
        };

        recognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("Session stopped event.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        };

        Console.WriteLine("Say something... (Press Enter to stop)");
        await recognizer.StartContinuousRecognitionAsync();

        // Wait for the user to stop recognition
        Console.ReadLine();

        // Stop recognition and clean up
        await recognizer.StopContinuousRecognitionAsync();
        await writer.DisposeAsync();
    }

    private static async Task AnalyzeTextWithCLU(string text)
    {
        var projectName = "";
        var deploymentName = "";
        var client = new ConversationAnalysisClient(new Uri(CluEndpoint), new AzureKeyCredential(CluKey));
        var data = new
        {
            analysisInput = new
            {
                conversationItem = new
                {
                    text = text,
                    id = "1",
                    participantId = "1",
                }
            },
            parameters = new
            {
                projectName,
                deploymentName,
                // Use Utf16CodeUnit for strings in .NET.
                stringIndexType = "Utf16CodeUnit",
            },
            kind = "Conversation",
        };

        Response response = client.AnalyzeConversation(RequestContent.Create(data));

        using JsonDocument result = JsonDocument.Parse(response.ContentStream);
        JsonElement conversationalTaskResult = result.RootElement;
        JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");

        Console.WriteLine($"Top intent: {conversationPrediction.GetProperty("topIntent").GetString()}");

        Console.WriteLine("Intents:");
        foreach (JsonElement intent in conversationPrediction.GetProperty("intents").EnumerateArray())
        {
            Console.WriteLine($"Category: {intent.GetProperty("category").GetString()}");
            Console.WriteLine($"Confidence: {intent.GetProperty("confidenceScore").GetSingle()}");
            Console.WriteLine();
        }

        Console.WriteLine("Entities:");
        foreach (JsonElement entity in conversationPrediction.GetProperty("entities").EnumerateArray())
        {
            Console.WriteLine($"Category: {entity.GetProperty("category").GetString()}");
            Console.WriteLine($"Text: {entity.GetProperty("text").GetString()}");
            Console.WriteLine($"Offset: {entity.GetProperty("offset").GetInt32()}");
            Console.WriteLine($"Length: {entity.GetProperty("length").GetInt32()}");
            Console.WriteLine($"Confidence: {entity.GetProperty("confidenceScore").GetSingle()}");
            Console.WriteLine();

            if (entity.TryGetProperty("resolutions", out JsonElement resolutions))
            {
                foreach (JsonElement resolution in resolutions.EnumerateArray())
                {
                    if (resolution.GetProperty("resolutionKind").GetString() == "DateTimeResolution")
                    {
                        Console.WriteLine($"Datetime Sub Kind: {resolution.GetProperty("dateTimeSubKind").GetString()}");
                        Console.WriteLine($"Timex: {resolution.GetProperty("timex").GetString()}");
                        Console.WriteLine($"Value: {resolution.GetProperty("value").GetString()}");
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
