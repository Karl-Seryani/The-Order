using UnityEngine;
using UnityEditor;

namespace TheOrder.Editor
{
    /// <summary>
    /// Generates a 4-floor horror house blockout (2 basements + 2 above ground).
    /// Run from Tools > Generate Granny House.
    /// All geometry uses Unity primitives with dark materials.
    /// </summary>
    public static class GrannyHouseGenerator
    {
        // House dimensions (in Unity units / meters)
        const float HOUSE_W = 20f;   // X width
        const float HOUSE_D = 16f;   // Z depth
        const float FLOOR_H = 3.5f;  // floor-to-floor height
        const float WALL_T  = 0.25f; // wall thickness
        const float SLAB_T  = 0.3f;  // floor slab thickness

        // Floor base Y positions
        static readonly float[] FLOOR_Y = {
            -FLOOR_H * 2, // B2  = -7.0
            -FLOOR_H,     // B1  = -3.5
            0f,           // F1  =  0.0
            FLOOR_H       // F2  =  3.5
        };

        static readonly string[] FLOOR_NAMES = { "B2", "B1", "F1", "F2" };

        static Material _wallMat;
        static Material _floorMat;
        static Material _ceilingMat;
        static Material _stairMat;
        static Material _secretMat;

        [MenuItem("Tools/Generate Granny House")]
        public static void Generate()
        {
            if (!EditorUtility.DisplayDialog("Generate Granny House",
                "This will create the house blockout in the current scene.\n\nContinue?",
                "Generate", "Cancel"))
                return;

            CreateMaterials();

            // Root parent
            var root = new GameObject("=== GRANNY HOUSE ===");
            Undo.RegisterCreatedObjectUndo(root, "Generate Granny House");

            // Ground plane
            CreateBox(root.transform, "GroundPlane", new Vector3(0, -FLOOR_H * 2 - 0.15f, 0),
                new Vector3(30, 0.1f, 24), _floorMat);

            // Build each floor
            for (int i = 0; i < 4; i++)
            {
                var floorParent = CreateEmpty(root.transform, $"--- {FLOOR_NAMES[i]} ---");
                float baseY = FLOOR_Y[i];

                BuildFloorSlab(floorParent.transform, FLOOR_NAMES[i], baseY);
                BuildExteriorWalls(floorParent.transform, FLOOR_NAMES[i], baseY);
                BuildInteriorWalls(floorParent.transform, FLOOR_NAMES[i], baseY, i);
            }

            // Ceiling / Roof above F2
            float roofY = FLOOR_Y[3] + FLOOR_H;
            CreateBox(root.transform, "Roof", new Vector3(0, roofY, 0),
                new Vector3(HOUSE_W + WALL_T * 2, SLAB_T, HOUSE_D + WALL_T * 2), _ceilingMat);

            // Staircases
            var stairsParent = CreateEmpty(root.transform, "--- STAIRS ---");
            // Main stairs: right side of hallway, connecting all floors
            for (int i = 0; i < 3; i++)
            {
                float fromY = FLOOR_Y[i];
                float toY = FLOOR_Y[i + 1];
                // Alternate stair direction per floor for realism
                bool goingForward = (i % 2 == 0);
                BuildStaircase(stairsParent.transform,
                    $"Stairs_{FLOOR_NAMES[i]}_to_{FLOOR_NAMES[i + 1]}",
                    fromY, toY, goingForward, i);
            }

            // Secret passages
            var secretsParent = CreateEmpty(root.transform, "--- SECRETS ---");
            BuildSecretPassages(secretsParent.transform);

            // Hiding spots
            var hidingParent = CreateEmpty(root.transform, "--- HIDING SPOTS ---");
            BuildHidingSpots(hidingParent.transform);

            Selection.activeGameObject = root;
            Debug.Log("[GrannyHouseGenerator] House generated! 4 floors, stairs, secrets, hiding spots.");
        }

        #region Materials

        static void CreateMaterials()
        {
            _wallMat = CreateMat("HouseWall", new Color(0.65f, 0.60f, 0.55f));
            _floorMat = CreateMat("HouseFloor", new Color(0.45f, 0.38f, 0.32f));
            _ceilingMat = CreateMat("HouseCeiling", new Color(0.55f, 0.50f, 0.45f));
            _stairMat = CreateMat("HouseStairs", new Color(0.50f, 0.42f, 0.35f));
            _secretMat = CreateMat("HouseSecret", new Color(0.35f, 0.30f, 0.28f));
        }

        static Material CreateMat(string name, Color color)
        {
            // Try to find URP lit shader first, fall back to standard
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader) { name = name, color = color };

            // For URP, set smoothness low for matte look
            mat.SetFloat("_Smoothness", 0.1f);

            // Save to Assets so it persists
            string dir = "Assets/_Project/Materials/House";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Materials");
                AssetDatabase.CreateFolder("Assets/_Project/Materials", "House");
            }

            string path = $"{dir}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                existing.color = color;
                existing.SetFloat("_Smoothness", 0.1f);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        #endregion

        #region Floor Slab

        static void BuildFloorSlab(Transform parent, string prefix, float baseY)
        {
            CreateBox(parent, $"{prefix}_Floor", new Vector3(0, baseY, 0),
                new Vector3(HOUSE_W, SLAB_T, HOUSE_D), _floorMat);
        }

        #endregion

        #region Exterior Walls

        static void BuildExteriorWalls(Transform parent, string prefix, float baseY)
        {
            float wallH = FLOOR_H;
            float cy = baseY + SLAB_T / 2 + wallH / 2;
            float halfW = HOUSE_W / 2;
            float halfD = HOUSE_D / 2;

            // Front wall (Z-) — with door gap on F1
            if (prefix == "F1")
            {
                // Left section
                CreateBox(parent, $"{prefix}_FrontWall_L",
                    new Vector3(-halfW / 2 - 1f, cy, -halfD),
                    new Vector3(halfW - 2f, wallH, WALL_T), _wallMat);
                // Right section
                CreateBox(parent, $"{prefix}_FrontWall_R",
                    new Vector3(halfW / 2 + 1f, cy, -halfD),
                    new Vector3(halfW - 2f, wallH, WALL_T), _wallMat);
                // Top above door
                CreateBox(parent, $"{prefix}_FrontWall_Top",
                    new Vector3(0, cy + wallH / 2 - 0.6f, -halfD),
                    new Vector3(2f, 1.2f, WALL_T), _wallMat);
            }
            else
            {
                CreateBox(parent, $"{prefix}_FrontWall",
                    new Vector3(0, cy, -halfD),
                    new Vector3(HOUSE_W, wallH, WALL_T), _wallMat);
            }

            // Back wall (Z+)
            CreateBox(parent, $"{prefix}_BackWall",
                new Vector3(0, cy, halfD),
                new Vector3(HOUSE_W, wallH, WALL_T), _wallMat);

            // Left wall (X-)
            CreateBox(parent, $"{prefix}_LeftWall",
                new Vector3(-halfW, cy, 0),
                new Vector3(WALL_T, wallH, HOUSE_D), _wallMat);

            // Right wall (X+)
            CreateBox(parent, $"{prefix}_RightWall",
                new Vector3(halfW, cy, 0),
                new Vector3(WALL_T, wallH, HOUSE_D), _wallMat);
        }

        #endregion

        #region Interior Walls

        static void BuildInteriorWalls(Transform parent, string prefix, float baseY, int floorIndex)
        {
            float wallH = FLOOR_H - SLAB_T;
            float cy = baseY + SLAB_T / 2 + wallH / 2 + SLAB_T / 2;

            switch (floorIndex)
            {
                case 0: BuildB2Interior(parent, prefix, cy, wallH); break;
                case 1: BuildB1Interior(parent, prefix, cy, wallH); break;
                case 2: BuildF1Interior(parent, prefix, cy, wallH); break;
                case 3: BuildF2Interior(parent, prefix, cy, wallH); break;
            }
        }

        // B2: Dungeon — boiler room (right), cell block (left back), tunnel corridor (left front)
        static void BuildB2Interior(Transform parent, string prefix, float cy, float h)
        {
            // Central hallway wall left (X = -3) running Z direction, with doorway gap
            CreateBox(parent, $"{prefix}_HallL_Front",
                new Vector3(-3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallL_Back",
                new Vector3(-3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            // Central hallway wall right (X = 3) with doorway gap
            CreateBox(parent, $"{prefix}_HallR_Front",
                new Vector3(3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallR_Back",
                new Vector3(3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            // Boiler room divider (right side, Z = 2)
            CreateBox(parent, $"{prefix}_BoilerDiv",
                new Vector3(6.5f, cy, 2f), new Vector3(7f, h, WALL_T), _wallMat);

            // Cell block dividers (left side)
            CreateBox(parent, $"{prefix}_Cell1",
                new Vector3(-6.5f, cy, 0f), new Vector3(7f, h, WALL_T), _wallMat);
            CreateBox(parent, $"{prefix}_Cell2",
                new Vector3(-6.5f, cy, 4f), new Vector3(7f, h, WALL_T), _wallMat);
        }

        // B1: Storage — wine cellar (left), storage rooms (right), central corridor
        static void BuildB1Interior(Transform parent, string prefix, float cy, float h)
        {
            // Hallway walls
            CreateBox(parent, $"{prefix}_HallL_Front",
                new Vector3(-3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallL_Back",
                new Vector3(-3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            CreateBox(parent, $"{prefix}_HallR_Front",
                new Vector3(3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallR_Back",
                new Vector3(3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            // Wine cellar divider (left, Z = -2)
            CreateBox(parent, $"{prefix}_WineCellarDiv",
                new Vector3(-6.5f, cy, -2f), new Vector3(7f, h, WALL_T), _wallMat);

            // Storage room divider (right, Z = 0)
            CreateBox(parent, $"{prefix}_StorageDiv",
                new Vector3(6.5f, cy, 0f), new Vector3(7f, h, WALL_T), _wallMat);

            // Additional storage split (right, Z = -4)
            CreateBox(parent, $"{prefix}_StorageDiv2",
                new Vector3(6.5f, cy, -4f), new Vector3(7f, h, WALL_T), _wallMat);
        }

        // F1: Main floor — living room (left front), kitchen (right front),
        //     dining (right back), study (left back), central hallway
        static void BuildF1Interior(Transform parent, string prefix, float cy, float h)
        {
            // Hallway walls with doorway gaps
            // Left hallway wall — front section, gap for living room door, back section
            CreateBox(parent, $"{prefix}_HallL_Front",
                new Vector3(-3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallL_Back",
                new Vector3(-3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            // Right hallway wall
            CreateBox(parent, $"{prefix}_HallR_Front",
                new Vector3(3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallR_Back",
                new Vector3(3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            // Kitchen / Dining divider (right side, Z = 0)
            CreateBox(parent, $"{prefix}_KitchenDiv_L",
                new Vector3(5f, cy, 0f), new Vector3(4f, h, WALL_T), _wallMat);
            CreateBox(parent, $"{prefix}_KitchenDiv_R",
                new Vector3(8.5f, cy, 0f), new Vector3(3f, h, WALL_T), _wallMat);

            // Living / Study divider (left side, Z = 0)
            CreateBox(parent, $"{prefix}_LivingDiv_L",
                new Vector3(-5f, cy, 0f), new Vector3(4f, h, WALL_T), _wallMat);
            CreateBox(parent, $"{prefix}_LivingDiv_R",
                new Vector3(-8.5f, cy, 0f), new Vector3(3f, h, WALL_T), _wallMat);
        }

        // F2: Upper floor — master bedroom (left), kid's room (right front),
        //     bathroom (right back), small closet, hallway
        static void BuildF2Interior(Transform parent, string prefix, float cy, float h)
        {
            // Hallway walls
            CreateBox(parent, $"{prefix}_HallL_Front",
                new Vector3(-3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallL_Back",
                new Vector3(-3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            CreateBox(parent, $"{prefix}_HallR_Front",
                new Vector3(3f, cy, -5.5f), new Vector3(WALL_T, h, 5f), _wallMat);
            CreateBox(parent, $"{prefix}_HallR_Back",
                new Vector3(3f, cy, 4.5f), new Vector3(WALL_T, h, 7f), _wallMat);

            // Master bedroom divider (left, Z = 2) — large room
            CreateBox(parent, $"{prefix}_MasterDiv",
                new Vector3(-6.5f, cy, 2f), new Vector3(7f, h, WALL_T), _wallMat);

            // Kid's room / Bathroom divider (right, Z = 0)
            CreateBox(parent, $"{prefix}_KidBathDiv",
                new Vector3(6.5f, cy, 0f), new Vector3(7f, h, WALL_T), _wallMat);

            // Bathroom back wall (right, Z = 4)
            CreateBox(parent, $"{prefix}_BathBackDiv",
                new Vector3(6.5f, cy, 4f), new Vector3(7f, h, WALL_T), _wallMat);

            // Small closet (end of hallway, Z = 6)
            CreateBox(parent, $"{prefix}_ClosetWall",
                new Vector3(0f, cy, 6f), new Vector3(6f, h, WALL_T), _wallMat);
        }

        #endregion

        #region Staircases

        static void BuildStaircase(Transform parent, string name, float fromY, float toY,
            bool goingForward, int floorIndex)
        {
            var stairParent = CreateEmpty(parent, name);
            int steps = 12;
            float totalRise = toY - fromY;
            float stepH = totalRise / steps;
            float stepD = 0.4f;
            float stepW = 1.8f;

            // Position stairs in the hallway — right side, alternating Z direction
            float startX = 0f; // center of hallway
            float startZ;

            if (goingForward)
                startZ = -2f;
            else
                startZ = 2f + (steps - 1) * stepD;

            float zDir = goingForward ? 1f : -1f;

            for (int i = 0; i < steps; i++)
            {
                float y = fromY + SLAB_T + stepH * i + stepH / 2;
                float z = startZ + i * stepD * zDir;

                // Each step is a box that extends from bottom to current height
                CreateBox(stairParent.transform, $"Step_{i}",
                    new Vector3(startX, y - stepH * i / 2, z),
                    new Vector3(stepW, stepH * (i + 1), stepD), _stairMat);
            }

            // Stair walls (railings)
            float midY = (fromY + toY) / 2 + SLAB_T / 2;
            float midZ = startZ + (steps / 2) * stepD * zDir;
            float stairLen = steps * stepD;

            CreateBox(stairParent.transform, "Rail_L",
                new Vector3(startX - stepW / 2 - 0.05f, midY, midZ),
                new Vector3(0.1f, totalRise + 1f, stairLen), _wallMat);
            CreateBox(stairParent.transform, "Rail_R",
                new Vector3(startX + stepW / 2 + 0.05f, midY, midZ),
                new Vector3(0.1f, totalRise + 1f, stairLen), _wallMat);
        }

        #endregion

        #region Secret Passages

        static void BuildSecretPassages(Transform parent)
        {
            // 1. B2 secret tunnel — behind cell wall, runs along the left exterior
            //    A narrow passage outside the cell block connecting to a hidden room
            var tunnel = CreateEmpty(parent, "Secret_B2_Tunnel");
            float tunnelY = FLOOR_Y[0] + SLAB_T / 2 + 1.25f;
            // Tunnel floor
            CreateBox(tunnel.transform, "Floor",
                new Vector3(-9f, FLOOR_Y[0] + SLAB_T / 2 + 0.1f, 5f),
                new Vector3(1.5f, 0.2f, 6f), _secretMat);
            // Tunnel walls
            CreateBox(tunnel.transform, "Wall_L",
                new Vector3(-9.75f, tunnelY, 5f),
                new Vector3(WALL_T, 2.5f, 6f), _secretMat);
            CreateBox(tunnel.transform, "Wall_R",
                new Vector3(-8.25f, tunnelY, 5f),
                new Vector3(WALL_T, 2.5f, 6f), _secretMat);
            CreateBox(tunnel.transform, "Ceiling",
                new Vector3(-9f, FLOOR_Y[0] + SLAB_T / 2 + 2.5f, 5f),
                new Vector3(1.5f, 0.2f, 6f), _secretMat);

            // 2. F1 hidden passage behind bookshelf — narrow corridor between
            //    living room and study, running parallel to the hallway wall
            var bookshelf = CreateEmpty(parent, "Secret_F1_BookshelfPassage");
            float passY = FLOOR_Y[2] + SLAB_T / 2 + 1.4f;
            CreateBox(bookshelf.transform, "Wall_Outer",
                new Vector3(-3.8f, passY, 0f),
                new Vector3(WALL_T, 2.8f, 4f), _secretMat);
            CreateBox(bookshelf.transform, "Wall_Inner",
                new Vector3(-2.4f, passY, 0f),
                new Vector3(WALL_T, 2.8f, 4f), _secretMat);
            CreateBox(bookshelf.transform, "Ceiling",
                new Vector3(-3.1f, FLOOR_Y[2] + SLAB_T / 2 + 2.8f, 0f),
                new Vector3(1.4f, 0.15f, 4f), _secretMat);

            // 3. Vent shaft from F2 closet dropping to B1
            //    A vertical shaft with a small opening
            var vent = CreateEmpty(parent, "Secret_VentShaft_F2_to_B1");
            float shaftCenterY = (FLOOR_Y[1] + FLOOR_Y[3]) / 2 + FLOOR_H / 2;
            float shaftHeight = FLOOR_Y[3] - FLOOR_Y[1] + FLOOR_H;
            CreateBox(vent.transform, "Wall_N",
                new Vector3(1.5f, shaftCenterY, 6.6f),
                new Vector3(1.2f, shaftHeight, WALL_T), _secretMat);
            CreateBox(vent.transform, "Wall_S",
                new Vector3(1.5f, shaftCenterY, 5.4f),
                new Vector3(1.2f, shaftHeight, WALL_T), _secretMat);
            CreateBox(vent.transform, "Wall_W",
                new Vector3(0.9f, shaftCenterY, 6f),
                new Vector3(WALL_T, shaftHeight, 1.2f), _secretMat);
            CreateBox(vent.transform, "Wall_E",
                new Vector3(2.1f, shaftCenterY, 6f),
                new Vector3(WALL_T, shaftHeight, 1.2f), _secretMat);

            // 4. B1 crawlspace under the wine cellar connecting to B2 tunnel
            var crawl = CreateEmpty(parent, "Secret_B1_Crawlspace");
            float crawlY = FLOOR_Y[1] - 0.5f;
            CreateBox(crawl.transform, "Wall_L",
                new Vector3(-9.5f, crawlY, -3f),
                new Vector3(WALL_T, 1.2f, 2f), _secretMat);
            CreateBox(crawl.transform, "Wall_R",
                new Vector3(-8.5f, crawlY, -3f),
                new Vector3(WALL_T, 1.2f, 2f), _secretMat);
            CreateBox(crawl.transform, "Ceiling",
                new Vector3(-9f, crawlY + 0.6f, -3f),
                new Vector3(1f, 0.15f, 2f), _secretMat);
        }

        #endregion

        #region Hiding Spots

        static void BuildHidingSpots(Transform parent)
        {
            // Alcoves built into walls — small recessed spaces a player can duck into

            // F1: Under-stair closet
            var underStair = CreateEmpty(parent, "HidingSpot_F1_UnderStairs");
            float f1y = FLOOR_Y[2] + SLAB_T / 2;
            CreateBox(underStair.transform, "Back",
                new Vector3(1.5f, f1y + 1f, -3.5f),
                new Vector3(WALL_T, 2f, 1.5f), _wallMat);
            CreateBox(underStair.transform, "Side",
                new Vector3(0.8f, f1y + 1f, -4.25f),
                new Vector3(1.5f, 2f, WALL_T), _wallMat);

            // F2: Closet alcove at end of hallway
            var closetAlcove = CreateEmpty(parent, "HidingSpot_F2_Closet");
            float f2y = FLOOR_Y[3] + SLAB_T / 2;
            CreateBox(closetAlcove.transform, "Back",
                new Vector3(-1.5f, f2y + 1f, 7.5f),
                new Vector3(1.5f, 2f, WALL_T), _wallMat);
            CreateBox(closetAlcove.transform, "Side_L",
                new Vector3(-2.25f, f2y + 1f, 6.85f),
                new Vector3(WALL_T, 2f, 1.3f), _wallMat);
            CreateBox(closetAlcove.transform, "Side_R",
                new Vector3(-0.75f, f2y + 1f, 6.85f),
                new Vector3(WALL_T, 2f, 1.3f), _wallMat);

            // B1: Storage room recess
            var storageRecess = CreateEmpty(parent, "HidingSpot_B1_StorageRecess");
            float b1y = FLOOR_Y[1] + SLAB_T / 2;
            CreateBox(storageRecess.transform, "Back",
                new Vector3(8f, b1y + 1f, -6f),
                new Vector3(WALL_T, 2f, 1.5f), _wallMat);
            CreateBox(storageRecess.transform, "Side",
                new Vector3(8.5f, b1y + 1f, -6.75f),
                new Vector3(1f, 2f, WALL_T), _wallMat);

            // B2: Behind boiler alcove
            var boilerAlcove = CreateEmpty(parent, "HidingSpot_B2_BehindBoiler");
            float b2y = FLOOR_Y[0] + SLAB_T / 2;
            CreateBox(boilerAlcove.transform, "Back",
                new Vector3(8f, b2y + 1f, 6f),
                new Vector3(WALL_T, 2f, 1.5f), _wallMat);
            CreateBox(boilerAlcove.transform, "Side",
                new Vector3(8.5f, b2y + 1f, 5.25f),
                new Vector3(1f, 2f, WALL_T), _wallMat);
        }

        #endregion

        #region Helpers

        static GameObject CreateBox(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.isStatic = true;

            var rend = go.GetComponent<Renderer>();
            if (rend != null && mat != null)
                rend.sharedMaterial = mat;

            return go;
        }

        static GameObject CreateEmpty(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            return go;
        }

        #endregion
    }
}
