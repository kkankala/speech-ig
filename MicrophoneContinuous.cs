using Microsoft.CognitiveServices.Speech;

namespace Speech_ig;

public class MicrophoneContinuous
{

    public static async Task ContinuousRecognitionWithStopAsync(SpeechConfig speechConfig)
    {
        using var recognizer = new SpeechRecognizer(speechConfig);
        recognizer.Recognized += async (s, e) =>
        {
            System.Console.WriteLine("reason is ", e.Result.Reason);
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"Recognized: {e.Result.Text}");
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
                Console.WriteLine($"CANCELED: Did you update the subscription info?");
            }
        };

        recognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("Session stopped event.");
        };
        recognizer.SessionStarted += (s, e) =>
       {

           Console.WriteLine("Session started event.");
       };

        // Start continuous recognition.
        Console.WriteLine("Say something... (Press Enter to stop)");
        await recognizer.StartContinuousRecognitionAsync();

        // Wait for the user to press enter to stop the recognition.
        Console.ReadLine();

        // Stop recognition.
        await recognizer.StopContinuousRecognitionAsync();
    }

}