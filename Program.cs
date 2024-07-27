using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Speech_ig;
using Microsoft.CognitiveServices.Speech.Audio;

Console.WriteLine("test");

var builder = new ConfigurationBuilder();
builder.AddUserSecrets<Program>();

var configuration = builder.Build();

string azureKey = configuration.GetSection("kk-ig-ai:apiKey")?.Value ?? throw new NullReferenceException("missing app config");
string azureLocation = "eastus";
var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);

// var wavFile = new FromWavFileInput();
// await wavFile.RecognizeContinuous(speechConfig);

// var microphone = new FromMicrophoneInput();
// await microphone.RecognizeOnce(speechConfig);


string outputFilePath = "output.wav";

// Configure the synthesizer to output to a WAV file
var audioConfig = AudioConfig.FromWavFileOutput(outputFilePath);
var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

// Text to be synthesized
string textToSynthesize = @"
            Agent: Thank you for calling [Company Name]! This is Alex. How can I assist you today?
            Customer: Hi Alex, I'm looking to see if you have any products in the price range of $20,000 to $50,000. Oh, and I need it to be red, with free shipping. Do you have anything like that?
            Agent: Hmm, let me check our inventory for you. One moment, please...
            Customer: Sure, take your time.
            Alright, thank you for waiting. We do have a few options that fit your criteria. First, we have the Model X Sports Car, which is priced at $45,000. It's available in a stunning red color and comes with free shipping.
            Customer: Oh, that sounds interesting. What else do you have?
            Agent: Let's see... we also have the UltraLux Sofa Set. It's perfect for a luxury living room, priced at $22,000. It’s available in a beautiful red fabric and, yes, it includes free shipping as well.
            Customer: Hmm, a sofa set. That's nice. Do you have anything else?
            Agent: Yes, we do. Another option is the Redstone Home Theater System. It's a top-of-the-line entertainment system, priced at $30,000. And of course, it ships for free.
            Customer: Those all sound great. The sports car is very tempting. Can you tell me more about it?
            Agent: Absolutely! The Model X Sports Car is one of our bestsellers. It features a powerful engine, sleek design, and the red color really makes it stand out. It also comes with a lot of high-tech features, like a state-of-the-art navigation system and advanced safety features.
            Customer: That sounds fantastic. I think I might go with the sports car. How do I proceed with the purchase?
            Agent: Wonderful choice! I can assist you with that. I'll need some information to get started. Could you please provide me with your name and contact details?
            Customer: Sure, my name is John Doe, and my phone number is 555-1234.
            Agent: Thank you, John. I'll just process this information and get everything set up for you. You’ll receive an email with the order confirmation and details shortly. Is there anything else I can help you with today?
            Customer: No, that's all. Thanks, Alex!
            Agent: You're welcome, John! Thank you for choosing [Company Name]. Have a great day!
            Customer: You too! Bye.
            Agent: Goodbye!";

// Perform synthesis
var result = await synthesizer.SpeakTextAsync(textToSynthesize);

// Check the result
if (result.Reason == ResultReason.SynthesizingAudioCompleted)
{
    Console.WriteLine($"Synthesis succeeded. Audio content written to file '{outputFilePath}'.");
}
else if (result.Reason == ResultReason.Canceled)
{
    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

    if (cancellation.Reason == CancellationReason.Error)
    {
        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
        Console.WriteLine($"CANCELED: Did you update the subscription info?");
    }
}