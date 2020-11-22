using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using UnityEditor;
using UnityEngine;

namespace MuseL
{
    [CustomEditor(typeof(MidiAsset))]
    public class MidiFileInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Play")){
                if(target is MidiAsset midiAsset)
                {
                    //using (var outputDevice = OutputDevice.GetById(0))
                    //using(var playback = midiAsset.MidiFile.GetPlayback(outputDevice))
                    //{
                    //    playback.Play();
                    //}
                    MidiPlayer.Play(midiAsset.MidiFile);
                }
            }
        }
    }
}
