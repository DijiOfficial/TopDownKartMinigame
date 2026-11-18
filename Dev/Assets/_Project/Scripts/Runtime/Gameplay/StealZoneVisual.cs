using UnityEngine;

namespace Battlekart
{
    public class StealZoneVisual
    {
        private const int ArcSegments = 24;

        private readonly LineRenderer _outline;
        private readonly LineRenderer _fill;
        private readonly float _height;
        private readonly Vector3[] _fillPoints = new Vector3[ArcSegments + 1];

        private float _outlineInner = -1.0f;
        private float _outlineOuter = -1.0f;
        private float _outlineAngle = -1.0f;

        public StealZoneVisual(Transform owner, Material material, float height, float outlineWidth, float fillWidth)
        {
            _height = height;
            _outline = CreateLine("Steal Zone Outline", owner, material, outlineWidth, true);
            _fill = CreateLine("Steal Zone Fill", owner, material, fillWidth, false);
        }

        public void SetOutline(bool visible, float innerRadius, float outerRadius, float halfAngle, Color color)
        {
            _outline.enabled = visible;
            if (visible == false)
            {
                return;
            }

            _outline.startColor = color;
            _outline.endColor = color;

            if (Mathf.Approximately(innerRadius, _outlineInner) && Mathf.Approximately(outerRadius, _outlineOuter) && Mathf.Approximately(halfAngle, _outlineAngle))
            {
                return;
            }

            _outlineInner = innerRadius;
            _outlineOuter = outerRadius;
            _outlineAngle = halfAngle;

            Vector3[] points = new Vector3[(ArcSegments + 1) * 2];
            for (int i = 0; i <= ArcSegments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)ArcSegments);
                points[i] = PointOnArc(outerRadius, angle);
                points[points.Length - 1 - i] = PointOnArc(innerRadius, angle);
            }
            _outline.positionCount = points.Length;
            _outline.SetPositions(points);
        }

        public void SetFill(float normalizedProgress, float radius, float halfAngle, Color color)
        {
            bool visible = normalizedProgress > 0.0f;
            _fill.enabled = visible;
            if (visible == false)
            {
                return;
            }

            _fill.startColor = color;
            _fill.endColor = color;

            float endAngle = Mathf.Lerp(-halfAngle, halfAngle, Mathf.Clamp01(normalizedProgress));
            for (int i = 0; i <= ArcSegments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, endAngle, i / (float)ArcSegments);
                _fillPoints[i] = PointOnArc(radius, angle);
            }
            _fill.positionCount = _fillPoints.Length;
            _fill.SetPositions(_fillPoints);
        }

        private Vector3 PointOnArc(float radius, float angleDegrees)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians) * radius, -Mathf.Cos(radians) * radius, -_height);
        }

        private static LineRenderer CreateLine(string name, Transform owner, Material material, float width, bool loop)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(owner, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = loop;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.alignment = LineAlignment.TransformZ;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            lineObject.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            return line;
        }
    }
}
