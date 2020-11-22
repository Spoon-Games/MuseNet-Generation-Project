using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MuseL
{
    public class InstrumentField : VisualElement
    {
        private EnumFlagsField enumFlagsField;

        public Instruments instruments => (Instruments)enumFlagsField.value;

        public new class UxmlFactory : UxmlFactory<InstrumentField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as InstrumentField;

                ate.Clear();

                ate.enumFlagsField = new EnumFlagsField("Instruments", Instruments.piano,true);
                ate.Add(ate.enumFlagsField);

            }
        }

        
    }
}
