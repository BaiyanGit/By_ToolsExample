using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIText : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Text>().text = transform.parent.name;
    }
}