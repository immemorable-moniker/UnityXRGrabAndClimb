using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class GravityCollider : MonoBehaviour
{
    public Transform Transform { get; private set; }
    public Transform FloorOffset;
    private CapsuleCollider[] Capsules;
    
    public delegate void CollisionStatusHandler(Transform transform, bool colliding);
    public event CollisionStatusHandler CollisionStatusChanged;

    private void Awake()
    {
        Transform = transform;
        Capsules = GetComponents<CapsuleCollider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < Capsules.Length; i++)
        {
            Capsules[i].height = FloorOffset.localPosition.y + Transform.localPosition.y + Capsules[i].radius; // Adding capsule radius to the height allows the body collider to also function as a head collider
            Capsules[i].center = new Vector3(0, -((Capsules[i].height) / 2) + Capsules[i].radius, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CollisionStatusChanged?.Invoke(Transform, true);
    }

    private void OnTriggerStay(Collider other)
    {
        CollisionStatusChanged?.Invoke(Transform, true);
    }

    private void OnTriggerExit(Collider other)
    {
        CollisionStatusChanged?.Invoke(Transform, false);
    }
}
