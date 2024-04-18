using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace svelde.nmea.parser
{
    public abstract class GsvMessage : NmeaMessage
    {
        public GsvMessage()
        {
            Satellites = new List<Satellite>();
        }

        [JsonProperty(PropertyName = "numberOfSentences")]
        public int NumberOfSentences { get; private set; }

        [JsonProperty(PropertyName = "sentenceNr")]
        public int SentenceNr { get; private set; }

        [JsonProperty(PropertyName = "numberOfSatellitesInView")]
        public int NumberOfSatellitesInView { get; private set; }

        [JsonProperty(PropertyName = "satellites")]
        public List<Satellite> Satellites { get; private set; }

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

            NumberOfSentences = Convert.ToInt32(items[0]);
            SentenceNr = Convert.ToInt32(items[1]);
            NumberOfSatellitesInView = Convert.ToInt32(items[2]);

            var satelliteCount = GetSatelliteCount(
                Convert.ToInt32(NumberOfSatellitesInView),
                Convert.ToInt32(NumberOfSentences),
                Convert.ToInt32(SentenceNr));

            for (int i = 0; i < satelliteCount; i++)
            {
                Satellites.Add(
                    new Satellite
                    {
                        SatellitePrnNumber = items[3 + (i * 4) + 0],
                        ElevationDegrees = items[3 + (i * 4) + 1],
                        AzimuthDegrees = items[3 + (i * 4) + 2],
                        SignalStrength = items[3 + (i * 4) + 3],
                    });
            }

            if (NumberOfSentences == SentenceNr)
            {
                OnNmeaMessageParsed(this);

                Satellites.Clear();
            }
        }

        protected override void OnNmeaMessageParsed(NmeaMessage e)
        {
            base.OnNmeaMessageParsed(e);
        }

        private int GetSatelliteCount(int numberOfSatellitesInView, int numberOfSentences, int sentenceNr)
        {
            if (numberOfSentences != sentenceNr)
            {
                return 4;
            }
            else
            {
                return numberOfSatellitesInView - ((sentenceNr - 1) * 4);
            }
        }

        public override string ToString()
        {
            var result = $"{Type}-{Port} InView:{NumberOfSatellitesInView} ";

            foreach(var s in Satellites)
            {
                result += $"{s.SatellitePrnNumber}: Azi={s.AzimuthDegrees}° Ele={s.ElevationDegrees}° Str={s.SignalStrength}; ";
            }

            return result; 
        }
    }
}

