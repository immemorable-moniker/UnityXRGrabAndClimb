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
        JointManipulator,
        JustEvents //Use this when you just care about using the OnGrab and OnDrop events in a separate script
    }

    public InteractableType type = InteractableType.Grabable;
    public bool StickyGrab = false;
    public bool RelativeAnchor = true;

    public delegate void GrabEventHandler(Interactable interactable, PlayerInput_Interactor interactor);
    public event GrabEventHandler OnGrab;
    public event GrabEventHandler OnDrop;

    private PlayerInput_Interactor GrabInteractor = null;
    private Joint grabJoint;

    private int InitialLayer;

    private void Start()
    {
        InitialLayer = gameObject.layer;
    }

    public void Grab(PlayerInput_Interactor grabScript)
    {
        ChangeLayerOfAllColliders(LayerMask.NameToLayer("DontHitPlayer"));
        if (GrabInteractor != null) {
            GrabInteractor.ForceDrop();
        }

        GrabInteractor = grabScript;

        OnGrab?.Invoke(this, grabScript);
    }

    public void Drop()
    {
        if (GrabInteractor != null)
        {
            OnDrop?.Invoke(this, GrabInteractor);

            var tempTransform = GrabInteractor;

            // Do this to prevent an endless loop of dropping
            GrabInteractor = null;
            tempTransform.GetComponent<PlayerInput_Interactor>().ForceDrop();
        }
        ChangeLayerOfAllColliders(InitialLayer);
    }

    void ChangeLayerOfAllColliders(int layer)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].gameObject.layer = layer;
    }
}
