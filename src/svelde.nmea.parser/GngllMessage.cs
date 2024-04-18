using System;
using Newtonsoft.Json;

namespace svelde.nmea.parser
{
  public abstract class GllMessage : NmeaMessage
  {
    [JsonProperty(PropertyName = "latitude")]
    public Location Latitude { get; private set; }

    [JsonProperty(PropertyName = "longitude")]
    public Location Longitude { get; private set; }

    [JsonProperty(PropertyName = "fixTaken")]
    public string FixTaken { get; private set; }

    [JsonProperty(PropertyName = "dataValid")]
    public string DataValid { get; private set; }

    [JsonProperty(PropertyName = "modeIndicator")]
    public ModeIndicator ModeIndicator { get; private set; }

    public override void Parse(string nmeaLine)
    {
      if (string.IsNullOrWhiteSpace(nmeaLine)
          || !nmeaLine.StartsWith($"${Type}"))
      {
        throw new NmeaParseMismatchException();
      }

      ParseChecksum(nmeaLine);

      if (MandatoryChecksum != ExtractChecksum(nmeaLine))
      {
        throw new NmeaParseChecksumException();
      }

      // remove identifier plus first comma
      var sentence = nmeaLine.Remove(0, $"${Type}".Length + 1);

      // remove checksum and star
      sentence = sentence.Remove(sentence.IndexOf('*'));

      var items = sentence.Split(',');

      Latitude = new Location(items[0] + items[1]);
      Longitude = new Location(items[2] + items[3]);
      FixTaken = items[4];
      DataValid = items[5];

      ModeIndicator = items.Length > 6
        ? new ModeIndicator(items[6])
        : new ModeIndicator("");

      OnNmeaMessageParsed(this);
    }

    protected override void OnNmeaMessageParsed(NmeaMessage e)
    {
      base.OnNmeaMessageParsed(e);
    }

    public override string ToString()
    {
      var result = $"{Type}-{Port} Latitude:{Latitude} Longitude:{Longitude} FixTaken:{FixTaken} Valid:{DataValid} Mode:{ModeIndicator}";

      return result;
    }
  }

  public sealed class GngllMessage : GllMessage
  {
    public GngllMessage()
    {
      // GLL protocol header
      //    GP indicates the device receives GPS satellites signal only
      //    GN indicates the position is calculated with BEIDOU satellite signal
      Type = "GNGLL";
    }
  }

  public sealed class GpgllMessage : GllMessage
  {
    public GpgllMessage()
    {
      // GLL protocol header
      //    GP indicates the device receives GPS satellites signal only
      //    GN indicates the position is calculated with BEIDOU satellite signal
      Type = "GPGLL";
    }
  }
}
