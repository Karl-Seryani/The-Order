using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Extension methods for Transform used by the run persistence system.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Returns a stable hierarchy path string for use as a persistence ID.
        /// Example: "Asylum/Floor1/Barricade_Door"
        /// </summary>
        public static string GetPersistenceId(this Transform t)
        {
            if (t.parent == null)
                return t.name;

            // Build path from root to this transform
            var sb = new System.Text.StringBuilder(128);
            BuildPath(t, sb);
            return sb.ToString();
        }

        private static void BuildPath(Transform t, System.Text.StringBuilder sb)
        {
            if (t.parent != null)
            {
                BuildPath(t.parent, sb);
                sb.Append('/');
            }
            sb.Append(t.name);
        }
    }
}
