using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class VelocitiesFromTransform : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }
    public Vector3 AngularVelocity { get; private set; }

    private float HistoryTimePeriod = 0.125f;
    private Transform MyTransform;
    private Vector3 PrevPosition;
    private Quaternion PrevRotation;
    public RingBuffer<float> PrevTimeDelta { get; private set; }
    public RingBuffer<Vector3> PrevVelocity { get; private set; }
    public RingBuffer<Vector3> PrevAngularVelocity { get; private set; }

    private void Awake()
    {
        MyTransform = transform;

        int HistoryCount = (int)Mathf.Ceil(XRDevice.refreshRate * HistoryTimePeriod);

        PrevTimeDelta = new RingBuffer<float>(HistoryCount);
        PrevVelocity = new RingBuffer<Vector3>(HistoryCount);
        PrevAngularVelocity = new RingBuffer<Vector3>(HistoryCount);
    }

    private void Update()
    {
        Vector3 FrameInteractorVelocity = (MyTransform.position - PrevPosition) / Time.deltaTime;
        Quaternion VelocityDiff = (MyTransform.rotation * Quaternion.Inverse(PrevRotation));
        Vector3 FrameInteractorAngularVelocity = (new Vector3(Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.x), Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.y), Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.z))
            / Time.deltaTime) * (2 * Mathf.PI / 360f);

        PrevPosition = MyTransform.position;
        PrevRotation = MyTransform.rotation;
        PrevTimeDelta.Add(Time.deltaTime);
        PrevVelocity.Add(FrameInteractorVelocity);
        PrevAngularVelocity.Add(FrameInteractorAngularVelocity);

        Velocity = Vector3.zero;
        AngularVelocity = Vector3.zero;
        float TotalTimeDelta = 0f;
        int NumSamples = 0;
        for (int i = 0; (i < PrevVelocity.Count) && (TotalTimeDelta < HistoryTimePeriod); i++)
        {
            TotalTimeDelta += PrevTimeDelta[i];
            Velocity += PrevVelocity[i];
            AngularVelocity += PrevAngularVelocity[i];
            NumSamples++;
        }

        Velocity /= NumSamples;
        AngularVelocity /= NumSamples;
    }
}
