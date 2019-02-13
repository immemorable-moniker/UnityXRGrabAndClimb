using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerInput_Interactor : MonoBehaviour
{
    public PlayerInput.HandSource hand;
    public Rigidbody XRRigRigidbody;
    public PlayerClimb ClimbScript;

    private PlayerInput InputSource;

    private Transform Transform;
    public Rigidbody Rigidbody { get; private set; }
    public Vector3 XRRigTargetDelta
    {
        get
        {
            if (Climbing)
            {
                return -((Rigidbody.position - XRRigRigidbody.position) - ClimbReference);
            }
            else
                return Vector3.zero;
        }
    }

    private int HistoryCount = 3;

    private GameObject HeldObject = null;
    private Interactable HeldObjectInteractable = null;
    private GameObject TargetObject;
    private bool waitingOnFirstRelease = false;
    private Joint grabJoint;
    private bool Climbing = false;
    private Vector3 ClimbReference;

    private Vector3 InteractorVelocity;
    private Vector3 InteractorAngularVelocity;
    private Vector3 PrevPosition;
    private Quaternion PrevRotation;
    private RingBuffer<Vector3> PrevVelocity;
    private RingBuffer<Vector3> PrevAngularVelocity;

    // Start is called before the first frame update
    void Awake()
    {
        InputSource = gameObject.GetComponentInParent<PlayerInput>();
        Transform = transform;
        Rigidbody = GetComponent<Rigidbody>();

        PrevVelocity = new RingBuffer<Vector3>(HistoryCount);
        PrevAngularVelocity = new RingBuffer<Vector3>(HistoryCount);
    }

    private void OnEnable()
    {
        if (hand == PlayerInput.HandSource.Left)
            InputSource.TriggerInputEvent_Left += HandleGrab;
        else
            InputSource.TriggerInputEvent_Right += HandleGrab;
    }

    private void OnDisable()
    {
        if (hand == PlayerInput.HandSource.Left)
            InputSource.TriggerInputEvent_Left -= HandleGrab;
        else
            InputSource.TriggerInputEvent_Right -= HandleGrab;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Interactable>() != null)
            TargetObject = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (TargetObject == other.gameObject)
            TargetObject = null;
    }

    void HandleGrab(bool value)
    {
        // Grab on press down
        if (value && HeldObject == null && TargetObject != null)
        {
            HeldObject = TargetObject;
            HeldObjectInteractable = HeldObject.GetComponent<Interactable>();

            if (HeldObjectInteractable.type == Interactable.InteractableType.Grabable
                || HeldObjectInteractable.type == Interactable.InteractableType.JointManipulator)
            {
                HeldObjectInteractable.Grab(this);
                CreateJoint(HeldObject);
                waitingOnFirstRelease = HeldObjectInteractable.StickyGrab;
            }
            else if (ClimbScript != null
                && HeldObjectInteractable.type == Interactable.InteractableType.Climbable)
            {
                ClimbScript.AddInfluencer(this);
                Climbing = true;
                ResetReference();
            }
        }
        else if (!value && waitingOnFirstRelease)
        {
            waitingOnFirstRelease = false;
        }
        // Release on second release
        else if (!value)
        {
            ForceDrop();
        }
    }

    public void ForceDrop()
    {
        waitingOnFirstRelease = false;
        if (HeldObject != null)
        {
            var tempObject = HeldObject;

            if (HeldObjectInteractable.type == Interactable.InteractableType.Grabable)
            {
                if (grabJoint)
                    DestroyJoint();

                Vector3 RadiusVector = HeldObject.transform.position - Transform.position;
                HeldObject.GetComponent<Rigidbody>().AddForce(InteractorVelocity + Vector3.Cross(InteractorAngularVelocity, RadiusVector), ForceMode.VelocityChange);
                HeldObject.GetComponent<Rigidbody>().AddTorque(InteractorAngularVelocity / (1 + RadiusVector.magnitude), ForceMode.VelocityChange);
            }
            else if (HeldObjectInteractable.type == Interactable.InteractableType.Climbable)
            {
                ClimbScript.RemoveInfluencer(this);
                Climbing = false;

                Vector3 RadiusVector = HeldObject.transform.position - Transform.position;
                XRRigRigidbody.AddForce(-XRRigRigidbody.velocity, ForceMode.VelocityChange); // zero the velocity
                XRRigRigidbody.AddForce(-InteractorVelocity, ForceMode.VelocityChange); // fling
            }
            else if (HeldObjectInteractable.type == Interactable.InteractableType.JointManipulator)
            {
                if (grabJoint)
                    DestroyJoint();

                Vector3 RadiusVector = HeldObject.transform.position - Transform.position;
                HeldObject.GetComponent<Rigidbody>().AddForce(InteractorVelocity + Vector3.Cross(InteractorAngularVelocity, RadiusVector), ForceMode.VelocityChange);
            }

            HeldObject = null;
            HeldObjectInteractable = null;
            tempObject.GetComponent<Interactable>().Drop();
        }
    }

    void CreateJoint(GameObject obj)
    {
        grabJoint = gameObject.AddComponent<FixedJoint>();
        grabJoint.breakForce = 1500f;
        grabJoint.connectedBody = obj.GetComponent<Rigidbody>();
    }

    private void OnJointBreak(float breakForce)
    {
        ForceDrop();
    }

    void DestroyJoint()
    {
        grabJoint.connectedBody = null;
        Destroy(grabJoint);
    }

    private void FixedUpdate()
    {
        Vector3 FrameInteractorVelocity = (Rigidbody.position - PrevPosition) / Time.fixedDeltaTime;
        Quaternion VelocityDiff = (Rigidbody.rotation * Quaternion.Inverse(PrevRotation));
        Vector3 FrameInteractorAngularVelocity = (new Vector3(Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.x), Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.y), Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.z))
            / Time.fixedDeltaTime) * (2 * Mathf.PI / 360f);

        PrevPosition = Rigidbody.position;
        PrevRotation = Rigidbody.rotation;
        PrevVelocity.Add(FrameInteractorVelocity);
        PrevAngularVelocity.Add(FrameInteractorAngularVelocity);

        for (int i = 0; i < HistoryCount; i++)
        {
            InteractorVelocity += PrevVelocity[i];
            InteractorAngularVelocity += PrevAngularVelocity[i];
        }

        InteractorVelocity /= HistoryCount;
        InteractorAngularVelocity /= HistoryCount;
    }

    public void ResetReference()
    {
        ClimbReference = (Rigidbody.position - XRRigRigidbody.position);
    }
}
