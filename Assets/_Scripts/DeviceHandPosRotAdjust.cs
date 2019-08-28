using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class DeviceHandPosRotAdjust : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("In DeviceHandPosRotAdjust, XRSettings.loadedDeviceName is " + XRSettings.loadedDeviceName + " and "
            + "model is " + XRDevice.model);

        if (XRSettings.loadedDeviceName.Contains("OpenVR") && XRDevice.model.Contains("Vive"))
        {
            transform.localPosition += new Vector3(0, 0, -0.1f);
            transform.rotation = Quaternion.Euler(60, 0, 0);
        }
        else if (XRSettings.loadedDeviceName.Contains("OpenVR") && XRDevice.model.Contains("Rift"))
        {
            transform.localPosition += new Vector3(0, 0, -0.1f);
            transform.rotation = Quaternion.Euler(35, 0, 0);
        }
        else if (XRSettings.loadedDeviceName.Contains("Oculus") && XRDevice.model.Contains("Rift"))
        {
            //transform.rotation = Quaternion.Euler(45, 0, 0);
            transform.localPosition += new Vector3(0, 0, -0.05f);
        }

        Debug.Log("RotateByDevice rotation set to " + transform.rotation);
    }
}
