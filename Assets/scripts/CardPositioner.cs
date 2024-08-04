using TMPro;
using UnityEngine;

public class CardPositioner : MonoBehaviour
{
    private Camera mainCamera;
    public Vector3 localOffset;
    private Transform parentTransform;
    public Vector3 CurrectPosition;

    public TMP_Text Title;
    public TMP_Text Description;
    public TMP_Text Price;
    public TMP_Text Manufacturer;

    private void Start()
    {
        mainCamera = Camera.main;
        parentTransform = transform.parent;
        //localOffset = transform.localPosition;
        localOffset = new Vector3(0,0.35f,0);
    }

    private void LateUpdate()
    {
        if (gameObject.activeSelf)
        {
        
            transform.position = transform.parent.position + localOffset;
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0; // Lock rotation to Y-axis only


            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToCamera);
            }

            transform.Rotate(new Vector3(0,180,0));

            CurrectPosition = transform.position;       

        }
    }
}