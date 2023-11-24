using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    [SerializeField] private Transform trackingTransform;

    private Vector3 offset = new Vector3(0f, 50f, -25f);

    private void Update()
    {
        transform.position = trackingTransform.position + offset;
    }
}
