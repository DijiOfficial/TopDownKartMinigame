using UnityEngine;

namespace Battlekart
{
    public static class RuntimePrimitive
    {
        private const string LitShaderName = "Universal Render Pipeline/Lit";

        private static Material _material;

        public static GameObject Create(PrimitiveType type)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            Material material = GetMaterial();
            if (material is not null)
            {
                primitive.GetComponent<Renderer>().sharedMaterial = material;
            }
            return primitive;
        }

        public static void ExtendColliderUp(GameObject primitive, float heightMultiplier)
        {
            if (primitive.TryGetComponent(out BoxCollider box))
            {
                box.size = new Vector3(1.0f, heightMultiplier, 1.0f);
                box.center = new Vector3(0.0f, (heightMultiplier - 1.0f) * 0.5f, 0.0f);
            }
        }

        private static Material GetMaterial()
        {
            if (_material == null)
            {
                Shader shader = Shader.Find(LitShaderName);
                if (shader == null)
                {
                    return null;
                }
                _material = new Material(shader);
            }
            return _material;
        }
    }
}
