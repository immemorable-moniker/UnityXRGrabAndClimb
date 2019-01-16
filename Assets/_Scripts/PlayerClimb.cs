using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerClimb : MonoBehaviour
{
    public GravityCollider GravityColliderScript;

    private Rigidbody Rigidbody;

    private List<PlayerInput_Interactor> ClimbInfluencers;
    private Vector3 ClimbXRRigReference;
    private Vector3 CollisionCorrection;

    private Vector3 BodyCollisionPosition;
    private Vector3 LastKnownGoodPosition;
    private bool Colliding;
    private float CorrectionRecoveryRate = 0.01f;

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();

        ClimbInfluencers = new List<PlayerInput_Interactor>(2);
        CollisionCorrection = Vector3.zero;
    }

    public void AddInfluencer(PlayerInput_Interactor NewInfluencer)
    {
        if (ClimbInfluencers.Contains(NewInfluencer))
            return;

        ClimbInfluencers.Add(NewInfluencer);
        Rigidbody.useGravity = false;
        GravityColliderScript.CollisionStatusChanged += HandleCollision;

        ResetReferences();
    }

    public void RemoveInfluencer(PlayerInput_Interactor NewInfluencer)
    {
        if (ClimbInfluencers.Contains(NewInfluencer))
        {
            GravityColliderScript.CollisionStatusChanged -= HandleCollision;
            ClimbInfluencers.Remove(NewInfluencer);
        }

        if (ClimbInfluencers.Count == 0)
        {
            Rigidbody.useGravity = true;
            CollisionCorrection = Vector3.zero;
            Colliding = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Colliding)
        {
            LastKnownGoodPosition = Rigidbody.position;
        }

        if (ClimbInfluencers.Count != 0)
        {
            Vector3 TargetPositionDeltaAverage = Vector3.zero;
            for (int i = 0; i < ClimbInfluencers.Count; i++)
            {
                TargetPositionDeltaAverage += ClimbInfluencers[i].XRRigTargetDelta;
            }
            TargetPositionDeltaAverage /= ClimbInfluencers.Count;
            
            if (Colliding)
                CollisionCorrection += LastKnownGoodPosition - BodyCollisionPosition;
            else if (CollisionCorrection != Vector3.zero)
            {
                CollisionCorrection -= CollisionCorrection.normalized * Mathf.Min(CorrectionRecoveryRate, CollisionCorrection.magnitude);
            }

            Debug.Log("CollisionCorrection " + CollisionCorrection);

            Rigidbody.MovePosition(
                ClimbXRRigReference 
                + CollisionCorrection 
                + TargetPositionDeltaAverage
                );
        }
    }

    void HandleCollision(Transform transform, bool colliding)
    {
        Debug.Log("HandleCollision.  colliding = " + colliding);

        Colliding = colliding;

        if (Colliding)
            BodyCollisionPosition = Rigidbody.position;
    }

    void ResetReferences()
    {
        ClimbXRRigReference = Rigidbody.position;

        for (int i = 0; i < ClimbInfluencers.Count; i++)
        {
            ClimbInfluencers[i].ResetReference();
        }
    }
}
