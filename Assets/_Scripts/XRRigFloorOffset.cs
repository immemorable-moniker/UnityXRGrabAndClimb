using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class XRRigFloorOffset : MonoBehaviour
{
    public float OffsetHeight = 1.69f;

    // Start is called before the first frame update
    void Start()
    {
        TrySetRoomScale();
    }

    public void TrySetRoomScale()
    {
        if (XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale))
        {
            transform.localPosition = Vector3.zero;
            Debug.Log("SUCCESS: XRRigFloorOffset.TrySetRoomScale() on Application.platform= " + Application.platform + " and XRSettings.loadedDeviceName=" + XRSettings.loadedDeviceName);
        }
        else
        {
            transform.localPosition = new Vector3(0, OffsetHeight, 0);
            Debug.Log("FAILURE: XRRigFloorOffset.TrySetRoomScale() on Application.platform= " + Application.platform + " and XRSettings.loadedDeviceName=" + XRSettings.loadedDeviceName);
        }
    }
}
