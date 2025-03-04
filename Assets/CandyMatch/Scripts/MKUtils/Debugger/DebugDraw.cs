using UnityEngine;

/*
  12.03.2021
  22.03.2021 - draw helix
 */
namespace Mkey
{
	public class DebugDraw 
	{
        public static void DrawCircle(Transform t, Vector2 center, float radius, Color color)
        {
            int count = 20;
            float da = 2 * Mathf.PI / count;
            Vector2[] pos = new Vector2[count + 1];
            for (int i = 0; i < count; i++)
            {
                float ida = i * da;
                pos[i] = t.TransformPoint(center + new Vector2(Mathf.Cos(ida) * radius, Mathf.Sin(ida) * radius));
            }
            pos[count] = pos[0];
            for (int i = 0; i < count; i++)
            {
                Debug.DrawLine(pos[i], pos[i + 1], color);
            }
        }

        public static void DrawCircle(Transform t, Vector2 center, float radius, int prec, Color color)
        {
            int count = prec;
            float da = 2 * Mathf.PI / count;
            Vector2[] pos = new Vector2[count + 1];
            for (int i = 0; i < count; i++)
            {
                float ida = i * da;
                pos[i] = t.TransformPoint(center + new Vector2(Mathf.Cos(ida) * radius, Mathf.Sin(ida) * radius));
            }
            pos[count] = pos[0];
            for (int i = 0; i < count; i++)
            {
                Debug.DrawLine(pos[i], pos[i + 1], color);
            }
        }

        public static void DrawCircle(Vector2 center, float radius, Color color)
        {
            int count = 20;
            float da = 2 * Mathf.PI / count;
            Vector2[] pos = new Vector2[count + 1];
            for (int i = 0; i < count; i++)
            {
                float ida = i * da;
                pos[i] = center + new Vector2(Mathf.Cos(ida) * radius, Mathf.Sin(ida) * radius);
            }
            pos[count] = pos[0];
            for (int i = 0; i < count; i++)
            {
                Debug.DrawLine(pos[i], pos[i + 1], color);
            }
        }

        public static void DrawHelix(Vector2 center, float angle, float k, int points,  Color color)
        {
            Vector3[] pos = ProcCurve.HelixPoints(center, angle, k, points);
            
            for (int i = 0; i <pos.Length; i++)
            {
                Debug.DrawLine(pos[i], pos[i + 1], color);
            }
        }
    }
}
