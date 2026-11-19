using UnityEngine;

namespace Battlekart
{
    public enum eItemType
    {
        None = 0,
        Boost = 1,
        Grapple = 2,
        Bomb = 3
    }

    public static class ItemInfo
    {
        public static Color GetColor(eItemType item)
        {
            switch (item)
            {
                case eItemType.Boost:
                    return new Color(0.2f, 0.85f, 1.0f);
                case eItemType.Grapple:
                    return new Color(1.0f, 0.6f, 0.1f);
                case eItemType.Bomb:
                    return new Color(1.0f, 0.15f, 0.15f);
                default:
                    return Color.white;
            }
        }

        public static string GetLabel(eItemType item)
        {
            switch (item)
            {
                case eItemType.Boost:
                    return "BOOST";
                case eItemType.Grapple:
                    return "GRAPPLE";
                case eItemType.Bomb:
                    return "BOMB";
                default:
                    return string.Empty;
            }
        }

        public static bool TryGetScreenPosition(Vector3 worldPosition, out Vector2 guiPosition)
        {
            guiPosition = Vector2.zero;
            Camera camera = Camera.main;
            if (camera == null)
            {
                return false;
            }

            Vector3 screen = camera.WorldToScreenPoint(worldPosition);
            if (screen.z <= 0.0f)
            {
                return false;
            }

            guiPosition = new Vector2(screen.x, Screen.height - screen.y);
            return true;
        }

        public static void DrawLabel(Vector2 center, string text, GUIStyle style, Color color)
        {
            Vector2 size = style.CalcSize(new GUIContent(text));
            Rect rect = new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
            float offset = Mathf.Max(1.0f, style.fontSize * 0.08f);

            style.normal.textColor = new Color(0.0f, 0.0f, 0.0f, 0.8f);
            GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), text, style);
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }
    }
}
