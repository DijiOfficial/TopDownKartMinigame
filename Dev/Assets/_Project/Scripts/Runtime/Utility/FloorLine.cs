using UnityEngine;
using UnityEngine.Rendering;

namespace Battlekart
{
    public static class FloorLine
    {
        private static Material _material;

        public static LineRenderer Create(string name, float width)
        {
            if (_material == null)
            {
                _material = new Material(Shader.Find("Sprites/Default"));
            }

            GameObject lineObject = new GameObject(name);
            lineObject.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.alignment = LineAlignment.TransformZ;
            line.sharedMaterial = _material;
            line.widthMultiplier = width;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        public static void SetArc(LineRenderer line, Vector3 center, float radius, float fromDegrees, float toDegrees, int segments)
        {
            line.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float radians = Mathf.Lerp(fromDegrees, toDegrees, i / (float)segments) * Mathf.Deg2Rad;
                line.SetPosition(i, center + new Vector3(Mathf.Sin(radians) * radius, 0.0f, Mathf.Cos(radians) * radius));
            }
        }

        public static void SetColor(LineRenderer line, Color color)
        {
            line.startColor = color;
            line.endColor = color;
        }

        public static void Destroy(LineRenderer line)
        {
            if (line != null)
            {
                Object.Destroy(line.gameObject);
            }
        }
    }
}
