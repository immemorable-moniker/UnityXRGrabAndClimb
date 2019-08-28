using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerReferences))]
public class PlayerWalk : MonoBehaviour
{
    public float moveSpeed = 2f;

    Rigidbody m_Rigidbody;
    PlayerInput m_PlayerInput;
    PlayerClimb m_PlayerClimb;
    PlayerReferences m_PlayerReferences;

    public bool moving { get; private set; } = false;
    bool m_LeftIsDriving = false; // true if left hand is driving, false if right

    Vector2 m_MoveDirection = Vector2.zero;

    // Start is called before the first frame update
    void Awake()
    {
        m_PlayerReferences = GetComponent<PlayerReferences>();
        m_PlayerInput = m_PlayerReferences.playerInput;
        m_PlayerClimb = m_PlayerReferences.playerClimb;
        m_Rigidbody = m_PlayerReferences.rigRigidbody;
    }

    private void Update()
    {
        if (m_PlayerClimb.Climbing)
            moving = false;

        if (moving && (m_MoveDirection != Vector2.zero))
        {
            // zero out previous walk speed
            m_Rigidbody.AddForce(-(new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z)), ForceMode.VelocityChange);

            // set walk speed relative to controller pointing direction
            Vector3 NewWalkDirection = new Vector3(m_MoveDirection.x, 0, m_MoveDirection.y);
            if (m_LeftIsDriving)
                m_Rigidbody.AddForce(TransformMoveDirectionToHandSpaceAndMoveScale(NewWalkDirection, m_PlayerReferences.leftHand.transform.rotation), ForceMode.VelocityChange);
            else
                m_Rigidbody.AddForce(TransformMoveDirectionToHandSpaceAndMoveScale(NewWalkDirection, m_PlayerReferences.rightHand.transform.rotation), ForceMode.VelocityChange);
        }
    }

    Vector3 TransformMoveDirectionToHandSpaceAndMoveScale(Vector3 newWalkVector, Quaternion handRotation)
    {
        return moveSpeed * Vector3.ProjectOnPlane((handRotation * newWalkVector), Vector3.up).normalized;
    }

    private void OnEnable()
    {
        m_PlayerInput.Primary2DAxisTouchInputEvent_Left += HandleTouchLeft;
        m_PlayerInput.Primary2DAxisTouchInputEvent_Right += HandleTouchRight;

        m_PlayerInput.Primary2DAxisInputEvent_Left += HandleAxisLeft;
        m_PlayerInput.Primary2DAxisInputEvent_Right += HandleAxisRight;
    }

    private void OnDisable()
    {
        m_MoveDirection = Vector2.zero;

        m_PlayerInput.Primary2DAxisTouchInputEvent_Left -= HandleTouchLeft;
        m_PlayerInput.Primary2DAxisTouchInputEvent_Right -= HandleTouchRight;

        m_PlayerInput.Primary2DAxisInputEvent_Left -= HandleAxisLeft;
        m_PlayerInput.Primary2DAxisInputEvent_Right -= HandleAxisRight;
    }

    void HandleTouchLeft(bool value)
    {
        if (value)
        {
            moving = true;
            m_LeftIsDriving = true;
        }
        else if (!value && m_LeftIsDriving)
        {
            Stop();
        }
    }

    void HandleTouchRight(bool value)
    {
        if (value)
        {
            moving = true;
            m_LeftIsDriving = false;
        }
        else if (!value && !m_LeftIsDriving)
        {
            Stop();
        }
    }

    void HandleAxisLeft(Vector2 value)
    {
        if (!(moving && m_LeftIsDriving))
            return;

        m_MoveDirection = value;
    }

    void HandleAxisRight(Vector2 value)
    {
        if (!(moving && !m_LeftIsDriving))
            return;

        m_MoveDirection = value;
    }

    void Stop()
    {
        moving = false;
    }
}

