using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Speech_ig;

public class FromMicrophoneInput
{
    private readonly string _outputTextFile;

    public FromMicrophoneInput(string outputTextFile = "userspeech.txt")
    {
        _outputTextFile = outputTextFile;
    }

    public async Task RecognizeOnce(SpeechConfig speechConfig)
    {
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        // Perform a single recognition
        var result = await speechRecognizer.RecognizeOnceAsync();

        // Check the result
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            Console.WriteLine($"Recognized: {result.Text}");
        }
        else if (result.Reason == ResultReason.NoMatch)
        {
            Console.WriteLine("No speech could be recognized.");
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(result);
            Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

            if (cancellation.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you update the subscription info?");
            }
        }
    }

    public async Task RecognizeContinuous(SpeechConfig speechConfig)
    {
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        speechRecognizer.Recognized += async (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string recognizedText = e.Result.Text;
                Console.WriteLine($"Recognized: {recognizedText}");
                await File.AppendAllTextAsync(_outputTextFile, recognizedText + Environment.NewLine);
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine("No speech could be recognized.");
            }
        };

        speechRecognizer.Canceled += (s, e) =>
        {
            Console.WriteLine($"CANCELED: Reason={e.Reason}");

            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you update the subscription info?");
            }
        };

        speechRecognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("Session stopped event.");
        };

        await speechRecognizer.StartContinuousRecognitionAsync();

        // Keep the program running while recognition is happening
        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();

        // Stop continuous recognition
        await speechRecognizer.StopContinuousRecognitionAsync();

        Console.WriteLine("Speech recognition stopped.");
    }
}