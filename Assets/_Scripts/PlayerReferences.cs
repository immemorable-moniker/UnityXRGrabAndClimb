using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerClimb))]
[RequireComponent(typeof(PlayerWalk))]
public class PlayerReferences : MonoBehaviour
{
    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;

    public Rigidbody rigRigidbody { get; private set; }
    public PlayerInput playerInput { get; private set; }
    public PlayerClimb playerClimb { get; private set; }
    public PlayerWalk playerWalk { get; private set; }

    private void Awake()
    {
        rigRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerClimb = GetComponent<PlayerClimb>();
        playerWalk = GetComponent<PlayerWalk>();
    }
}
