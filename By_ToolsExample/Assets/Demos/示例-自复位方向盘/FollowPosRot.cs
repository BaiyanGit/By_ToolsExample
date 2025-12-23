using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosRot : MonoBehaviour
{
    public static FollowPosRot Instance;

    public Transform parent;

    public Transform target;

    public Transform targetFollow;

  
    public float _accelerationX;
    public float _accelerationY;
    public float _accelerationZ;

    private Vector3 offset;

    public float offsetY;

    private void Awake()
    {
        Instance = this;


    }

  

    private void LateUpdate()
    {
        target.SetParent(parent);

        offset = targetFollow.localPosition - target.localPosition;

        _accelerationX = offset.x;

        _accelerationY = offset.y;

        _accelerationZ = offset.z;


        offsetY = targetFollow.localEulerAngles.y - target.localEulerAngles.y;

        target.SetParent(null);
    }

    

    private void OnDestroy()
    {
        Instance = null;
    }

}
