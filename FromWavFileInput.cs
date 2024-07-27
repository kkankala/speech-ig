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