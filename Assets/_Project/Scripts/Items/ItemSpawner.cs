using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// Shared utility for spawning item pickups in the world.
    /// Used by HeldItemController (drop) and ToolReceiver (reward).
    /// </summary>
    public static class ItemSpawner
    {
        /// <summary>
        /// Spawn a world pickup for the given item at the specified position.
        /// Adds a fitted BoxCollider, Rigidbody with gravity, and ItemPickup component.
        /// </summary>
        public static GameObject SpawnPickup(ItemData item, Vector3 position)
        {
            GameObject pickupGo;

            if (item.MeshPrefab != null)
            {
                pickupGo = Object.Instantiate(item.MeshPrefab, position, Quaternion.identity);
            }
            else
            {
                pickupGo = new GameObject($"Pickup_{item.DisplayName}");
                pickupGo.transform.position = position;
            }

            pickupGo.transform.localScale = item.MeshScale;

            // Strip existing colliders and add a mesh-fitted BoxCollider
            foreach (var c in pickupGo.GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
                Object.Destroy(c);
            }

            var box = pickupGo.AddComponent<BoxCollider>();
            FitBoxColliderToMesh(pickupGo, box);

            // Load physics material from Resources
            var physicsMat = Resources.Load<PhysicsMaterial>("ItemPhysics");
            if (physicsMat != null)
            {
                box.material = physicsMat;
            }

            // Add physics
            var rb = pickupGo.GetComponent<Rigidbody>();
            if (rb == null)
                rb = pickupGo.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 1.5f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 1.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Add ItemPickup component
            var pickup = pickupGo.GetComponent<ItemPickup>();
            if (pickup == null)
                pickup = pickupGo.AddComponent<ItemPickup>();
            pickup.Initialize(item);

            // Add collision audio for drop impact sounds
            if (item.ImpactClip != null)
            {
                var collisionAudio = pickupGo.AddComponent<ItemCollisionAudio>();
                collisionAudio.SetImpactClip(item.ImpactClip, item.ImpactVolumeMultiplier);
            }

            pickupGo.name = $"Pickup_{item.DisplayName}";
            return pickupGo;
        }

        /// <summary>Fit a BoxCollider to the combined mesh bounds of all renderers.</summary>
        public static void FitBoxColliderToMesh(GameObject go, BoxCollider box)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                box.size = Vector3.one * 0.15f;
                box.center = Vector3.zero;
                return;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            box.center = go.transform.InverseTransformPoint(bounds.center);
            var localSize = bounds.size;
            var scale = go.transform.lossyScale;
            
            // Prevent zero or near-zero divisions
            float safeScaleX = Mathf.Abs(scale.x) > 0.001f ? scale.x : 1f;
            float safeScaleY = Mathf.Abs(scale.y) > 0.001f ? scale.y : 1f;
            float safeScaleZ = Mathf.Abs(scale.z) > 0.001f ? scale.z : 1f;
            
            box.size = new Vector3(
                localSize.x / safeScaleX,
                localSize.y / safeScaleY,
                localSize.z / safeScaleZ
            );

            // Add slight padding to make pickup easier
            const float padding = 1.15f;
            box.size *= padding;
        }
    }
}
