using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour
{
    public enum InteractableType
    {
        Grabable,
        Climbable,
        JointManipulator
    }

    public InteractableType type = InteractableType.Grabable;
    public bool StickyGrab = false;

    private Transform GrabTransform = null;

    private Joint grabJoint;

    public void Grab(PlayerInput_Interactor grabScript)
    {
        if (GrabTransform != null)
        {
            GrabTransform.GetComponent<PlayerInput_Interactor>().ForceDrop();
        }

        GrabTransform = grabScript.transform;
    }

    public void Drop()
    {
        if (GrabTransform != null)
        {
            var tempTransform = GrabTransform;

            GrabTransform = null;
            tempTransform.GetComponent<PlayerInput_Interactor>().ForceDrop();
        }
    }
}
