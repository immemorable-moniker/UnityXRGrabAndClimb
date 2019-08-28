using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerInput_Interactor : MonoBehaviour
{
    public PlayerInput.HandSource hand;
    public Rigidbody xrRigRigidbody;
    public PlayerClimb climbScript;
    public VelocitiesFromTransform velocities;

    private PlayerInput InputSource;

    private Transform MyTransform;
    public Rigidbody MyRigidbody { get; private set; }
    
    public Vector3 XRRigTargetDelta
    {
        get
        {
            if (Climbing)
            {
                return -((MyRigidbody.position - xrRigRigidbody.position) - ClimbReference);
            }
            else
                return Vector3.zero;
        }
    }

    public bool HoldingSomething { get { return (HeldObject != null); } }

    private GameObject HeldObject = null;
    private Interactable HeldObjectInteractable = null;
    private GameObject TargetObject;
    private bool waitingOnFirstRelease = false;
    private ConfigurableJoint grabJoint;
    private bool Climbing = false;
    private Vector3 ClimbReference;

    private Quaternion grabJointStartRotation;

    // Start is called before the first frame update
    void Awake()
    {
        InputSource = gameObject.GetComponentInParent<PlayerReferences>().playerInput;
        MyTransform = transform;
        MyRigidbody = GetComponent<Rigidbody>();
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

    public bool TryGrab(GameObject ObjectToGrab)
    {
        Interactable ObjectToGrabInteractable = ObjectToGrab.GetComponent<Interactable>();

        if (HeldObject != null || ObjectToGrab == null || ObjectToGrabInteractable == null || ObjectToGrabInteractable.type != Interactable.InteractableType.Grabable)
            return false;

        // yes, this is duplicated from HandleGrab. No good reason other than code cleanliness.  Necessary for public calls, and I'm not interested in reafactoring right now.
        HeldObject = ObjectToGrab;
        HeldObjectInteractable = HeldObject.GetComponent<Interactable>();

        GrabObject(ObjectToGrab);
        return true;
    }

    void GrabObject(GameObject ObjectToGrab)
    {
        Interactable ObjectToGrabInteractable = ObjectToGrab.GetComponent<Interactable>();
        ObjectToGrabInteractable.Grab(this);
        CreateJoint(ObjectToGrab);
        waitingOnFirstRelease = ObjectToGrabInteractable.StickyGrab;
    }

    // Entry into grabbing. value indicates state of button - true for starting a grab or false for ending a grab
    public void HandleGrab(bool value)
    {
        // Grab on press down
        if (value && HeldObject == null && TargetObject != null)
        {
            HeldObject = TargetObject;
            HeldObjectInteractable = HeldObject.GetComponent<Interactable>();

            if (HeldObjectInteractable.type == Interactable.InteractableType.JustEvents)
                HeldObjectInteractable.Grab(this);
            else if (HeldObjectInteractable.type == Interactable.InteractableType.Grabable
                || HeldObjectInteractable.type == Interactable.InteractableType.JointManipulator)
            {
                GrabObject(HeldObject);
            }
            else if (climbScript != null 
                && HeldObjectInteractable.type == Interactable.InteractableType.Climbable)
            {
                climbScript.AddInfluencer(this);
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

                Vector3 RadiusVector = HeldObject.transform.position - MyTransform.position;
                HeldObject.GetComponent<Rigidbody>().AddForce(velocities.Velocity + Vector3.Cross(velocities.AngularVelocity, RadiusVector), ForceMode.VelocityChange);
                HeldObject.GetComponent<Rigidbody>().AddTorque(velocities.AngularVelocity / (1 + RadiusVector.magnitude), ForceMode.VelocityChange);
            }
            else if (HeldObjectInteractable.type == Interactable.InteractableType.Climbable)
            {
                climbScript.RemoveInfluencer(this);
                Climbing = false;

                xrRigRigidbody.AddForce(-xrRigRigidbody.velocity, ForceMode.VelocityChange); // zero the velocity
                xrRigRigidbody.AddForce(-3f * velocities.Velocity, ForceMode.VelocityChange); // fling
            }
            else if (HeldObjectInteractable.type == Interactable.InteractableType.JointManipulator)
            {
                if (grabJoint)
                    DestroyJoint();

                Vector3 RadiusVector = HeldObject.transform.position - MyTransform.position;
                HeldObject.GetComponent<Rigidbody>().AddForce(velocities.Velocity + Vector3.Cross(velocities.AngularVelocity, RadiusVector), ForceMode.VelocityChange);
            }

            HeldObject = null;
            HeldObjectInteractable = null;
            tempObject.GetComponent<Interactable>().Drop();
        }
    }

    void CreateJoint(GameObject obj)
    {
        grabJoint = gameObject.AddComponent<ConfigurableJoint>();

        // from fixedjoint
        grabJoint.breakForce = 1500f;
        grabJoint.connectedBody = obj.GetComponent<Rigidbody>();

        // configurablejoint additions - make it behave like a fixed joint
        grabJoint.xMotion = ConfigurableJointMotion.Locked;
        grabJoint.yMotion = ConfigurableJointMotion.Locked;
        grabJoint.zMotion = ConfigurableJointMotion.Locked;
        grabJoint.angularXMotion = ConfigurableJointMotion.Locked;
        grabJoint.angularYMotion = ConfigurableJointMotion.Locked;
        grabJoint.angularZMotion = ConfigurableJointMotion.Locked;

        if (!HeldObjectInteractable.RelativeAnchor)
        {
            grabJoint.autoConfigureConnectedAnchor = false;
            grabJoint.connectedAnchor = Vector3.zero;
            grabJoint.anchor = Vector3.zero;

            grabJoint.angularXMotion = ConfigurableJointMotion.Free;
            grabJoint.angularYMotion = ConfigurableJointMotion.Free;
            grabJoint.angularZMotion = ConfigurableJointMotion.Free;

            JointDrive drive = new JointDrive();
            drive.positionSpring = 100f;
            drive.positionDamper = 15f;
            drive.maximumForce = Mathf.Infinity;
            grabJoint.slerpDrive = drive;
            grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
            grabJoint.configuredInWorldSpace = false;
            grabJointStartRotation = MyRigidbody.rotation;
            grabJoint.SetTargetRotationLocal(grabJoint.connectedBody.rotation, grabJointStartRotation);
        }
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

    public void ResetReference()
    {
        ClimbReference = (MyRigidbody.position - xrRigRigidbody.position);
    }

    public void SetGrabJointTargetRotation(Quaternion targetRotation)
    {
        //grabJoint.SetTargetRotationLocal(grabJoint.connectedBody.rotation, MyRigidbody.rotation);

        //grabJoint.configuredInWorldSpace = false;
        grabJoint.SetTargetRotationLocal(targetRotation, grabJointStartRotation);
    }
}
