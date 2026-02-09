using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TheOrder.Editor
{
    /// <summary>
    /// One-click utility: finds interactive furniture parts (doors, drawers) by name,
    /// adds SlidableFurniture component, and unchecks Static.
    /// Also has a cleanup to undo misplaced components.
    /// </summary>
    public static class SetupInteractiveFurniture
    {
        /// <summary>
        /// Exact name prefixes for objects that should be slidable.
        /// Only doors/drawers — never parent frames, bodies, lights, or cases.
        /// </summary>
        private static readonly string[] SLIDABLE_PREFIXES = new[]
        {
            "SheetRackCase_",    // individual drawers inside SheetRack
            "Cupboard_Door_",    // cupboard door panels
            "MirrorShelf_Door",  // MirrorShelf_DoorL, MirrorShelf_DoorR
            "MedRackDoor_",      // MedRackDoor_L, MedRackDoor_R
            "Case_Door_",        // CaseMetallic door panels (Case_Door_L, Case_Door_R)
        };

        /// <summary>
        /// Names that should NEVER have SlidableFurniture (parent frames, bodies, lights).
        /// </summary>
        private static readonly string[] EXCLUDED_NAMES = new[]
        {
            "SheetRack",
            "MirrorShelf_Case",
            "CupboardLight",
            "MedRack",
        };

        [MenuItem("Tools/The Order/Setup Interactive Furniture")]
        public static void Setup()
        {
            int added = 0;
            int unchecked_ = 0;

            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

            foreach (var t in allTransforms)
            {
                string name = t.gameObject.name;
                if (!IsSlidable(name)) continue;

                // Uncheck Static
                if (t.gameObject.isStatic)
                {
                    Undo.RecordObject(t.gameObject, "Setup Furniture");
                    t.gameObject.isStatic = false;
                    EditorUtility.SetDirty(t.gameObject);
                    unchecked_++;
                }

                // Add SlidableFurniture if not already present
                if (!t.TryGetComponent<Doors.SlidableFurniture>(out _))
                {
                    Undo.AddComponent<Doors.SlidableFurniture>(t.gameObject);
                    added++;
                }
            }

            // Sync slide direction and distance on ALL existing SlidableFurniture components
            int updated = 0;
            var allFurniture = Object.FindObjectsByType<Doors.SlidableFurniture>(FindObjectsSortMode.None);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var distField = typeof(Doors.SlidableFurniture).GetField("_slideDistance", flags);
            var dirField = typeof(Doors.SlidableFurniture).GetField("_slideDirection", flags);

            foreach (var comp in allFurniture)
            {
                bool changed = false;

                if (distField != null && !Mathf.Approximately((float)distField.GetValue(comp), 0.35f))
                {
                    distField.SetValue(comp, 0.35f);
                    changed = true;
                }

                if (dirField != null && (Vector3)dirField.GetValue(comp) != Vector3.back)
                {
                    dirField.SetValue(comp, Vector3.back);
                    changed = true;
                }

                if (changed)
                {
                    Undo.RecordObject(comp, "Sync Furniture Settings");
                    EditorUtility.SetDirty(comp);
                    updated++;
                }
            }

            Debug.Log($"[SetupFurniture] Done — added {added}, unchecked Static {unchecked_}, synced {updated}.");
        }

        [MenuItem("Tools/The Order/Cleanup Misplaced Furniture Components")]
        public static void Cleanup()
        {
            int removed = 0;
            var all = Object.FindObjectsByType<Doors.SlidableFurniture>(FindObjectsSortMode.None);

            foreach (var comp in all)
            {
                string name = comp.gameObject.name;
                if (!IsSlidable(name))
                {
                    Debug.Log($"[SetupFurniture] Removing SlidableFurniture from: {name}", comp.gameObject);
                    Undo.DestroyObjectImmediate(comp);
                    removed++;
                }
            }

            Debug.Log($"[SetupFurniture] Cleanup done — removed {removed} misplaced components.");
        }

        private static bool IsSlidable(string name)
        {
            // Check exclusions first
            foreach (var excluded in EXCLUDED_NAMES)
            {
                if (name == excluded || name.StartsWith(excluded + " ("))
                    return false;
            }

            // Check if it matches a slidable prefix
            foreach (var prefix in SLIDABLE_PREFIXES)
            {
                if (name.StartsWith(prefix)) return true;
            }

            return false;
        }
    }
}
