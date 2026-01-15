using UnityEngine;

namespace _3rdBy.ByFunc.Extension
{
    public class RaycastTriggerHelper
    {
        public static bool IsInCollider(MeshCollider other, Vector3 point)
        {
            Vector3 from = (Vector3.up * 5000f);
            Vector3 dir  = (point - from).normalized;
            float   dist = Vector3.Distance(from, point);
//fwd 
            int hit_count = Cast_Till(from, point, other);
//back
            dir       =  (from - point).normalized;
            hit_count += Cast_Till(point, point + (dir * dist), other);

            if (hit_count % 2 == 1)
            {
                return (true);
            }

            return (false);
        }

        private static int Cast_Till(Vector3 from, Vector3 to, MeshCollider other)
        {
            int     counter = 0;
            Vector3 dir     = (to - from).normalized;
            float   dist    = Vector3.Distance(from, to);
            bool    isBreak = false;
            while (!isBreak)
            {
                isBreak = true;
                var hits = Physics.RaycastAll(from, dir, dist);
                for (int tt = 0; tt < hits.Length; tt++)
                {
                    if (hits[tt].collider == other)
                    {
                        counter++;
                        from    = hits[tt].point + dir.normalized * .001f;
                        dist    = Vector3.Distance(from, to);
                        isBreak = false;
                        break;
                    }
                }
            }

            return (counter);
        }
    }
}