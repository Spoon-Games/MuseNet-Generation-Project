using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MuseL
{
    public class MuseRecieverElement : VisualElement
    {
        private Label nameLabel;
        private Label statusLabel;
        private Button playButton;
        private Button saveButton;
        //private Button saveAsButton;
        private Button deleteButton;

        private MuseReciever reciever;

        private MuseEditor museParent;

        public new class UxmlFactory : UxmlFactory<MuseRecieverElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as MuseRecieverElement;

                ate.Clear();

                ate.Init();

            }
        }

        public MuseRecieverElement()
        {
            Init();
        }

        public MuseRecieverElement(MuseEditor parent):this()
        {
            this.museParent = parent;
            if(parent != null)
            {
                parent.OnUpdate += Update;
            }
        }

        private void Init()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("MuseRecieverElement");
            VisualElement labelFromUXML = visualTree.Instantiate();
            Add(labelFromUXML);

            nameLabel = this.Q<Label>("name-label");
            nameLabel.text = "Empty";
            statusLabel = this.Q<Label>("status-label");
            statusLabel.text = "Not started";
            playButton = this.Q<Button>("play-button");
            playButton.clicked += Play;
            saveButton = this.Q<Button>("save-button");
            saveButton.clicked += Save;
            //saveAsButton = this.Q<Button>("save-as-button");
            //saveAsButton.clicked += SaveAs;
            deleteButton = this.Q<Button>("delete-button");
            deleteButton.clicked += Delete;
        }

        public void SetReciever(MuseReciever reciever)
        {
            this.reciever = reciever;

            nameLabel.text = reciever.GetName();
        }

        public void Update()
        {
            string status = "";
            if(reciever != null)
            {
                status += new TimeSpan(0,0,0,0,reciever.CurrentDurration).ToString(@"m\:ss") + " of "+ new TimeSpan(0, 0, 0, 0, reciever.totalDurration).ToString(@"m\:ss");
                if (reciever.IsNoResponse)
                    status += " No Response";
                else
                    status += reciever.IsRunning ? "  Running..." : "  Finished";
            }
            else
            {
                status += "Error";
            }
            statusLabel.text = status;
        }


        private void Play()
        {
            reciever?.Play();
        }

        private void Save()
        {
            if(reciever != null)
            museParent.Save(reciever);
        }

        //private void SaveAs()
        //{
        //    if (reciever != null)
        //        museParent.SaveAs(reciever);
        //}

        private void Delete()
        {
            if(reciever != null)
                museParent.TryDeleteReciever(reciever);

            museParent.OnUpdate -= Update;
        }
    }
}
