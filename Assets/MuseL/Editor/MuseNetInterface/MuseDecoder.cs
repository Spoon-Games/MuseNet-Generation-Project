

using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;

namespace MuseL
{
    public static class MuseDecoder
    {
        public static string[] Instruments = {"piano", "piano", "piano", "piano", "piano", "piano", "piano", "piano", "piano", "piano", "piano", "piano", "piano", "piano",
                                  "violin", "violin", "cello", "cello", "bass", "bass", "guitar", "guitar",
                                  "flute", "flute", "clarinet", "clarinet", "trumpet", "trumpet", "harp", "harp"};

        public static int[] Volumes = { 0, 24, 32, 40, 48, 56, 64, 72, 80, 88, 96, 104, 112, 120, 80, 0, 80, 0, 80, 0, 80, 0, 80, 0, 80, 0, 80, 0, 80, 0 };

        public const int MUSE_BEATS_PER_MICRO_SEC = 960000;

        public static MidiFile DecodeMuseEncoding(string[] tokens)
        {
            TrackConsultant tracks = new TrackConsultant();

            foreach (string token in tokens)
            {
                DecodeToken(int.Parse(token), tracks);
            }

            tracks.FinishDrums();
            return tracks.GetMidiFile();
        }

        public static int GetDurration(string[] tokens)
        {
            int durration = 0;
            foreach(string t in tokens)
            {
                int token = int.Parse(t);
                if (token >= 3968 && token < 4096)
                {
                    var delay = (token % 128) + 1;
                    durration += delay;
                }
            }
            return durration * 10;
        }

        private static void DecodeToken(int token, TrackConsultant tracks)
        {
            if (token >= 0 && token < 3840)
            {
                var pitch = token % 128;
                var inst_vol_index = token >> 7;
                var instrument = Instruments[inst_vol_index];
                var volume = Volumes[inst_vol_index];
                tracks[instrument].AddNoteEvent(pitch, volume, volume > 0);
                return;

            }
            else if (token >= 3840 && token < 3968)
            {
                var pitch = token % 128;
                tracks["drum"].AddNoteEvent(pitch, 80, true);
            }
            else if (token >= 3968 && token < 4096)
            {
                var delay = (token % 128) + 1;
                tracks.AddDeltaTime(delay);
                return;
            }
            else if (token == 4096)
            {
                return; //Start
            }
            else
            {
                return; //Invalid
            }


        }

        private class TrackConsultant
        {
            private Dictionary<string, TrackData> tracks = new Dictionary<string, TrackData>();
            private long globalDeltaTime = 0;

            public TrackData this[string key]
            {
                get
                {
                    if (tracks.TryGetValue(key, out TrackData data))
                        return data;
                    else
                    {
                        TrackData d = new TrackData(key, globalDeltaTime);
                        tracks.Add(key, d);
                        return d;

                    }
                }
            }

            public void AddDeltaTime(long deltaTime)
            {
                globalDeltaTime += deltaTime;

                foreach (var dat in tracks.Values)
                {
                    dat.AddDeltaTime(deltaTime);
                }
            }

            public void FinishDrums()
            {
                if (tracks.TryGetValue("drum", out TrackData data))
                {
                    HashSet<int> drumPitches = new HashSet<int>();

                    foreach (var e in data.trackChunk.Events)
                    {
                        if (e is NoteOnEvent noteOn)
                        {
                            drumPitches.Add(noteOn.NoteNumber);
                        }
                    }

                    foreach (int pitch in drumPitches)
                    {
                        data.AddNoteEvent(pitch, 0, false);
                    }
                }
            }

            public MidiFile GetMidiFile()
            {
                MidiFile midiFile = new MidiFile();
                midiFile.Chunks.Add(new TrackChunk(new SetTempoEvent(MUSE_BEATS_PER_MICRO_SEC)));

                foreach (var data in tracks)
                {
                    midiFile.Chunks.Add(data.Value.trackChunk);

                    //Console.WriteLine("Track: " + data.Key + " Length: " + data.Value.trackChunk.Events.Count);
                    //foreach (var e in data.Value.trackChunk.Events)
                    //  Console.WriteLine("\tEvent: " + e.ToString());
                }

                return midiFile;
            }

        }

        private class TrackData
        {
            internal TrackChunk trackChunk;
            private long deltaTime = 0;
            internal int channel;

            public TrackData(string instrument, long deltaTime)
            {
                channel = InstrumentToChannel(instrument);

                trackChunk = new TrackChunk(new ProgramChangeEvent((SevenBitNumber)ChannelToProgramm(channel)) { Channel = (FourBitNumber)channel });

                //AddEvent(new ProgramChangeEvent((SevenBitNumber)49));
                //AddEvent(new ChannelPrefixEvent((byte)channel));

                this.deltaTime = deltaTime;

                //Console.WriteLine("New TrackData: " + instrument + " Channel: " + channel + " Programm: " + ChannelToProgramm(channel));
            }

            public void AddEvent(MidiEvent e)
            {
                trackChunk.Events.Add(e);
            }

            public void AddNoteEvent(int pitch, int volume, bool isOn)
            {
                //Console.WriteLine("Add NoteEvent Pitch: " + pitch + " volume: " + volume + " isOn: " + isOn+" deltaTime: "+deltaTime);

                if (isOn)
                {
                    AddEvent(new NoteOnEvent((SevenBitNumber)pitch, (SevenBitNumber)volume)
                    {
                        Channel = (FourBitNumber)channel,
                        DeltaTime = deltaTime
                    });
                }
                else
                {
                    AddEvent(new NoteOffEvent((SevenBitNumber)pitch, (SevenBitNumber)0)
                    {
                        Channel = (FourBitNumber)channel,
                        DeltaTime = deltaTime
                    });
                }

                deltaTime = 0;
            }

            internal void AddDeltaTime(long deltaTime)
            {
                this.deltaTime += deltaTime;
            }

            private static int InstrumentToChannel(string instrument)
            {
                switch (instrument)
                {
                    case "piano": return 0;
                    case "violin": return 1;
                    case "cello": return 2;
                    case "bass": return 3;
                    case "guitar": return 4;
                    case "flute": return 5;
                    case "clarinet": return 6;
                    case "trumpet": return 7;
                    case "harp": return 8;
                    case "drum": return 9;
                    default: return 0;
                }
            }

            private static int ChannelToProgramm(int channel)
            {
                switch (channel)
                {
                    case 0: return 0;
                    case 1: return 40;
                    case 2: return 42;
                    case 3: return 32;
                    case 4: return 24;
                    case 5: return 73;
                    case 6: return 71;
                    case 7: return 56;
                    case 8: return 46;
                    case 9: return 0;
                    default: return 0;
                }
            }
        }
    }
}
