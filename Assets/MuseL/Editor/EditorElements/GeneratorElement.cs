using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MuseL
{
    public class GeneratorElement : VisualElement
    {
        private ObjectField startMelodieField;
        private MyOptionsField genreField;
        private InstrumentField instrumentsField;
        private IntegerField temperatureField;
        private IntegerField turnicationField;

        private FloatField lengthField;
        private IntegerField amountField;
        private Button generateButton;

        private IGenerateMuseReciever generatorParent;

        public new class UxmlFactory : UxmlFactory<GeneratorElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as GeneratorElement;

                ate.Clear();


                ate.Init();

            }
        }

        private void Init()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("GeneratorElement");
            VisualElement labelFromUXML = visualTree.Instantiate();
            Add(labelFromUXML);

            startMelodieField = this.Q<ObjectField>("start-melodie-field");
            startMelodieField.objectType = typeof(MidiAsset);
            genreField = this.Q<MyOptionsField>("genre-field");
            genreField.Init(0, MuseReciever.Genres);
            instrumentsField = this.Q<InstrumentField>("instruments-field");

            temperatureField = this.Q<IntegerField>("temperature-field");
            turnicationField = this.Q<IntegerField>("trunication-field");
            lengthField = this.Q<FloatField>("length-field");
            amountField = this.Q<IntegerField>("amount-field");
            generateButton = this.Q<Button>("generate-button");
            generateButton.clicked += Generate;
        }

        public void SetParent(IGenerateMuseReciever parent)
        {
            this.generatorParent = parent;
        }

        private void Generate()
        {
            MuseReciever museReciever = new MuseReciever(
                (startMelodieField.value is MidiAsset midiAsset ? midiAsset.MidiFile : null),
                MuseReciever.Genres[genreField.value],
                instrumentsField.instruments,
                temperatureField.value,
                turnicationField.value,
                lengthField.value,
                (startMelodieField.value is MidiAsset midiAsset2? midiAsset2.name : ""));
            generatorParent?.Generate(museReciever, amountField.value);
        }
    }

    public interface IGenerateMuseReciever
    {
        void Generate(MuseReciever museReciever, int amount);
    }
}
