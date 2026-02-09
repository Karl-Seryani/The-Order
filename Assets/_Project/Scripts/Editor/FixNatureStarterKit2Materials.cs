using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheOrder.Editor
{
    public static class FixNatureStarterKit2Materials
    {
        private const string NatureRoot = "Assets/NatureStarterKit2/Nature";

        [MenuItem("Tools/The Order/Fix NatureStarterKit2 Purple Materials")]
        public static void RunFromMenu()
        {
            RunInternal();
        }

        // Supports batch mode:
        // -executeMethod TheOrder.Editor.FixNatureStarterKit2Materials.RunFromCommandLine
        public static void RunFromCommandLine()
        {
            RunInternal();
        }

        private static void RunInternal()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[NatureStarterKit2] URP/Lit shader was not found.");
                return;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { NatureRoot });
            if (prefabGuids.Length == 0)
            {
                Debug.LogWarning($"[NatureStarterKit2] No prefabs found under {NatureRoot}.");
                return;
            }

            HashSet<string> visitedMaterials = new HashSet<string>();
            int changedPrefabs = 0;
            int changedMaterials = 0;
            int scannedPrefabs = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                scannedPrefabs++;
                bool prefabChanged = false;

                // Embedded materials inside the prefab file.
                Object[] embeddedAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                foreach (Object asset in embeddedAssets)
                {
                    if (asset is not Material mat)
                    {
                        continue;
                    }

                    string key = MaterialKey(mat);
                    if (!visitedMaterials.Add(key))
                    {
                        continue;
                    }

                    if (TryUpgradeMaterial(mat, urpLit))
                    {
                        changedMaterials++;
                        prefabChanged = true;
                    }
                }

                if (prefabChanged)
                {
                    EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(path));
                    changedPrefabs++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[NatureStarterKit2] Scanned {scannedPrefabs} prefabs. Fixed {changedMaterials} materials across {changedPrefabs} prefabs.");
        }

        private static bool NeedsUpgrade(Material mat)
        {
            if (mat.shader == null)
            {
                return true;
            }

            string shaderName = mat.shader.name;
            if (shaderName == "Hidden/InternalErrorShader")
            {
                return true;
            }

            if (shaderName.StartsWith("Universal Render Pipeline/"))
            {
                return false;
            }

            return true;
        }

        private static bool IsLeafMaterial(Material mat)
        {
            string name = mat.name.ToLowerInvariant();
            if (name.Contains("leaf"))
            {
                return true;
            }

            return mat.HasProperty("_ShadowTex");
        }

        private static void UpgradeToUrpLit(Material mat, Shader urpLit, bool isLeaf)
        {
            Texture baseMap = null;
            Texture bumpMap = null;
            Color baseColor = Color.white;
            float cutoff = 0.3f;

            if (mat.HasProperty("_MainTex"))
            {
                baseMap = mat.GetTexture("_MainTex");
            }
            if (mat.HasProperty("_BumpSpecMap"))
            {
                bumpMap = mat.GetTexture("_BumpSpecMap");
            }
            if (mat.HasProperty("_Color"))
            {
                baseColor = mat.GetColor("_Color");
            }
            if (mat.HasProperty("_Cutoff"))
            {
                cutoff = mat.GetFloat("_Cutoff");
            }

            mat.shader = urpLit;

            if (mat.HasProperty("_BaseMap") && baseMap != null)
            {
                mat.SetTexture("_BaseMap", baseMap);
            }
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", baseColor);
            }
            if (mat.HasProperty("_BumpMap") && bumpMap != null)
            {
                mat.SetTexture("_BumpMap", bumpMap);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (isLeaf)
            {
                if (mat.HasProperty("_AlphaClip"))
                {
                    mat.SetFloat("_AlphaClip", 1f);
                }
                if (mat.HasProperty("_Cutoff"))
                {
                    mat.SetFloat("_Cutoff", Mathf.Clamp(cutoff, 0.1f, 0.9f));
                }
                if (mat.HasProperty("_Cull"))
                {
                    mat.SetFloat("_Cull", (float)CullMode.Off);
                }
                mat.EnableKeyword("_ALPHATEST_ON");
            }
            else
            {
                if (mat.HasProperty("_AlphaClip"))
                {
                    mat.SetFloat("_AlphaClip", 0f);
                }
                if (mat.HasProperty("_Cull"))
                {
                    mat.SetFloat("_Cull", (float)CullMode.Back);
                }
                mat.DisableKeyword("_ALPHATEST_ON");
            }
        }

        private static bool TryUpgradeMaterial(Material mat, Shader urpLit)
        {
            if (mat == null || !NeedsUpgrade(mat))
            {
                return false;
            }

            bool isLeaf = IsLeafMaterial(mat);
            UpgradeToUrpLit(mat, urpLit, isLeaf);
            EditorUtility.SetDirty(mat);
            return true;
        }

        private static string MaterialKey(Material mat)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mat, out string guid, out long localId))
            {
                return $"{guid}:{localId}";
            }

            return $"instance:{mat.GetInstanceID()}";
        }
    }
}
