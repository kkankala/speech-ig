using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Speech_ig;

public class FromWavFileInput
{
    private readonly string _waveFile;
    private readonly string _outputTextFile;
    private readonly FileInfo _fileInfo;

    public FromWavFileInput(string waveFile = "Shakespeare.wav", string outputTextFile = "Shakespeare.txt")
    {
        _fileInfo = new FileInfo(waveFile);
        if (!_fileInfo.Exists)
        {
            throw new FileNotFoundException(waveFile);
        }
        _waveFile = waveFile;
        _outputTextFile = outputTextFile;
    }

    public async Task RecognizeContinuous(SpeechConfig speechConfig)
    {
        var audioConfig = AudioConfig.FromWavFileInput(_waveFile);
        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var stopRecognition = new TaskCompletionSource<int>();

        FileStream fileStream = File.OpenWrite(_outputTextFile);
        StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);


        recognizer.Recognized += async (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string recognizedText = e.Result.Text;
                Console.WriteLine($"Recognized: {recognizedText}");
                await streamWriter.WriteLineAsync(e.Result.Text);
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
            if (e.Reason != CancellationReason.EndOfStream)
            {
                Console.WriteLine("Speech recognition canceled.");
            }
            stopRecognition.TrySetResult(0);
            streamWriter.Close();
        };

        recognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("Session stopped event.");
            stopRecognition.TrySetResult(0);
            streamWriter.Close();
        };

        Console.WriteLine("Speech recognition started.");
        await recognizer.StartContinuousRecognitionAsync();
        Task.WaitAny(new[] { stopRecognition.Task });
        await recognizer.StopContinuousRecognitionAsync();
    }


    public async Task RecognizeOnce(SpeechConfig speechConfig)
    {

        try
        {
            {
                Console.WriteLine("Speech recognition started.");
                using var audioConfig = AudioConfig.FromWavFileInput(_fileInfo.FullName);
                using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                var result = await speechRecognizer.RecognizeOnceAsync();

                FileStream fileStream = File.OpenWrite(_outputTextFile);
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