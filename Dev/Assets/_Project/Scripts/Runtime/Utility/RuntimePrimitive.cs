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
