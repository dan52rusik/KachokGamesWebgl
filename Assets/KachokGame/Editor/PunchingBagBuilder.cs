using UnityEditor;
using UnityEngine;

namespace Tutorial.Editor
{
    public static class PunchingBagBuilder
    {
        [MenuItem("KachokGame/Create Punching Bag")]
        public static void CreatePunchingBag()
        {
            GameObject anchor = new("PunchingBag_Anchor");
            Undo.RegisterCreatedObjectUndo(anchor, "Create Punching Bag");
            anchor.transform.position = new Vector3(0f, 3.1f, 0f);

            GameObject anchorVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(anchorVisual, "Create Punching Bag Anchor");
            anchorVisual.name = "AnchorVisual";
            anchorVisual.transform.position = anchor.transform.position;
            anchorVisual.transform.localScale = new Vector3(0.08f, 0.10f, 0.08f);
            Object.DestroyImmediate(anchorVisual.GetComponent<Collider>());
            anchorVisual.transform.SetParent(anchor.transform, true);

            GameObject bag = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(bag, "Create Punching Bag Mesh");
            bag.name = "PunchingBag";
            bag.transform.position = new Vector3(0f, 1.7f, 0f);
            bag.transform.localScale = new Vector3(0.7f, 1.15f, 0.7f);

            Rigidbody rb = Undo.AddComponent<Rigidbody>(bag);
            rb.mass = 22f;

            ConfigurableJoint joint = Undo.AddComponent<ConfigurableJoint>(bag);
            joint.connectedBody = null;
            joint.connectedAnchor = anchor.transform.position;

            GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(rope, "Create Punching Bag Rope");
            rope.name = "RopeVisual";
            rope.transform.position = new Vector3(0f, 2.35f, 0f);
            rope.transform.localScale = new Vector3(0.03f, 0.75f, 0.03f);
            Object.DestroyImmediate(rope.GetComponent<Collider>());

            var ropeRenderer = rope.GetComponent<Renderer>();
            if (ropeRenderer != null)
                AssignColorMaterial(ropeRenderer, new Color(0.16f, 0.16f, 0.18f, 1f), "PunchingBag_Rope");

            var bagRenderer = bag.GetComponent<Renderer>();
            if (bagRenderer != null)
                AssignColorMaterial(bagRenderer, new Color(0.78f, 0.12f, 0.09f, 1f), "PunchingBag_Bag");

            var bagScript = Undo.AddComponent<Tutorial.PunchingBag>(bag);
            var anchorTransformField = typeof(Tutorial.PunchingBag).GetField("anchorTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            anchorTransformField?.SetValue(bagScript, anchor.transform);
            var ropeField = typeof(Tutorial.PunchingBag).GetField("ropeVisual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ropeField?.SetValue(bagScript, rope.transform);
            var anchorField = typeof(Tutorial.PunchingBag).GetField("anchorVisual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            anchorField?.SetValue(bagScript, anchorVisual.transform);
            bagScript.ConfigureJoint();

            Selection.activeGameObject = bag;
        }

        private static void AssignColorMaterial(Renderer renderer, Color color, string materialName)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            var material = new Material(shader)
            {
                name = materialName,
                color = color
            };

            renderer.sharedMaterial = material;
        }
    }
}
