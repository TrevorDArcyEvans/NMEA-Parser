using Newtonsoft.Json;
using svelde.nmea.parser;
using System;
using System.Collections.Generic;
using System.IO;

namespace svelde.nmea.app
{
  public class Program : IDisposable
  {
    private static NmeaParser _parser;

    private static StreamWriter _streamWriter;

    private static SerialReader _serialReader;

    public static void Main(string[] args)
    {
      // get port from command line or use default (COM7)
      var port = args.Length == 1 ? args[0] : "COM7";
      
      var utc = DateTime.UtcNow;
      var fileName = $"{utc.Year}-{utc.Month}-{utc.Day}={utc.Hour}-{utc.Minute}-{utc.Second}.log";
      _streamWriter = File.AppendText(fileName);

      Console.WriteLine($"Read serial port: {port}");

      _parser = new NmeaParser();

      _parser.NmeaMessageParsed += NmeaMessageParsed;

      _serialReader = new SerialReader(args[0]);

      _serialReader.NmeaSentenceReceived += NmeaSentenceReceived;

      _serialReader.Open();

      Console.WriteLine("Initialized...");

      Console.ReadKey();
    }

    private static void NmeaMessageParsed(object sender, NmeaMessage e)
    {
      var @switch = new Dictionary<Type, Action>
      {
        {typeof(GnggaMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GpggaMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GngllMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GngsaMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GpgsaMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GnrmcMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GprmcMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GntxtMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GnvtgMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GpvtgMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GpgsvMessage), () => { Console.WriteLine($"{e}(GPS)"); }},
        {typeof(GlgsvMessage), () => { Console.WriteLine($"{e}(Glosnass)"); }},
        {typeof(GbgsvMessage), () => { Console.WriteLine($"{e}(Baidoo)"); }},
      };

      @switch[e.GetType()]();
    }

    private static void NmeaSentenceReceived(object sender, NmeaSentence e)
    {
      _streamWriter.WriteLine(e.Sentence);

      _parser.Parse(e.Sentence);
    }

    public void Dispose()
    {
      _serialReader?.Dispose();
    }
  }

  public class Telemetry
  {
    [JsonProperty(PropertyName = "location")]
    public TelemetryLocation Location { get; set; }

    [JsonProperty(PropertyName = "modeIndicator")]
    public ModeIndicator ModeIndicator { get; set; }

    [JsonProperty(PropertyName = "fixTaken")]
    public string FixTaken { get; set; }
  }

  public class TelemetryLocation
  {
    [JsonProperty(PropertyName = "lat")]
    public decimal Latitude { get; set; }

    [JsonProperty(PropertyName = "lon")]
    public decimal Longitude { get; set; }

    [JsonProperty(PropertyName = "alt")]
    public decimal? Altitude { get; set; }
  }
}
