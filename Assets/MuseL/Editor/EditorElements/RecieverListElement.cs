using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MuseL
{
    public class RecieverListElement : VisualElement,IDisposable
    {
        private ListView listView;
        private List<MuseReciever> recievers = new List<MuseReciever>();

        private MuseEditor museParent;

        public new class UxmlFactory : UxmlFactory<RecieverListElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as RecieverListElement;

                ate.Clear();

                ate.Init();

            }
        }

        public void Add(MuseReciever reciever)
        {
            recievers.Add(reciever);
            listView.Refresh();
        }

        public void AddRange(IEnumerable<MuseReciever> recievers)
        {
            this.recievers.AddRange(recievers);
            listView.Refresh();
        }

        public void Remove(MuseReciever reciever)
        {
            this.recievers.Remove(reciever);
            reciever.Dispose();
            listView.Refresh();
        }

        public void SetParent(MuseEditor parent)
        {
            this.museParent = parent;
        }

        private void Init()
        {

            Func<VisualElement> makeItem = () => new MuseRecieverElement(museParent);
            Action<VisualElement, int> bindItem = (e, i) => (e as MuseRecieverElement).SetReciever(recievers[i]);

            const int itemHeight = 16;

            listView = new ListView(recievers, itemHeight, makeItem, bindItem);

            //listView.selectionType = SelectionType.Multiple;

            //listView.onItemChosen += obj => Debug.Log(obj);
            //listView.onSelectionChanged += objects => Debug.Log(objects);

            listView.style.flexGrow = 1.0f;
            Add(listView);
        }

        public void Dispose()
        {
            foreach (var r in recievers)
                r.Dispose();
            recievers.Clear();
        }
    }
}
