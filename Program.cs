using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Speech_ig;

Console.WriteLine("test");

var builder = new ConfigurationBuilder();
builder.AddUserSecrets<Program>();

var configuration = builder.Build();

string azureKey = configuration.GetSection("kk-ig-ai:apiKey")?.Value ?? throw new NullReferenceException("missing app config");
string azureLocation = "eastus";
var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);

var wavFile = new FromWavFileInput();
await wavFile.RecognizeOnce(speechConfig);

