# NMEA Parser

Parser for NMEA messages. Currently parses:
* GSV
* GGA
* GSA
* GLL
* GMC
* VTG
* TXT

## Usage

The parser parses message like '$GNGLL,4513.13795,N,01859.19702,E,143717.00,A,A*72'

        ...
        _parser = new NmeaParser();

        _parser.NmeaMessageParsed += NmeaMessageParsed;

        _parser.Parse(sentence);
        ...

        private static void NmeaMessageParsed(object sender, NmeaMessage e)
        {
            Console.WriteLine($"{e}");
            
            if (e is GnrmcMessage)
            {
               // Use message
            }   
        }

Once the message is parsed, a derived NmeaMessage is made available in the 'NmeaMessageParsed' event.

### Multiple parsers?

If you have multiple NMEA devices please check out the 'NmeaParserList' class. This class is designed to support multiple parsers without crossing input.

## Why events?

The event is used because some NMEA messages are split up in multiple sentences like GSA and GSV.

For these messages, the result is buffered. Once the complete message is made available, the event is raised.

## Degrees

The location degrees are calculated as decimal degrees. 

        (d)dd + (mm.mmmm/60) (* -1 for W and S)

## Serial Reader

For testing purposes, a reader for serial ports is added in the test application. If uses .Net Standard System IO access to serial ports.

### System.IO.Ports

The example app supports now Windows and Linux.

## NuGet

You can use this library as a [Nuget](https://www.nuget.org/packages/svelde-nmea-parser) Package:

    Install-Package svelde-nmea-parser -Version 1.0.5

## Tested devices

The following devices are used to test the parser:

* BEITIAN USB GNSS GPS Receiver BN-85U (U-Blox UBX-M8030)
* Webio GRB-288 Bluetooth GPS mouse 
* GSpace GS-R238 GPS mouse (SiRFstarIII)

*Note: If you want to have your GPS device tested and officially supported here, please send me a DM on github or twitter @svelde*

## Contribute

The code of this logic is [open source](https://github.com/sandervandevelde/nmeaparser) and licenced under the MIT license.

Want to contribute? Throw me a pull request....

Want to know more about me? Check out [my blog](https://blog.vandevelde-online.com).
