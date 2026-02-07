using UnityEngine;
using UnityEditor;
using TheOrder.Hunter;

namespace TheOrder.Editor
{
    public static class SetupHunterAI
    {
        [MenuItem("Tools/Setup Hunter AI References")]
        public static void Setup()
        {
            GameObject hunter = GameObject.Find("Hunter");
            if (hunter == null)
            {
                Debug.LogError("Could not find Hunter GameObject in scene!");
                return;
            }

            HunterAI hunterAI = hunter.GetComponent<HunterAI>();
            if (hunterAI == null)
            {
                Debug.LogError("HunterAI component not found on Hunter!");
                return;
            }

            GameObject waypointParent = GameObject.Find("--- WAYPOINTS ---");
            if (waypointParent == null)
            {
                Debug.LogError("Could not find --- WAYPOINTS --- parent!");
                return;
            }

            SerializedObject serialized = new SerializedObject(hunterAI);

            int childCount = waypointParent.transform.childCount;
            SerializedProperty waypointsProp = serialized.FindProperty("_patrolWaypoints");
            waypointsProp.arraySize = childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = waypointParent.transform.GetChild(i);
                waypointsProp.GetArrayElementAtIndex(i).objectReferenceValue = child;
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(hunterAI);

            Debug.Log("Assigned " + childCount + " waypoints to HunterAI");
        }
    }
}
