﻿using Newtonsoft.Json;

namespace svelde.nmea.parser
{
    /// <summary>
    /// $GNVTG,,T,,M,0.050,N,0.092,K,A*33
    ///        VTG - Track made good and ground speed
    ///        VTG,054.7,T,034.4,M,005.5,N,010.2,K
    ///           054.7,T True track made good
    ///           034.4,M Magnetic track made good
    ///           005.5,N Ground speed, knots
    ///           010.2,K Ground speed, Kilometers per hour
    /// </summary>
    public class GnvtgMessage : NmeaMessage
    {
        public override string GetIdentifier() => "$GNVTG";

        [JsonProperty(PropertyName = "trueTrackMadeGood")]
        public string TrueTrackMadeGood { get; private set; }

        [JsonProperty(PropertyName = "magneticTrackMadeGood")]
        public string MagneticTrackMadeGood { get; private set; }

        [JsonProperty(PropertyName = "groundSpeedKnots")]
        public string GroundSpeedKnots { get; private set; }

        [JsonProperty(PropertyName = "groundSpeedKilometersPerHour")]
        public string GroundSpeedKilometersPerHour { get; private set; }

        [JsonProperty(PropertyName = "nodeIndicator")]
        public ModeIndicator ModeIndicator { get; private set; }

        public override void Parse(string nmeaLine)
        {
            if (string.IsNullOrWhiteSpace(nmeaLine)
                    || !nmeaLine.StartsWith(GetIdentifier()))
            {
                throw new NmeaParseMismatchException();
            }

            ParseChecksum(nmeaLine);

            if (MandatoryChecksum != ExtractChecksum(nmeaLine))
            {
                throw new NmeaParseChecksumException();
            }

            // remove identifier plus first comma
            var sentence = nmeaLine.Remove(0, GetIdentifier().Length + 1);

            // remove checksum and star
            sentence = sentence.Remove(sentence.IndexOf('*'));

            var items = sentence.Split(',');

            // TODO: check existance of indexbefore inserting

            TrueTrackMadeGood = items[0] + items[1];
            MagneticTrackMadeGood = items[2] + items[3];
            GroundSpeedKnots = items[4] + items[5];
            GroundSpeedKilometersPerHour = items[6] + items[7];

            ModeIndicator = items.Length > 8
                ? new ModeIndicator(items[8])
                : new ModeIndicator("");

            OnNmeaMessageParsed(this);
        }

        protected override void OnNmeaMessageParsed(NmeaMessage e)
        {
            base.OnNmeaMessageParsed(e);
        }

        public override string ToString()
        {
            var result = $"{GetIdentifier()} Truetrack:{TrueTrackMadeGood} MagneticTrack:{MagneticTrackMadeGood} Speed:{GroundSpeedKnots}/{GroundSpeedKilometersPerHour} Mode:{ModeIndicator}";

            return result;
        }
    }
}
