using UnityEngine;

public class RollerTest : MonoBehaviour
{
    public RollerShutterController rollerShutter;

    [Header("卷起")] public KeyCode keyCodeRoll = KeyCode.Q;
    [Header("展开")] public KeyCode keyCodePlate = KeyCode.E;

    private void Update()
    {
        if (Input.GetKey(keyCodeRoll))
        {
            rollerShutter.RollUp();
        }

        if (Input.GetKey(keyCodePlate))
        {
            rollerShutter.Unroll();
        }
    }
}