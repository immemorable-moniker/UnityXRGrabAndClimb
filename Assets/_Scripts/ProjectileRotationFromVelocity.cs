using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileRotationFromVelocity : MonoBehaviour
{
    private Rigidbody MyRigidbody;
    [Tooltip("If you want the forward direction of the flight of an object to be different than the default forward of the gameobject, specify an Euler rotation correction here.")]
    public Vector3 ObjectForwardDirectionCorrection = new Vector3(0, 0, 0);

    private void Start()
    {
        MyRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (MyRigidbody.velocity.magnitude > 1f)
            MyRigidbody.MoveRotation(Quaternion.LookRotation(MyRigidbody.velocity) * Quaternion.Euler(ObjectForwardDirectionCorrection));
    }
}
