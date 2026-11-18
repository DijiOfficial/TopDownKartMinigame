using UnityEngine;

namespace Battlekart
{
    public static class RendererTint
    {
        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");
        private static readonly MaterialPropertyBlock _block = new();

        public static void Apply(Renderer renderer, Color color)
        {
            renderer.GetPropertyBlock(_block);
            _block.SetColor(_baseColorId, color);
            _block.SetColor(_colorId, color);
            renderer.SetPropertyBlock(_block);
        }

        public static void Apply(Renderer[] renderers, Color color)
        {
            foreach (Renderer renderer in renderers)
            {
                Apply(renderer, color);
            }
        }
    }
}
