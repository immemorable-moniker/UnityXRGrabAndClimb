using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class DeviceInteractorPosRotOffset : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("In DeviceInteractorPosRotOffset, XRSettings.loadedDeviceName is " + XRSettings.loadedDeviceName + " and "
            + "model is " + XRDevice.model);

        if (XRSettings.loadedDeviceName.Contains("OpenVR") && XRDevice.model.Contains("Vive"))
        {
            transform.localPosition += new Vector3(0, 0, -0.1f);
            transform.rotation = Quaternion.Euler(85, 0, 0);
        }
        else if (XRSettings.loadedDeviceName.Contains("OpenVR") && XRDevice.model.Contains("Rift"))
        {
            transform.localPosition += new Vector3(0, 0, -0.1f);
            transform.rotation = Quaternion.Euler(80, 0, 0);
        }
        else if (XRSettings.loadedDeviceName.Contains("Oculus") && XRDevice.model.Contains("Rift"))
        {
            transform.localPosition += new Vector3(0, 0, -0.05f);
            transform.rotation = Quaternion.Euler(45, 0, 0);
        }

        Debug.Log("RotateByDevice rotation set to " + transform.rotation);
    }
}
