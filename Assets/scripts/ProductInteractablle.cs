using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class ProductInteractablle : XRGrabInteractable
{
    Outline outline;
    SphereCollider sphereCollider;
    Vector3 InitialPosition;
    Quaternion InitialRotation;
    public bool MoveBack;
    public void Initialize()
    {
            
        sphereCollider = GetComponent<SphereCollider>();


        outline = gameObject.AddComponent<Outline>();
        outline.OutlineWidth = 6.0f;
        outline.OutlineColor = Color.cyan;
        outline.enabled = false;

        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        MoveBack = false;

        useDynamicAttach = true;
      }
    void Update()
    {
        if (MoveBack)
        {
            transform.position = Vector3.Lerp(transform.position, InitialPosition, 0.025f);
            transform.rotation = Quaternion.Lerp(transform.rotation, InitialRotation, 0.05f);
            if (Vector3.Distance(transform.position, InitialPosition) < 0.01f &&Quaternion.Angle(transform.rotation, InitialRotation) < 0.1f)
            {
                transform.position = InitialPosition;
                transform.rotation = InitialRotation;
                print("Reset");
                MoveBack = false;
            }
        }
    }

    private static MeshRenderer FindFirstMeshRenderer(GameObject root)
    {
        MeshRenderer meshRenderer = root.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            return meshRenderer;
        }

        foreach (Transform child in root.transform)
        {
            meshRenderer = FindFirstMeshRenderer(child.gameObject);
            if (meshRenderer != null)
            {
                return meshRenderer;
            }
        }

        return null;
    }

    public static SphereCollider AddSphereColliderToFirstMeshRenderer(GameObject root)
    {
        if (root == null)
        {
            return null;
        }

        MeshRenderer meshRenderer = FindFirstMeshRenderer(root);

        if (meshRenderer != null)
        {
            SphereCollider sphereCollider = meshRenderer.gameObject.AddComponent<SphereCollider>();
            return sphereCollider;
        }
        return null;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        outline.enabled = true;
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        outline.enabled = false;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        MoveBack = true;
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
    }

    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        base.OnDeactivated(args);
    }
}
