using UnityEngine;
using System.Collections.Generic;

namespace Watermelon.BubbleShooter
{
    public static class PathSmoothTool
    {
        //arrayToSmooth is original Vector3 array, smoothness is the number of interpolations. 
        public static Vector3[] Smooth(Vector3[] arrayToSmooth, float smoothness)
        {
            List<Vector3> points;
            List<Vector3> curvedPoints;
            int pointsLength = 0;
            int curvedLength = 0;

            if (smoothness < 1.0f)
                smoothness = 1.0f;

            pointsLength = arrayToSmooth.Length;

            curvedLength = pointsLength * Mathf.RoundToInt(smoothness) - 1;
            curvedPoints = new List<Vector3>(curvedLength);

            float t = 0.0f;
            for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
            {
                t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

                points = new List<Vector3>(arrayToSmooth);

                for (int j = pointsLength - 1; j > 0; j--)
                {
                    for (int i = 0; i < j; i++)
                    {
                        points[i] = (1 - t) * points[i] + t * points[i + 1];
                    }
                }

                curvedPoints.Add(points[0]);
            }

            return curvedPoints.ToArray();
        }
    }
}