using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MuseL
{
    public static class MidiPlayer
    {
        private static Playback playback;
        private static OutputDevice outputDevice;
        private static Task playTask;

        public static float Progress
        {
            get
            {
                if (playback == null || !playback.IsRunning)
                    return 0;
                float p = ((float)playback.GetCurrentTime<MetricTimeSpan>().TotalMicroseconds) / ((float)playback.GetDuration<MetricTimeSpan>().TotalMicroseconds);
                return p;
            }
            set
            {
                if (playback == null || !playback.IsRunning)
                    return;

                float v = Mathf.Clamp01(value);
                playback.MoveToTime(playback.GetCurrentTime<MetricTimeSpan>().Multiply(v));
            }
        }

        public static bool IsPlaying
        {
            get
            {
                return playback != null && playback.IsRunning;
            }
        }

        public static void ContinuePlay()
        {
            if (playback == null || !playback.IsRunning)
                return;

            playback.Play();
        }

        public static void Play(MidiFile file)
        {
            DisposePlayback();

            UnityTickGenerator generator = new UnityTickGenerator();

            MidiClockSettings clockSettings = new MidiClockSettings();
            clockSettings.CreateTickGeneratorCallback = () => generator;

            outputDevice = OutputDevice.GetById(0);
            playback = file.GetPlayback(outputDevice, clockSettings);

            playback.Start();

            playback.Finished += (a, b) =>
            {
                DisposePlayback();
            };
        }

        public static void Stop()
        {
            DisposePlayback();
        }

        private static void DisposePlayback()
        {
            playback?.Dispose();
            outputDevice?.Dispose();
        }

        public static string GetPlayString()
        {
            string s = "";
            if (playback != null && playback.IsRunning)
            {
                s += Format(playback.GetCurrentTime<MetricTimeSpan>()) + " of " +
                    Format(playback.GetDuration<MetricTimeSpan>());
            }
            else
                s = "--";
            return s;
        }

        private static string Format(MetricTimeSpan timeSpan)
        {
            return timeSpan.Minutes + ":" + timeSpan.Seconds.ToString("00");
        }

        private class UnityTickGenerator : TickGenerator
        {
            private CancellationTokenSource cts;

            protected override void Start(TimeSpan interval)
            {
                cts = new CancellationTokenSource();

                playTask = new Task(() =>
                {
                    CancellationToken token = cts.Token;
                    while (IsRunning && !token.IsCancellationRequested)
                    {
                        GenerateTick();
                        Thread.Sleep(1);
                    }
                });
                playTask.Start();
            }

            public override void Dispose()
            {
                base.Dispose();
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
