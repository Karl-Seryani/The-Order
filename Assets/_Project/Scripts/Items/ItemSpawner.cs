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

            // Add physics
            var rb = pickupGo.GetComponent<Rigidbody>();
            if (rb == null)
                rb = pickupGo.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Add ItemPickup component
            var pickup = pickupGo.GetComponent<ItemPickup>();
            if (pickup == null)
                pickup = pickupGo.AddComponent<ItemPickup>();
            pickup.Initialize(item);

            pickupGo.name = $"Pickup_{item.DisplayName}";
            return pickupGo;
        }

        /// <summary>Fit a BoxCollider to the combined mesh bounds of all renderers.</summary>
        public static void FitBoxColliderToMesh(GameObject go, BoxCollider box)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                box.size = Vector3.one * 0.1f;
                return;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            box.center = go.transform.InverseTransformPoint(bounds.center);
            var localSize = bounds.size;
            var scale = go.transform.lossyScale;
            box.size = new Vector3(
                scale.x != 0f ? localSize.x / scale.x : localSize.x,
                scale.y != 0f ? localSize.y / scale.y : localSize.y,
                scale.z != 0f ? localSize.z / scale.z : localSize.z
            );
        }
    }
}
