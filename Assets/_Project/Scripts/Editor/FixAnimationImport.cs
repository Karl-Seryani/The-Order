using UnityEditor;
using UnityEngine;

namespace TheOrder.Editor
{
    /// <summary>
    /// Fixes Mixamo animation imports: strips baked root position Y
    /// that causes the Hunter to float during walk/run animations.
    /// Run via Tools > The Order > Fix Animation Import.
    /// </summary>
    public static class FixAnimationImport
    {
        [MenuItem("Tools/The Order/Fix Animation Import")]
        public static void Fix()
        {
            string[] paths = new[]
            {
                "Assets/_Project/Animations/Walking.fbx",
                "Assets/_Project/Animations/Running.fbx",
                "Assets/_Project/Animations/Looking Around.fbx"
            };

            foreach (string path in paths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[FixAnimationImport] Could not find: {path}");
                    continue;
                }

                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                if (clips.Length == 0)
                {
                    clips = importer.defaultClipAnimations;
                }

                bool changed = false;
                foreach (var clip in clips)
                {
                    if (clip.keepOriginalPositionY || clip.keepOriginalPositionXZ)
                    {
                        clip.keepOriginalPositionY = false;
                        clip.keepOriginalPositionXZ = false;
                        changed = true;
                    }
                }

                if (changed)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                    Debug.Log($"[FixAnimationImport] {path} — stripped baked root position Y/XZ");
                }
                else
                {
                    Debug.Log($"[FixAnimationImport] {path} — already correct");
                }
            }

            Debug.Log("[FixAnimationImport] Done.");
        }
    }
}
