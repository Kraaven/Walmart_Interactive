using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using Newtonsoft.Json;

public class ProductInteractable : XRGrabInteractable
{
    private Outline outline;
    private SphereCollider sphereCollider;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool moveBack;
    private bool viewingID;
    private bool isResetting;
    private GameObject card;
    private Vector3 originalScale;
    private Vector3 shrunkScale;

    public GameObject ID_Card;
    public float outlineWidth = 6.0f;
    public Color outlineColor = Color.cyan;
    public float shrinkFactor = 0.3f;
    public float transitionSpeed = 0.1f;
    public float resetDuration = 1f;

    public void Initialize()
    {
        SetupComponents();
        SetupInitialTransform();
        SetupCard();
        SetupScales();
    }

    private void SetupComponents()
    {
        sphereCollider = GetComponent<SphereCollider>();
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineWidth = outlineWidth;
        outline.OutlineColor = outlineColor;
        outline.enabled = false;
    }

    private void SetupInitialTransform()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        moveBack = false;
        useDynamicAttach = true;
    }

    private void SetupCard()
    {
        card = Instantiate(ModelImporter.UI_CardPrefab, transform);
        ModelImporter.FixUIScale(card);
        card.GetComponent<RectTransform>().Translate(0, 0.1f, 0);
        card.SetActive(false);

        var Positioner = card.GetComponent<CardPositioner>();
        print(JsonConvert.SerializeObject(ModelImporter.LocalData));
        ProductDisplay info = ModelImporter.LocalData.GetInformation(gameObject.name);
        Positioner.Title.text = info.Name;
        Positioner.Description.text = info.Description;
        Positioner.Price.text = $"${info.Price}<sub>[{info.Stock}]</sub>";
        Positioner.Manufacturer.text = info.Manufacturer;

    }

    private void SetupScales()
    {
        originalScale = transform.GetChild(0).localScale;
        shrunkScale = originalScale * shrinkFactor;
    }

    private void Update()
    {
        if (moveBack)
        {
            MoveBackToInitialPosition();
        }
    }

    private void MoveBackToInitialPosition()
    {
        if (viewingID && !isResetting)
        {
            StartCoroutine(ResetObjectWrapper());
        }

        transform.position = Vector3.Lerp(transform.position, initialPosition, transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, transitionSpeed * 2);

        if (Vector3.Distance(transform.position, initialPosition) < 0.01f &&
            Quaternion.Angle(transform.rotation, initialRotation) < 0.1f)
        {
            CompleteReset();
        }
    }

    private void CompleteReset()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        Debug.Log("Reset complete");
        moveBack = false;
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

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        moveBack = true;
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        viewingID = !viewingID;
        StartCoroutine(viewingID ? ShrinkObject() : EnlargeObject());
    }

    private IEnumerator ShrinkObject()
    {
        yield return ScaleObject(shrunkScale);
        card.SetActive(true);
    }

    private IEnumerator EnlargeObject()
    {
        card.SetActive(false);
        yield return ScaleObject(originalScale);
    }

    private IEnumerator ScaleObject(Vector3 targetScale)
    {
        Transform model = transform.GetChild(0);
        while (Vector3.Distance(model.localScale, targetScale) > 0.01f)
        {
            model.localScale = Vector3.Lerp(model.localScale, targetScale, transitionSpeed);
            yield return null;
        }
        model.localScale = targetScale;
    }

    private IEnumerator ResetObjectWrapper()
    {
        isResetting = true;
        card.SetActive(false);
        yield return ResetObject();
        isResetting = false;
    }

    private IEnumerator ResetObject()
    {
        Transform model = transform.GetChild(0);
        float elapsedTime = 0f;
        Vector3 startScale = model.localScale;

        while (elapsedTime < resetDuration)
        {
            model.localScale = Vector3.Lerp(startScale, originalScale, elapsedTime / resetDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        model.localScale = originalScale;
        card.SetActive(false);
        viewingID = false;
    }

    public static SphereCollider AddSphereColliderToFirstMeshRenderer(GameObject root)
    {
        if (root == null) return null;

        MeshRenderer meshRenderer = FindFirstMeshRenderer(root);
        return meshRenderer != null ? meshRenderer.gameObject.AddComponent<SphereCollider>() : null;
    }

    private static MeshRenderer FindFirstMeshRenderer(GameObject root)
    {
        MeshRenderer meshRenderer = root.GetComponent<MeshRenderer>();
        if (meshRenderer != null) return meshRenderer;

        foreach (Transform child in root.transform)
        {
            meshRenderer = FindFirstMeshRenderer(child.gameObject);
            if (meshRenderer != null) return meshRenderer;
        }

        return null;
    }
}