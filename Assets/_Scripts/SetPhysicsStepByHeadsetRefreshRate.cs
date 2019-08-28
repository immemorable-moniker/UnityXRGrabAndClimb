using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class SetPhysicsStepByHeadsetRefreshRate : MonoBehaviour
{
    void OnEnable()
    {
        XRDevice.deviceLoaded += OnDeviceLoaded;
    }

    void OnDisable()
    {
        XRDevice.deviceLoaded += OnDeviceLoaded;
    }

    void OnDeviceLoaded(string device)
    {
        Debug.Log("Loaded device " + device + ". Refresh rate detected as " + XRDevice.refreshRate);

        // set physics refresh rate to HMD framerate
        Time.fixedDeltaTime = 1 / XRDevice.refreshRate;

        //float fraction = 2.0f;
        //Time.fixedDeltaTime = fraction / XRDevice.refreshRate;

        //// set to a fraction of the refresh rate, but < 50 Hz == > 0.02f
        //while (Time.fixedDeltaTime < 0.02f)
        //{
        //    Debug.Log("fixedDeltaTime " + Time.fixedDeltaTime + " is too low.  Adjusting.");
        //    fraction += 1.0f;
        //    Time.fixedDeltaTime = fraction / XRDevice.refreshRate;
        //}

        Debug.Log("fixedDeltaTime has been set to " + Time.fixedDeltaTime);
    }
}
