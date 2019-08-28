using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerReferences))]
public class PlayerClimb : MonoBehaviour
{
    public BodyColliderBehaviour GravityColliderScript;

    private Rigidbody m_Rigidbody;

    private List<PlayerInput_Interactor> m_ClimbInfluencers;
    private Vector3 m_ClimbXRRigReference;
    private Vector3 m_CollisionCorrection;

    private Vector3 m_BodyCollisionPosition;
    private Vector3 m_LastKnownGoodPosition;
    private bool m_Colliding;
    private float m_CorrectionRecoveryRate = 0.01f;

    public bool Climbing
    {
        get
        {
            return (m_ClimbInfluencers.Count != 0);
        }
    }

    void Awake()
    {
        m_Rigidbody = GetComponent<PlayerReferences>().rigRigidbody;

        m_ClimbInfluencers = new List<PlayerInput_Interactor>(2);
        m_CollisionCorrection = Vector3.zero;
    }

    public void AddInfluencer(PlayerInput_Interactor NewInfluencer)
    {
        if (m_ClimbInfluencers.Contains(NewInfluencer))
            return;

        m_ClimbInfluencers.Add(NewInfluencer);
        m_Rigidbody.useGravity = false;
        GravityColliderScript.CollisionStatusChanged += HandleCollision;

        ResetReferences();
    }

    public void RemoveInfluencer(PlayerInput_Interactor NewInfluencer)
    {
        if (m_ClimbInfluencers.Contains(NewInfluencer))
        {
            GravityColliderScript.CollisionStatusChanged -= HandleCollision;
            m_ClimbInfluencers.Remove(NewInfluencer);
        }

        if (m_ClimbInfluencers.Count == 0)
        {
            m_Rigidbody.useGravity = true;
            m_CollisionCorrection = Vector3.zero;
            m_Colliding = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_Colliding)
        {
            m_LastKnownGoodPosition = m_Rigidbody.position;
        }

        if (m_ClimbInfluencers.Count != 0)
        {
            Vector3 TargetPositionDeltaAverage = Vector3.zero;
            for (int i = 0; i < m_ClimbInfluencers.Count; i++)
            {
                TargetPositionDeltaAverage += m_ClimbInfluencers[i].XRRigTargetDelta;
            }
            TargetPositionDeltaAverage /= m_ClimbInfluencers.Count;
            
            if (m_Colliding)
                m_CollisionCorrection += m_LastKnownGoodPosition - m_BodyCollisionPosition;
            else if (m_CollisionCorrection != Vector3.zero)
            {
                m_CollisionCorrection -= m_CollisionCorrection.normalized * Mathf.Min(m_CorrectionRecoveryRate, m_CollisionCorrection.magnitude);
            }

            m_Rigidbody.MovePosition(
                m_ClimbXRRigReference
                + m_CollisionCorrection
                + TargetPositionDeltaAverage
                );
        }
    }

    void HandleCollision(Transform transform, bool colliding)
    {
        m_Colliding = colliding;

        if (m_Colliding)
            m_BodyCollisionPosition = m_Rigidbody.position;
    }

    void ResetReferences()
    {
        m_ClimbXRRigReference = m_Rigidbody.position;

        for (int i = 0; i < m_ClimbInfluencers.Count; i++)
        {
            m_ClimbInfluencers[i].ResetReference();
        }
    }
}
