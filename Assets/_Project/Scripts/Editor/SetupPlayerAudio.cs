using UnityEditor;
using UnityEngine;

namespace TheOrder.Editor
{
    /// <summary>
    /// Editor utility to wire PlayerAudio references on the Player GameObject.
    /// Menu: Tools > The Order > Setup Player Audio
    /// </summary>
    public static class SetupPlayerAudio
    {
        [MenuItem("Tools/The Order/Setup Player Audio")]
        public static void Setup()
        {
            // Find the Player GameObject
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("[SetupPlayerAudio] Could not find 'Player' GameObject in scene.");
                return;
            }

            // Add PlayerAudio component if missing
            var playerAudio = player.GetComponent<Player.PlayerAudio>();
            if (playerAudio == null)
            {
                playerAudio = Undo.AddComponent<Player.PlayerAudio>(player);
                Debug.Log("[SetupPlayerAudio] Added PlayerAudio component to Player.");
            }

            // Create or find BreathSource child
            Transform breathSourceTransform = player.transform.Find("BreathSource");
            AudioSource breathSource;

            if (breathSourceTransform == null)
            {
                GameObject breathSourceObj = new GameObject("BreathSource");
                Undo.RegisterCreatedObjectUndo(breathSourceObj, "Create BreathSource");
                breathSourceObj.transform.SetParent(player.transform, false);
                breathSource = breathSourceObj.AddComponent<AudioSource>();
            }
            else
            {
                breathSource = breathSourceTransform.GetComponent<AudioSource>();
                if (breathSource == null)
                {
                    breathSource = breathSourceTransform.gameObject.AddComponent<AudioSource>();
                }
            }

            // Configure as 2D source
            breathSource.spatialBlend = 0f;
            breathSource.playOnAwake = false;
            breathSource.loop = false;

            // Load voice clips
            string breathPath = "Assets/Voices - Essentials/Voice_Male/Voice_Male_Breath/";

            AudioClip chaseShock = AssetDatabase.LoadAssetAtPath<AudioClip>(
                breathPath + "Voice_Male_V1_Breath_Shocked_Mono_02.wav");
            AudioClip idleBreathing = AssetDatabase.LoadAssetAtPath<AudioClip>(
                breathPath + "Voice_Male_V1_Breath_Frozen_Loop_Mono.wav");
            AudioClip postChaseBreath = AssetDatabase.LoadAssetAtPath<AudioClip>(
                breathPath + "Voice_Male_V1_Breath_Mouth_Normal_Loop_Mono.wav");

            if (chaseShock == null) Debug.LogWarning("[SetupPlayerAudio] Could not load Breath_Shocked_Mono_02");
            if (idleBreathing == null) Debug.LogWarning("[SetupPlayerAudio] Could not load Breath_Frozen_Loop_Mono");
            if (postChaseBreath == null) Debug.LogWarning("[SetupPlayerAudio] Could not load Breath_Mouth_Normal_Loop_Mono");

            // Wire serialized fields
            SerializedObject so = new SerializedObject(playerAudio);
            so.FindProperty("_chaseShockClip").objectReferenceValue = chaseShock;
            so.FindProperty("_idleBreathingClip").objectReferenceValue = idleBreathing;
            so.FindProperty("_postChaseBreathClip").objectReferenceValue = postChaseBreath;
            so.FindProperty("_breathSource").objectReferenceValue = breathSource;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(playerAudio);

            Debug.Log("[SetupPlayerAudio] Player audio wired successfully! " +
                      $"Shock={chaseShock != null}, Idle={idleBreathing != null}, PostChase={postChaseBreath != null}");
        }
    }
}
