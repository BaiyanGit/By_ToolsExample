using UnityEngine;

public class RecodeMove : MonoBehaviour
{
    public Transform[] points;
    private int _index;
    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, points[_index].position, 2 * Time.deltaTime);
        if (!(Vector3.Distance(transform.position, points[_index].position) < 0.1f)) return;
        _index++;
        if (_index != points.Length) return;
        _index = 0;
    }
}