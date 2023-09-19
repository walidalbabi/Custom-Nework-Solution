using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.Debuging
{
    public static class EVDebug
    {
        public static void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c, float duration)
        {
            // create matrix
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(pos, rot, scale);

            var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
            var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
            var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
            var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

            var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
            var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
            var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
            var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

            Debug.DrawLine(point1, point2, c, duration);
            Debug.DrawLine(point2, point3, c, duration);
            Debug.DrawLine(point3, point4, c, duration);
            Debug.DrawLine(point4, point1, c, duration);

            Debug.DrawLine(point5, point6, c, duration);
            Debug.DrawLine(point6, point7, c, duration);
            Debug.DrawLine(point7, point8, c, duration);
            Debug.DrawLine(point8, point5, c, duration);

            Debug.DrawLine(point1, point5, c, duration);
            Debug.DrawLine(point2, point6, c, duration);
            Debug.DrawLine(point3, point7, c, duration);
            Debug.DrawLine(point4, point8, c, duration);

            // optional axis display
            Debug.DrawRay(m.GetColumn(3), m.GetColumn(2), Color.magenta, duration); // Forward (Z-axis)
            Debug.DrawRay(m.GetColumn(3), m.GetColumn(1), Color.yellow, duration);  // Up (Y-axis)
            Debug.DrawRay(m.GetColumn(3), m.GetColumn(0), Color.red, duration);     // Right (X-axis)
        }

    }
}

