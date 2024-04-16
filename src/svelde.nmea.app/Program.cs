﻿using Newtonsoft.Json;
using svelde.nmea.parser;
using System;
using System.Collections.Generic;
using System.IO;

namespace svelde.nmea.app
{
  public class Program : IDisposable
  {
    private static DateTime _lastSent;

    private static NmeaParser _parser;

    private static StreamWriter _streamWriter;

    private static SerialReader _serialReader;

    public static void Main(string[] args)
    {
      _lastSent = DateTime.Now;

      var utc = DateTime.UtcNow;
      var fileName = $"{utc.Year}-{utc.Month}-{utc.Day}={utc.Hour}-{utc.Minute}-{utc.Second}.log";
      _streamWriter = File.AppendText(fileName);

      Console.WriteLine("Read serial port");

      _parser = new NmeaParser();

      _parser.NmeaMessageParsed += NmeaMessageParsed;

      // TODO   support passing in port from command line
      _serialReader = new SerialReader();

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
        {typeof(GnrmcMessage), () => { SendMessage(e); }},
        {typeof(GprmcMessage), () => { SendMessage(e); }},
        {typeof(GntxtMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GnvtgMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GpvtgMessage), () => { Console.WriteLine($"{e}"); }},
        {typeof(GpgsvMessage), () => { Console.WriteLine($"{e}(GPS)"); }},
        {typeof(GlgsvMessage), () => { Console.WriteLine($"{e}(Glosnass)"); }},
        {typeof(GbgsvMessage), () => { Console.WriteLine($"{e}(Baidoo)"); }},
      };

      @switch[e.GetType()]();
    }

    private static void SendMessage(NmeaMessage e)
    {
      Console.WriteLine($"{e}");

      if (_lastSent >= DateTime.Now.AddSeconds(-10))
      {
        return;
      }

      try
      {
        if (!(e is GnrmcMessage)
            || !(e as GnrmcMessage).ModeIndicator.IsValid())
        {
          Console.WriteLine($"*** Invalid fix '{(e as GngllMessage)?.ModeIndicator}'; no location sent");
          return;
        }

        _lastSent = DateTime.Now;

        var telemetry = new Telemetry
        {
          Location = new TelemetryLocation
          {
            Latitude = (e as GnrmcMessage).Latitude.ToDecimalDegrees(),
            Longitude = (e as GnrmcMessage).Longitude.ToDecimalDegrees(),
          },
          FixTaken = (e as GnrmcMessage).TimeOfFix,
          ModeIndicator = (e as GnrmcMessage).ModeIndicator,
        };

        var json = JsonConvert.SerializeObject(telemetry);

        // TODO     SendMessage
        // var message = new Message(Encoding.ASCII.GetBytes(json));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Exception during IoT communication {ex}");
      }
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
