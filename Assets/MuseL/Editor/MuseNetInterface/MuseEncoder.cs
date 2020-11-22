using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MuseL
{
    public static class MuseEncoder
    {
        private static int[] Violins = { 40, 41, 44, 45, 48, 49, 50, 51 };
        private static int[] Cello = { 42, 43 };
        private static int[] Bass = { 32, 33, 34, 35, 36, 37, 38, 39 };
        private static int[] Guitar = { 24, 25, 26, 27, 28, 29, 30, 31 };
        private static int[] Flute = { 72, 73, 74, 75, 76, 77, 78, 79 };
        private static int[] Clarinet = { 64, 65, 66, 67, 68, 69, 70, 71 };
        private static int[] Trumpet = { 56, 57, 58, 59, 60, 61, 62, 63 };
        private static int[] Harp = { 46 };

        private static int[] Piano_Volumes = { 0, 24, 32, 40, 48, 56, 64, 72, 80, 88, 96, 104, 112, 120 };

        public static string[] Encode(MidiFile midiFile)
        {
            List<NoteEventData> notes = GetSortedNotes(midiFile);

            List<string> builder = new List<string>();

            Dictionary<int, int> currentInstruments = new Dictionary<int, int>();
            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];

                var deltaTime = (i == 0) ? note.StartTime : (note.StartTime - notes[i - 1].StartTime);
                var timeLeftToWait = deltaTime;
                while (timeLeftToWait > 0)
                {
                    var waitTime = timeLeftToWait > 128 ? 128 : timeLeftToWait;
                    var token = 3968 + waitTime - 1;
                    builder.Add(token.ToString());
                    timeLeftToWait -= waitTime;
                }

                if (note.IsProgrammChange)
                {
                    currentInstruments[note.Channel] = note.Pitch;
                    continue;
                }

                int currentInstrument = currentInstruments.GetOrDefault(note.Channel);

                GetBaseNote(currentInstrument, note.Channel, note.Velocity, out int baseNote, out int baseOffNote);

                if (note.IsOn)
                {
                    builder.Add((baseNote + note.Pitch).ToString());
                }
                else if (baseOffNote != -1)
                {
                    builder.Add((baseOffNote + note.Pitch).ToString());
                }
            }


            return builder.ToArray();
        }

        private static List<NoteEventData> GetSortedNotes(MidiFile midiFile)
        {
            List<NoteEventData> notes = new List<NoteEventData>();

            int museTimeScale = MuseDecoder.MUSE_BEATS_PER_MICRO_SEC / (int)GetMicroSecPerBeat(midiFile);

            int originalOrder = 0;
            foreach (var track in midiFile.GetTrackChunks())
            {
                long startTime = 0;
                foreach (var midiEvent in track.Events)
                {
                    startTime += midiEvent.DeltaTime;

                    if (midiEvent is NoteEvent note)
                    {
                        notes.Add(new NoteEventData(note.NoteNumber, note.Velocity, note.Channel, note is NoteOnEvent, startTime * museTimeScale, originalOrder));
                    }
                    else if (midiEvent is ProgramChangeEvent programChangeEvent)
                    {
                        notes.Add(new NoteEventData(programChangeEvent.ProgramNumber, programChangeEvent.Channel, startTime * museTimeScale, originalOrder));
                    }
                    else
                        continue;
                    originalOrder++;
                }
            }

            notes.Sort((a, b) =>
            {
                if (a.StartTime < b.StartTime) { return -1; }
                if (a.StartTime > b.StartTime) { return 1; }
                if (a.OriginOrder < b.OriginOrder) { return -1; }
                if (a.OriginOrder > b.OriginOrder) { return 1; }
                return 0;
            });

            return notes;
        }

        private static void GetBaseNote(int currentInstrument, int channel, int velocity, out int baseNote, out int baseOffNote)
        {
            baseNote = -1;
            baseOffNote = -1;

            if (channel == 9)
            {
                //Drums
                baseNote = 3840;
                Console.WriteLine("Encode drum");
            }
            else if (Violins.Contains(currentInstrument))
            {
                //"violin"
                baseNote = 14 * 128;
                baseOffNote = 15 * 128;
            }
            else if (Cello.Contains(currentInstrument))
            {
                baseNote = 16 * 128;
                baseOffNote = 17 * 128;
            }
            else if (Bass.Contains(currentInstrument))
            {
                baseNote = 18 * 128;
                baseOffNote = 19 * 128;
            }
            else if (Guitar.Contains(currentInstrument))
            {
                baseNote = 20 * 128;
                baseOffNote = 21 * 128;
            }
            else if (Flute.Contains(currentInstrument))
            {
                baseNote = 22 * 128;
                baseOffNote = 23 * 128;
            }
            else if (Clarinet.Contains(currentInstrument))
            {
                baseNote = 24 * 128;
                baseOffNote = 25 * 128;
            }
            else if (Trumpet.Contains(currentInstrument))
            {
                baseNote = 26 * 128;
                baseOffNote = 27 * 128;
            }
            else if (Harp.Contains(currentInstrument))
            {
                baseNote = 28 * 128;
                baseOffNote = 29 * 128;
            }
            else
            {
                //Piano
                for (int vi = 0; vi < Piano_Volumes.Length; vi++)
                {
                    if (Piano_Volumes[vi] >= velocity)
                    {
                        if (vi > 0 && Piano_Volumes[vi - 1] + Piano_Volumes[vi] > velocity * 2)
                            vi--;
                        baseNote = vi * 128;
                        break;
                    }
                }
                baseOffNote = 0 * 128;
            }
        }

        private static long GetMicroSecPerBeat(MidiFile midiFile)
        {
            foreach (var chunk in midiFile.GetTrackChunks())
            {
                foreach (var e in chunk.Events)
                {
                    if (e is SetTempoEvent tempoEvent)
                    {
                        return tempoEvent.MicrosecondsPerQuarterNote;
                    }
                }
            }

            return 1000000;
        }

        private class NoteEventData
        {
            public int Pitch;
            public int Velocity;
            public int Channel;
            public bool IsOn;

            public long StartTime;

            public int OriginOrder;

            public bool IsProgrammChange;

            public NoteEventData(int instrument, int channel, long startTime, int originOrder)
            {
                Pitch = instrument;
                Channel = channel;
                StartTime = startTime;
                OriginOrder = originOrder;
                IsProgrammChange = true;
            }

            public NoteEventData(int pitch, int velocity, int channel, bool isOn, long startTime, int originOrder)
            {
                Pitch = pitch;
                Velocity = velocity;
                Channel = channel;
                IsOn = isOn && velocity > 0;
                StartTime = startTime;
                OriginOrder = originOrder;
            }
        }

        public static V GetOrDefault<K, V>(this Dictionary<K, V> dictionary, K key)
        {
            if (dictionary.TryGetValue(key, out V value))
                return value;
            else
                return default(V);
        }
    }
}
