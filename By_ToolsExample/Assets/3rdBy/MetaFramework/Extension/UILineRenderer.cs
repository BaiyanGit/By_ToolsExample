using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
namespace ExtraFoundation.Components
{
    //===============================================================================
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Effects/UILineRenderer")]
    public class UILineRenderer : MaskableGraphic
    {
        #region Public Properties
        //---------------------------------------------------------------------------
        public Texture MainTexture
        {
            get { return texture; }
            set { texture = value; }
        }
 
        //---------------------------------------------------------------------------
        public float LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }
 
        //---------------------------------------------------------------------------
        public PositionType Space
        {
            get { return posType; }
            set { posType = value; }
        }
 
        //---------------------------------------------------------------------------
        public bool LineList
        {
            get { return lineList; }
            set { lineList = value; }
        }
 
        //---------------------------------------------------------------------------
        public bool LineCaps
        {
            get { return lineCaps; }
            set { lineCaps = value; }
        }
 
        //---------------------------------------------------------------------------
        public JoinType LineJoin
        {
            get { return lineJoin; }
            set { lineJoin = value; }
        }
 
        //---------------------------------------------------------------------------
        public override Texture mainTexture
        {
            get
            {
                return texture == null ? s_WhiteTexture : texture;
            }
        }
 
        //---------------------------------------------------------------------------
        /// <summary>
        /// Texture to be used.
        /// </summary>
        public Texture Texture
        {
            get
            {
                return texture;
            }
 
            set
            {
                if (texture == value)
                {
                    return;
                }
 
                texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }
 
        //---------------------------------------------------------------------------
        public Rect UVRect
        {
            get
            {
                return uvRect;
            }
 
            set
            {
                if (uvRect == value)
                {
                    return;
                }
 
                uvRect = value;
                SetVerticesDirty();
            }
        }
 
        #endregion
 
        #region Public Methods
        //---------------------------------------------------------------------------
        public void AddVector2Point(Vector2 pos)
        {
            Point point = new Point();
            point.Position = pos;
            point.IsTarget = false;
 
            points.Add(point);
        }
 
        //---------------------------------------------------------------------------
        public void AddTransformPoint(Transform trans)
        {
            Point point = new Point();
            point.Target = trans;
            point.IsTarget = true;
 
            points.Add(point);
        }
 
        public void InsertVector2PointAt(int index, Vector2 pos)
        {
            Point point = new Point();
            point.Position = pos;
            point.IsTarget = false;
 
            points.Insert(index, point);
        }
 
        //---------------------------------------------------------------------------
        public void RemovePointAt(int index)
        {
            if(index >= 0 && points.Count > index)
            {
                points.RemoveAt(index);
            }
        }
 
        //---------------------------------------------------------------------------
        public void ClearPoints()
        {
            points.Clear();
        }
        #endregion
 
        #region UnityMethods
        //---------------------------------------------------------------------------
        private void Update()
        {
            // if (Application.isEditor && transform.position != cachePos)
            // {
            //     cachePos = transform.position;
            //     OnRebuildRequested();
            // }
#if UNITY_EDITOR
            if (transform.position != cachePos)
            {
                cachePos = transform.position;
                OnRebuildRequested();
            }
#endif
        }
        #endregion
 
        #region Internal Methods
        //---------------------------------------------------------------------------
        private List<Vector2> GetTruePoins()
        {
            List<Vector2> truePoins = new List<Vector2>();
 
            if (posType == PositionType.Absolute)
            {
                Vector2 pOffset = transform.position;
                for (int i = 0, count = points.Count; i < count; i++)
                {
                    Point point = points[i];
 
                    if (point.IsTarget)
                    {
                        if (point.Target)
                        {
                            truePoins.Add((Vector2)point.Target.position - pOffset);
                        }
                    }
                    else
                    {
                        truePoins.Add(point.Position - pOffset);
                    }
 
                }
            }
            else
            {
                for (int i = 0, count = points.Count; i < count; i++)
                {
                    Point point = points[i];
 
                    if (point.IsTarget)
                    {
                        if (point.Target)
                        {
                            truePoins.Add(point.Target.position - transform.position);
                        }
                    }
                    else
                    {
                        truePoins.Add(point.Position);
                    }
 
                }
            }
 
            return truePoins;
        }
 
        //---------------------------------------------------------------------------
        private List<UIVertex[]> LineListSegments(List<Vector2> points)
        {
            var segments = new List<UIVertex[]>();
            for (int i = 1, count = points.Count; i < count; i += 2)
            {
                var start = points[i - 1];
                var end = points[i];
 
                if (lineCaps)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.Start));
                }
 
                segments.Add(CreateLineSegment(start, end, SegmentType.Middle));
 
                if (lineCaps)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.End));
                }
            }
 
            return segments;
        }
 
        //---------------------------------------------------------------------------
        private List<UIVertex[]> StraightLineSegments(List<Vector2> points)
        {
            var segments = new List<UIVertex[]>();
            for (int i = 1, count = points.Count; i < count; i++)
            {
                var start = points[i - 1];
                var end = points[i];
 
                if (lineCaps && i == 1)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.Start));
                }
 
                segments.Add(CreateLineSegment(start, end, SegmentType.Middle));
 
                if (lineCaps && i == count - 1)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.End));
                }
            }
 
            return segments;
        }
 
        //---------------------------------------------------------------------------
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (points == null)
            {
                return;
            }
 
            vh.Clear();
 
            List<Vector2> truePoins = GetTruePoins();
            List<UIVertex[]> segments = null;
 
            if (lineList)
            {
                segments = LineListSegments(truePoins);
            }
            else
            {
                segments = StraightLineSegments(truePoins);
            }
 
            for (int i = 0, count = segments.Count; i < count; i++)
            {
                if (!lineList && i < segments.Count - 1)
                {
                    Vector3 vec1 = segments[i][1].position - segments[i][2].position;
                    Vector3 vec2 = segments[i + 1][2].position
                        - segments[i + 1][1].position;
 
                    float angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;
 
                    // 转动方向
                    Vector3 cross = Vector3.Cross(vec1.normalized, vec2.normalized);
                    float sign = Mathf.Sign(cross.z);
 
                    // 计算斜点
                    float miterDistance = lineWidth * 0.5f / Mathf.Tan(angle * 0.5f);
 
                    Vector3 offset = vec1.normalized * miterDistance * sign;
                    Vector3 miterPointA = segments[i][2].position - offset;
                    Vector3 miterPointB = segments[i][3].position + offset;
 
                    var joinType = lineJoin;
                    if (joinType == JoinType.Miter)
                    {
                        if (miterDistance < vec1.magnitude * 0.5f
                            && miterDistance < vec2.magnitude * 0.5f
                            && angle > MinMiterJoin)
                        {
                            segments[i][2].position = miterPointA;
                            segments[i][3].position = miterPointB;
                            segments[i + 1][0].position = miterPointB;
                            segments[i + 1][1].position = miterPointA;
                        }
                        else
                        {
                            joinType = JoinType.Bevel;
                        }
                    }
 
                    if (joinType == JoinType.Bevel)
                    {
                        if (miterDistance < vec1.magnitude * 0.5f
                            && miterDistance < vec2.magnitude * 0.5f
                            && angle > MinBevelNiceJoin)
                        {
                            if (sign < 0)
                            {
                                segments[i][2].position = miterPointA;
                                segments[i + 1][1].position = miterPointA;
                            }
                            else
                            {
                                segments[i][3].position = miterPointB;
                                segments[i + 1][0].position = miterPointB;
                            }
                        }
 
                        var join = new UIVertex[]
                        {
                            segments[i][2],
                            segments[i][3],
                            segments[i + 1][0],
                            segments[i + 1][1]
                        };
 
                        vh.AddUIVertexQuad(join);
                    }
                }
 
                vh.AddUIVertexQuad(segments[i]);
            }
        }
 
        //---------------------------------------------------------------------------
        /// <summary>
        /// 创建线段链接处过度
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
        {
            if (type == SegmentType.Start)
            {
                var capStart = start - ((end - start).normalized * lineWidth * 0.5f);
 
                return CreateLineSegment(capStart, start, SegmentType.Start);
            }
            else if (type == SegmentType.End)
            {
                var capEnd = end + ((end - start).normalized * lineWidth * 0.5f);
 
                return CreateLineSegment(end, capEnd, SegmentType.End);
            }
 
            Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be "
                + "SegmentType.Start or SegmentType.End");
 
            return null;
        }
 
        //---------------------------------------------------------------------------
        /// <summary>
        /// 构建矩形
        /// </summary>
        /// <param name="start">开始点</param>
        /// <param name="end">结束点</param>
        /// <param name="type">线段类型</param>
        /// <returns>矩形顶点数据</returns>
        private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end,
            SegmentType type)
        {
            var uvs = MiddleUvs;
            if (type == SegmentType.Start)
            {
                uvs = StartUvs;
            }
            else if (type == SegmentType.End)
            {
                uvs = EndUvs;
            }
 
            Vector2 nDir = new Vector2(start.y - end.y, end.x - start.x).normalized;
            Vector2 offset = nDir * lineWidth * 0.5f;
 
 
            var v1 = start - offset;
            var v2 = start + offset;
            var v3 = end + offset;
            var v4 = end - offset;
 
            return SetVbo(new[] { v1, v2, v3, v4 }, uvs);
        }
 
        //---------------------------------------------------------------------------
        /// <summary>
        /// 构建<see cref="UIVertex"/>矩形
        /// </summary>
        /// <param name="vertices">顶点</param>
        /// <param name="uvs">UV</param>
        /// <returns>矩形UI顶点数据</returns>
        protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
        {
            UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
 
            return vbo;
        }
        #endregion
        
        #region Internal Fields
        //---------------------------------------------------------------------------
        private const float MinMiterJoin = 15 * Mathf.Deg2Rad;
        private const float MinBevelNiceJoin = 0 * Mathf.Deg2Rad;
        [SerializeField]
        private Texture texture;
        [SerializeField]
        private Rect uvRect = new Rect(0f, 0f, 1f, 1f);
        [SerializeField]
        private float lineWidth = 2;
        //[SerializeField]
        //private bool useMargins;
        //[SerializeField]
        //private Vector2 margin;
        [SerializeField]
        private List<Point> points = new List<Point>();
        [SerializeField]
        private bool lineList = false;
        [SerializeField]
        private bool lineCaps = false;
        [SerializeField]
        private JoinType lineJoin = JoinType.Bevel;
        [SerializeField]
        private PositionType posType = PositionType.Relative;
        private Vector3 cachePos;
 
        #region UV
        //---------------------------------------------------------------------------
        private static readonly Vector2 UVTopLeft = Vector2.zero;
        private static readonly Vector2 UVBottomLeft = new Vector2(0, 1);
        private static readonly Vector2 UVTopCenter = new Vector2(0.5f, 0);
        private static readonly Vector2 UVBottomCenter = new Vector2(0.5f, 1);
        private static readonly Vector2 UVTopRight = new Vector2(1, 0);
        private static readonly Vector2 UVBottomRight = new Vector2(1, 1);
        private static readonly Vector2[] StartUvs = new[]
        {
            UVTopLeft,
            UVBottomLeft,
            UVBottomCenter,
            UVTopCenter
        };
        private static readonly Vector2[] MiddleUvs = new[]
        {
            UVTopCenter,
            UVBottomCenter,
            UVBottomCenter,
            UVTopCenter
        };
        private static readonly Vector2[] EndUvs = new[]
        {
            UVTopCenter,
            UVBottomCenter,
            UVBottomRight,
            UVTopRight
        };
        #endregion
 
        #endregion
 
        #region Public Declarations
        //===========================================================================
        public enum PositionType
        {
            Relative,
            Absolute
        }
 
        //===========================================================================
        /// <summary>
        /// 连接处处理
        /// </summary>
        public enum JoinType
        {
            Bevel,
            Miter
        }
 
        //===========================================================================
        [System.Serializable]
        private struct Point
        {
            #region Public Properties
            //-----------------------------------------------------------------------
            public Vector2 Position
            {
                get { return position; }
                set { position = value; }
            }
 
            //-----------------------------------------------------------------------
            public Transform Target
            {
                get { return target; }
                set { target = value; }
            }
 
            //-----------------------------------------------------------------------
            public bool IsTarget
            {
                get { return isTarget; }
                set { isTarget = value; }
            }
            #endregion
 
            #region Internal Fields
            //-----------------------------------------------------------------------
            [SerializeField]
            private Vector2 position;
 
            //-----------------------------------------------------------------------
            [SerializeField]
            private Transform target;
 
            //-----------------------------------------------------------------------
            [SerializeField]
            private bool isTarget;
            #endregion
 
        }
        #endregion
 
        #region Internal Declarations
        //===========================================================================
        private enum SegmentType
        {
            Start,
            Middle,
            End,
        }
        #endregion
    }
}