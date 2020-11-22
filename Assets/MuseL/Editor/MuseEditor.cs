using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace MuseL
{
    public class MuseEditor : EditorWindow,IGenerateMuseReciever
    {
        private MidiPlayerElement midiPlayer;
        private GeneratorElement generatorElement;
        private RecieverListElement recieverList;

        public Action OnUpdate;

        [MenuItem("Window/Util/MuseEditor")]
        public static void ShowExample()
        {
            MuseEditor wnd = GetWindow<MuseEditor>();
            wnd.titleContent = new GUIContent("MuseEditor");
        }

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = Resources.Load<VisualTreeAsset>("MuseEditor");
            VisualElement labelFromUXML = visualTree.Instantiate();
            labelFromUXML.style.flexGrow = 1;
            labelFromUXML.style.height = new StyleLength() { value = new Length(100, LengthUnit.Percent) };
            root.Add(labelFromUXML);
            root.style.flexGrow = 1;
            root.style.height = new StyleLength() { value = new Length(100, LengthUnit.Percent) };

            midiPlayer = root.Q<MidiPlayerElement>();
            generatorElement = root.Q<GeneratorElement>();
            generatorElement.SetParent(this);
            recieverList = root.Q<RecieverListElement>();
            recieverList.SetParent(this);

            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }

        private void OnInspectorUpdate()
        {
            midiPlayer?.Update();
            OnUpdate?.Invoke();
        }

        private void OnDestroy()
        {
            Dispose();
        }  

        private void OnDisable()
        {
            Dispose(); 
        }

        private void OnExit(object sender, EventArgs args) => Dispose();

        private void Dispose()
        {
            Debug.Log("Dispose");
            recieverList?.Dispose();
        }

        public void SaveAs(MuseReciever museReciever)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Midi", museReciever.GetName(), "asset", "Save generated");
            SaveAt(museReciever, path);
        }

        public void Save(MuseReciever museReciever)
        {
            SaveAs(museReciever);
        }

        private void SaveAt(MuseReciever reciever,string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            if (reciever.Tokens == 0)
                return;

            MidiAsset asset = MidiAsset.CreateInstance<MidiAsset>();
            asset.MidiFile = MuseDecoder.DecodeMuseEncoding(reciever.EncodedMidi);

            //int i = 1;
            //string org = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path);
            //string ext = Path.GetExtension(path);
            //while (File.Exists(path)) {
            //    path = org + i + ext;
            //    i++;
            //        }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            AssetDatabase.CreateAsset(asset, path);
        }

        public void Generate(MuseReciever museReciever, int amount)
        {
            amount = Mathf.Max(amount, 1);

            MuseReciever[] recievers = new MuseReciever[amount];
            recievers[0] = museReciever;

            for (int i = 1; i < amount; i++)
            {
                recievers[i] = museReciever.Clone();
            }

            foreach (var r in recievers)
                r.ActivateRecieving();

            recieverList.AddRange(recievers);
        }

        public void TryDeleteReciever(MuseReciever reciever)
        {
            recieverList.Remove(reciever);
        }
    } 
}