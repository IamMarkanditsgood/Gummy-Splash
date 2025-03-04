using System;
using System.Collections.Generic;
using UnityEngine;

/*
   11.03.2021
 */
namespace Mkey
{
    public enum PointPosition { LEFT, RIGHT, BEYOND, BEHIND, BETWEEN, ORIGIN, DESTINATION }; // СЛЕВА, СПРАВА, ВПЕРЕДИ, ПОЗАДИ, МЕЖДУ, НАЧАЛО, КОНЕЦ
    public enum IntersectionPointType { NONE, TIP, PFIP, NFIP }

    public class LineSegment
    {
        #region properties
        public Vector2 P1 { get; private set; }
        public Vector2 P2 { get; private set; }
        public Vector2 P1P2 { get; private set; }
        public float Magnitude => (magnitude >= 0) ? magnitude : magnitude = (P1P2).magnitude; 
        public float SqrMagnitude => (sqrMagnitude >= 0) ? sqrMagnitude : sqrMagnitude = (P1P2).sqrMagnitude;
        public Vector2 Normalized
        {
            get
            {
                if (isNormalized)
                    return normalized;
                else
                {
                    isNormalized = true;
                    return normalized = P1P2.normalized;
                }
            }
        }

        // equation of the line Ax + By + C = 0 (http://www.math.by/geometry/eqline.html)
        public float A { get { return P1.y - P2.y; } }
        public float B { get { return P2.x - P1.x; } }
        public float C { get { return P1.x * P2.y - P2.x * P1.y; } }

        public Dictionary<float, Vector2> SplitPoints { get; private set; }
        public bool IsSplitted =>  SplitPoints.Count > 0;
        public int SplitPointsCount =>  SplitPoints.Count;
        #endregion properties

        #region temp vars
        private bool isNormalized = false;
        private Vector2 normalized;
        private float magnitude = -1;
        private float sqrMagnitude = -1;
        #endregion temp vars

        #region ctor
        public LineSegment(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
            P1P2 = P2 - P1;
            SplitPoints = new Dictionary<float, Vector2>();
        }

        public LineSegment(Vector2 p1, Vector2 dir, float length)
        {
            P1 = p1;
            P2 = p1 + dir.normalized * length;
            magnitude = length;
            P1P2 = P2 - P1;
            SplitPoints = new Dictionary<float, Vector2>();
        }

        public LineSegment(LineSegment ls, bool calcMagnitude)
        {
            P1 = ls.P1;
            P2 = ls.P2;
            P1P2 = P2 - P1;
            if (calcMagnitude) magnitude = P1P2.magnitude;
            SplitPoints = new Dictionary<float, Vector2>();
        }
        #endregion ctor

        #region intersection
        /// <summary>
        /// True if exist true intersection point (TIP).
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public bool IsIntersected(LineSegment ls)  //кормен - каждый из отрезков пересекает прямую с други отрезком или конечная точка одного лежит на другом 
        {
            Func<Vector2, Vector2, float> vectProd = (v1, v2) =>
            {
                return v1.x * v2.y - v2.x * v1.y;
            };

            Vector2 p3 = ls.P1;
            Vector2 p4 = ls.P2;

            Vector2 p1p3 = p3 - P1;
            Vector2 p1p2 = P2 - P1;
            Vector2 p1p4 = p4 - P1;
            Vector2 p3p4 = p4 - p3;

            float d1 = vectProd((p1p3), (p1p2)) * vectProd((p1p4), (p1p2));
            float d2 = vectProd((-p1p3), (p3p4)) * vectProd((P2 - p3), (p3p4));

            return ((d1 <= 0) && (d2 <= 0));
        }

        /// <summary>
        /// Return intersection point between 2 segments TIP and FIP
        /// </summary>
        /// <param name="lS"></param>
        /// <returns></returns>
        public Vector2 GetIntersectionPointWith(LineSegment lS, ref bool intersected, ref bool overlapped)
        {
            float A1 = A;
            float B1 = B;
            float C1 = C;
            float A2 = lS.A;
            float B2 = lS.B;
            float C2 = lS.C;

            float k = A1 * B2 - A2 * B1;
            Vector2 iP = new Vector2();
            intersected = false;
            if (k != 0)
            {
                iP = new Vector2((-C1 * B2 + C2 * B1) / k, (-A1 * C2 + A2 * C1) / k); //http://e-maxx.ru/algo/lines_intersection
                intersected = true;
            }
            else
            {
                overlapped = ((A1 * B2 - C1 * C2 == 0) && (B1 * C2 - C1 * B2 == 0));
            }
            return iP;
        }

        /// <summary>
        /// Return TIP (true intersection point), PFIP (positive false intersection point) or NFIP (negative false intersection point)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public IntersectionPointType ClassifyIntersection(Vector2 point)
        {
            float P2P1m = SqrMagnitude;
            float P2Pm = (P2 - point).sqrMagnitude;
            float P1Pm = (P1 - point).sqrMagnitude;
            if (P2P1m >= P1Pm && P2P1m >= P2Pm)
            {
                return IntersectionPointType.TIP;
            }
            else if (P2Pm > P2P1m && P1Pm < P2Pm)
            {
                return IntersectionPointType.NFIP;
            }
            else if (P1Pm > P2P1m && P2Pm < P1Pm)
            {
                return IntersectionPointType.PFIP;
            }
            return IntersectionPointType.NONE;
        }

        /// <summary>
        /// Return intersection points with circle (true and false)
        /// </summary>
        /// <param name="lS"></param>
        /// <returns></returns>
        public void GetIntersectionPointWithCircle(Vector2 center, float radius, ref bool intersected, ref Vector2 point1, ref Vector2 point2)
        {
            float r2 = radius * radius;
            LineSegment lD = PerpendicularFromPoint(center);
            //  Debug.Log("OD length : " + lD.Magnitude);
            intersected = (lD.SqrMagnitude <= r2);
            if (!intersected) return;

            float hordLength = Mathf.Sqrt(r2 - lD.SqrMagnitude);
            //   Debug.Log("hord length : " + hordLength);

            Vector2 hord = Normalized * hordLength;

            point1 = center + lD.P1P2 + hord;
            point2 = center + lD.P1P2 - hord;
        }

        /// <summary>
        /// http://rsdn.org/forum/alg/1257071.all
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public bool IsIntersectCircleTIP(Vector2 center, float radius)
        {
            float x01 = P1.x - center.x;
            float y01 = P1.y - center.y;
            float x02 = P2.x - center.x;
            float y02 = P2.y - center.y;

            float dx = x02 - x01;
            float dy = y02 - y01;

            float a = dx * dx + dy * dy;
            float b = 2.0f * (x01 * dx + y01 * dy);
            float c = x01 * x01 + y01 * y01 - radius * radius;

            if (-b < 0) return (c < 0);
            if (-b < (2.0f * a)) return (4.0f * a * c - b * b < 0);
            return (a + b + c < 0);
        }
        #endregion intersection

        #region split
        /// <summary>
        /// Add point to SplitPoints if point is TIP
        /// </summary>
        /// <param name="p"></param>
        private void AddSplitPoint(Vector2 p)
        {
            float P1Pm = (p - P1).sqrMagnitude;
            float P2Pm = (p - P2).sqrMagnitude;

            if (P1Pm <= SqrMagnitude && P2Pm <= SqrMagnitude)
            {
                if (!SplitPoints.ContainsKey(P1Pm))
                {
                    SplitPoints.Add(P1Pm, p);
                }
            }
        }

        public void SplitByIntersection(LineSegment lS)
        {
            bool intersected = false;
            bool overlapped = false;
            Vector2 iP = GetIntersectionPointWith(lS, ref intersected, ref overlapped);
            if (intersected)
            {
                AddSplitPoint(iP);
                Debug.Log("Segments intersection point: " + iP);
            }
        }

        public void SplitByCircleIntersection(Vector2 center, float radius)
        {
            Vector2 point1 = Vector2.zero;
            Vector2 point2 = Vector2.zero;
            bool intersected = false;
            GetIntersectionPointWithCircle(center, radius, ref intersected, ref point1, ref point2);
            Debug.Log("Intersected ------------------------------------------------------------" + point1 + " : " + point2);
            if (intersected)
            {
                AddSplitPoint(point1);
                AddSplitPoint(point2);
            }
            Debug.Log("SplitPoints.Count" + SplitPoints.Count);
        }

        public List<LineSegment> GetSplitSegments()
        {
            List<LineSegment> result = new List<LineSegment>();

            if (SplitPoints.Count == 0)
            {
                result.Add(this);
                return result;
            }
            List<float> keys = new List<float>(SplitPoints.Count);
            foreach (var item in SplitPoints)
            {
                keys.Add(item.Key);
            }

            keys.Sort();
            result.Add(new LineSegment(P1, SplitPoints[keys[0]]));
            //  keys.ForEach((k) => { Debug.Log(k); });

            for (int i = 0; i < SplitPoints.Count - 1; i++)
            {
                result.Add(new LineSegment(SplitPoints[keys[i]], SplitPoints[keys[i + 1]]));
            }
            result.Add(new LineSegment(SplitPoints[keys[SplitPoints.Count - 1]], P2));
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public List<LineSegment> GetOutsideSplitSegments(Vector2 center, float radius)
        {
            List<LineSegment> result = new List<LineSegment>();

            //Func<Vector2, float, Vector2, bool> PointOutside = (c, r, pos) =>
            //{
            //    float sDist = (c - pos).sqrMagnitude;
            //    return sDist >= r * r;
            //};

            //Func<Vector2, float, LineSegment, bool> SegmentPointsOutside = (c, r, ls) =>
            //{
            //   // bool sp1Outside = PointOutside(c, r, ls.P1);
            //   // bool sp2Outside = PointOutside(c, r, ls.P2);
            //    bool spMiddleOutside = PointOutside(c, r, (ls.P2 + ls.P1) / 2f);

            //    return  spMiddleOutside;
            //};

            if (SplitPoints.Count == 0)
            {
                //   result.Add(this); // ??? why
                return result;
            }

            List<float> keys = new List<float>(SplitPoints.Count);

            foreach (var item in SplitPoints)
            {
                keys.Add(item.Key);
            }

            keys.Sort();

            LineSegment lS = new LineSegment(P1, SplitPoints[keys[0]]);

            if (lS.IsSegmentPointsOutside(center, radius))
                result.Add(lS);

            for (int i = 0; i < SplitPoints.Count - 1; i++)
            {
                lS = new LineSegment(SplitPoints[keys[i]], SplitPoints[keys[i + 1]]);
                if (lS.IsSegmentPointsOutside(center, radius))
                    result.Add(lS);
            }
            lS = new LineSegment(SplitPoints[keys[SplitPoints.Count - 1]], P2);
            if (lS.IsSegmentPointsOutside(center, radius))
                result.Add(lS);
            return result;
        }
        #endregion split

        #region distance
        /// <summary>
        /// distance from point to line
        /// https://ru.wikipedia.org/wiki/%D0%A0%D0%B0%D1%81%D1%81%D1%82%D0%BE%D1%8F%D0%BD%D0%B8%D0%B5_%D0%BE%D1%82_%D1%82%D0%BE%D1%87%D0%BA%D0%B8_%D0%B4%D0%BE_%D0%BF%D1%80%D1%8F%D0%BC%D0%BE%D0%B9_%D0%BD%D0%B0_%D0%BF%D0%BB%D0%BE%D1%81%D0%BA%D0%BE%D1%81%D1%82%D0%B8
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float DistanceFromPoint(Vector2 point)
        {
            float l = A * point.x + B * point.y + C;
            return (Mathf.Abs(l) / Mathf.Sqrt(A * A + B * B));
        }

        /// <summary>
        /// Sqr distance from point to line
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float SqrDistanceFromPoint(Vector2 point)
        {
            float l = A * point.x + B * point.y + C;
            return l * l / (A * A + B * B);
        }
        #endregion distance

        #region additional
        /// <summary>
        /// Return relative point position to line
        /// https://dxdy.ru/topic81630.html
        /// 
        /// http://informatics.mccme.ru/course/view.php?id=22
        /// Ласло М. Вычислительная геометрия и компьютерная графика на C++
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public PointPosition Classify(Vector2 p)
        {
            /*
                1) https://acmp.ru/article.asp?id_text=172
                при общей начальной точке двух векторов их векторное произведение 
                больше нуля, если второй вектор направлен влево от первого,       
                и меньше нуля, если вправо. 
                2) либо через скалярное произведение повернутого на -90 градусов вектора на точку, 
                хотя и не очень очевидно
             */
            Vector2 a = P2 - P1; // 1
            Vector2 b = p - P1; // 2
            double sa = a.x * b.y - b.x * a.y; // 3 b' = d rotate -90 = (b.y, -bx); dot prduct a*b' = a.x*b.y - b.x*a.y;

            double tsa = (sa < 0) ? -sa : sa; // ??? not enough precision
            sa = (tsa < 0.001f) ? 0 : sa;

            if (sa > 0.0)
                return PointPosition.LEFT;
            if (sa < 0.0)
                return PointPosition.RIGHT;
            if ((a.x * b.x < 0.0) || (a.y * b.y < 0.0))
                return PointPosition.BEHIND;
            if (a.sqrMagnitude < b.sqrMagnitude)
                return PointPosition.BEYOND;
            if (P1 == p)
                return PointPosition.ORIGIN;
            if (P2 == p)
                return PointPosition.DESTINATION;
            return PointPosition.BETWEEN;
        }

        /// <summary>
        /// Return new segment with offset. If dist>0 positive offset.
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public LineSegment OffsetSegment(float dist)
        {
            Vector2 dL = P2 - P1;
            Vector2 offsetDir = (dist > 0) ? new Vector2(dL.y, -dL.x).normalized * dist : new Vector2(-dL.y, dL.x).normalized * -(dist); // normal vector to dL
            return new LineSegment(P1 + offsetDir, P2 + offsetDir);
        }

        /// <summary>
        /// Return LineSegment Between point and intersection point (TIP or FIP) with  perpendicular to this linesegment
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public LineSegment PerpendicularFromPoint(Vector2 point)
        {
            Vector2 p1 = point;
            Vector2 perpDir;

            PointPosition pP = Classify(point);
            if (pP == PointPosition.LEFT)
            {
                perpDir = new Vector2(P1P2.y, -P1P2.x);
            }
            else
            {
                perpDir = new Vector2(-P1P2.y, P1P2.x);
            }
            return new LineSegment(point, perpDir, DistanceFromPoint(point));
        }

        public bool IsSegmentPointsOutside(Vector2 center, float radius)
        {
            Func<Vector2, float, Vector2, bool> PointOutside = (c, r, pos) =>
            {
                float sDist = (c - pos).sqrMagnitude;
                return sDist >= r * r;
            };

            bool sp1Outside = PointOutside(center, radius, P1);
            bool sp2Outside = PointOutside(center, radius, P2);
            bool spMiddleOutside = PointOutside(center, radius, (P2 + P1) / 2f);

            return spMiddleOutside && sp1Outside && sp2Outside;
        }
        #endregion additional

        #region closest point
        /// <summary>
        /// Return TIP closest point between point and this segment https://math.stackexchange.com/questions/846054/closest-points-on-two-line-segments
        /// </summary>
        /// <param name="lS"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        public Vector2 ClosestPoint(Vector2 point)
        {
            LineSegment lS = PerpendicularFromPoint(point);
            IntersectionPointType iPT = ClassifyIntersection(lS.P2);
            if (iPT == IntersectionPointType.TIP)
            {
                return lS.P2;
            }
            else
            {
                return ((point - P1).sqrMagnitude < (point - P2).sqrMagnitude) ? P1 : P2;
            }
        }

        /// <summary>
        /// Return closest point pair  between this segment and other segment https://math.stackexchange.com/questions/846054/closest-points-on-two-line-segments
        /// </summary>
        /// <param name="other"></param>
        /// <param name="otherPoint"></param>
        /// <param name="thisPoint"></param>
        public void ClosestPoint(LineSegment other, ref Vector2 otherPoint, ref Vector2 thisPoint)
        {
            // from other to this
            Vector2 P1o_T = ClosestPoint(other.P1); // closes point from other.P1 to this segment
            float P1o_T_sm = (other.P1 - P1o_T).sqrMagnitude;
            Vector2 P2o_T = ClosestPoint(other.P2); // closes point from other.P2 to this segment
            float P2o_T_sm = (other.P2 - P2o_T).sqrMagnitude;

            bool p1ismin = P1o_T_sm < P2o_T_sm;
            float min_dist1 = (p1ismin) ? P1o_T_sm : P2o_T_sm;
            Vector2 otherPoint_t1 = (p1ismin) ? other.P1 : other.P2; // choose closest other point
            Vector2 thisPoint_t1 = (p1ismin) ? P1o_T : P2o_T;        // choose cloest this point

            // from this to other
            Vector2 TP1_o = other.ClosestPoint(P1);
            float TP1_o_sm = (P1 - TP1_o).sqrMagnitude;
            Vector2 TP2_o = other.ClosestPoint(P2);
            float TP2_o_sm = (P2 - TP2_o).sqrMagnitude;

            p1ismin = TP1_o_sm < TP2_o_sm;
            float min_dist2 = (p1ismin) ? TP1_o_sm : TP2_o_sm;
            Vector2 otherPoint_t2 = (p1ismin) ? TP1_o : TP2_o;
            Vector2 thisPoint_t2 = (p1ismin) ? P1 : P2;

            otherPoint = (min_dist1 < min_dist2) ? otherPoint_t1 : otherPoint_t2;
            thisPoint = (min_dist1 < min_dist2) ? thisPoint_t1 : thisPoint_t2;
        }
        #endregion closest point

        #region display
        /// <summary>
        /// Display with transform from local scpace to world, with white color
        /// </summary>
        /// <param name="t"></param>
        /// <param name="color"></param>
        public void Display(Transform t)
        {
            Display(t, Color.white);
        }

        /// <summary>
        /// Display with transform from local scpace to world
        /// </summary>
        /// <param name="t"></param>
        /// <param name="color"></param>
        public void Display(Transform t, Color color)
        {
            Debug.DrawLine(t.TransformPoint(P1), t.TransformPoint(P2), color);
        }

        /// <summary>
        /// Display with white color
        /// </summary>
        /// <param name="t"></param>
        /// <param name="color"></param>
        public void Display()
        {
            Display(Color.white);
        }

        public void Display(Color color)
        {
            Debug.DrawLine(P1, P2, color);
        }

        public void DisplaySplitPoints(Transform t)
        {

            foreach (var item in SplitPoints)
            {
                Debug.Log("splitpoint: " + item.Value);
                DebugDraw.DrawCircle(t, item.Value, 4f, Color.gray);
            }
        }
        #endregion display

        #region override
        public override string ToString()
        {
            return (P1 + " : " + P2);
        }
        #endregion override
    }

    public class SegmentsIntersection
    {
        public Vector2 Position { get; private set; }
        public LineSegment LineSegment1 { get; private set; }
        public LineSegment LineSegment2 { get; private set; }
        public IntersectionPointType pp2LS1 { get; private set; }
        public IntersectionPointType pp2LS2 { get; private set; }
        private bool hasIntersection;
        public bool HasIntersection { get { return hasIntersection; } }
        private bool isOverlappedSegments;
        public bool IsOverlappedSegments { get { return isOverlappedSegments; } }

        // true intersection point
        public bool IsTip2LS1 { get { return (HasIntersection && (pp2LS1 == IntersectionPointType.TIP)); } }
        public bool IsTip2LS2 { get { return (HasIntersection && (pp2LS2 == IntersectionPointType.TIP)); } }

        // false intersection point
        public bool IsFip2LS1 { get { return (HasIntersection && !IsTip2LS1); } }
        public bool IsFip2LS2 { get { return (HasIntersection && !IsTip2LS2); } }

        // positive false intersection point
        public bool IsPFip2LS1 { get { return (HasIntersection && (pp2LS1 == IntersectionPointType.PFIP)); } }
        public bool IsPFip2LS2 { get { return (HasIntersection && (pp2LS2 == IntersectionPointType.PFIP)); } }

        // negative false intersection point
        public bool IsNFip2LS1 { get { return ((HasIntersection && pp2LS1 == IntersectionPointType.NFIP)); } }
        public bool IsNFip2LS2 { get { return ((HasIntersection && pp2LS2 == IntersectionPointType.NFIP)); } }

        public SegmentsIntersection(LineSegment lS1, LineSegment lS2)
        {
            LineSegment1 = lS1;
            LineSegment2 = lS2;
            Position = lS1.GetIntersectionPointWith(lS2, ref hasIntersection, ref isOverlappedSegments);
            pp2LS1 = IntersectionPointType.NONE;
            pp2LS2 = IntersectionPointType.NONE;
            if (IsOverlappedSegments)
            {
                Position = lS1.P2;
            }
            if (hasIntersection)
            {
                pp2LS1 = lS1.ClassifyIntersection(Position);
                pp2LS2 = lS2.ClassifyIntersection(Position);
            }


        }

        public void DrawIntersectionPoint(Transform t)
        {
            Color c1 = Color.black;
            Color c2 = Color.black;

            if (IsTip2LS1)
            {
                c1 = Color.green;
            }
            else if (IsPFip2LS1)
            {
                c1 = Color.blue;
            }
            else if (IsNFip2LS1)
            {
                c1 = Color.red;
            }


            if (IsTip2LS2)
            {
                c2 = Color.green;
            }
            else if (IsPFip2LS2)
            {
                c2 = Color.blue;
            }
            else if (IsNFip2LS2)
            {
                c2 = Color.red;
            }
            DebugDraw.DrawCircle(t, Position, 2, c1); // to ls1
            DebugDraw.DrawCircle(t, Position, 4, c2); // to ls2
        }
    }
}
