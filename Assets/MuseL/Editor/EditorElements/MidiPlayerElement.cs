using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MuseL
{
    public class MidiPlayerElement : VisualElement
    {
        private Button playButton;
        private Slider playSlider;
        private Label timeLabel;
        private MidiFile selectedFile;

        public new class UxmlFactory : UxmlFactory<MidiPlayerElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as MidiPlayerElement;


                ate.Clear();

                ate.Init();

            }
        }

        private void Init()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("MidiPlayer");
            VisualElement labelFromUXML = visualTree.Instantiate();
            Add(labelFromUXML);

            playButton = this.Q<Button>("play-button");
            playButton.clicked += OnPressedPlay;
            playSlider = this.Q<Slider>();
            playSlider.RegisterValueChangedCallback(v =>
            {
                MidiPlayer.Progress = v.newValue;
            });
            timeLabel = this.Q<Label>("time-label");
            timeLabel.text = "00";
        }

        private void OnPressedPlay()
        {
            if (MidiPlayer.IsPlaying)
            {
                MidiPlayer.Stop();
            }
            else
                MidiPlayer.ContinuePlay();
        }


        public void Update()
        {
            playSlider.SetValueWithoutNotify(MidiPlayer.Progress);
            timeLabel.text = MidiPlayer.GetPlayString();
            playButton.text = MidiPlayer.IsPlaying ? "Stop" : "Play";
        }
    } 
}
