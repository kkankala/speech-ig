using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure;
using Azure.AI.Language.Conversations;

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
        var client = new ConversationAnalysisClient(new Uri(CluEndpoint), new AzureKeyCredential(CluKey));

        var response = await client.AnalyzeConversationAsync(
            "YourProjectName", // Replace with your CLU project name
            "YourDeploymentName", // Replace with your CLU deployment name
            new TextConversationAnalysisInput
            {
                Text = text,
                Language = "en"
            }
        );

        Console.WriteLine($"Intent: {response.Value.Prediction.TopIntent}");
        Console.WriteLine("Entities:");
        foreach (var entity in response.Value.Prediction.Entities)
        {
            Console.WriteLine($"  - {entity.Category}: {entity.Text} (Confidence: {entity.ConfidenceScore})");
        }
    }
}
