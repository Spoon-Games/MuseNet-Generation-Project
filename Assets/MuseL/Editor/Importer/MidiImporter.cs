using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace MuseL
{
    [ScriptedImporter(1, "mid")]
    public class MidiImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            byte[] mideFile = File.ReadAllBytes(ctx.assetPath);

            MidiAsset asset = MidiAsset.CreateInstance<MidiAsset>();
            asset.SetMidiFileRaw(mideFile);

            AssetDatabase.CreateAsset(asset, Path.GetDirectoryName(ctx.assetPath) +Path.DirectorySeparatorChar+ Path.GetFileNameWithoutExtension(ctx.assetPath)+".asset");

            File.Delete(ctx.assetPath); 
        }
    } 
}
