using UnityEngine;
using UnityEngine.InputSystem;

public class FireHydrant : MonoBehaviour
{
    //[SerializeField] private GameObject HydrantPrefab;
    [SerializeField] private GameObject CurrentHydrant;
    [SerializeField] private GameObject BackHydrant;
    [SerializeField] InputActionReference pickupRef;
    public static bool isHydrantPickedUp { get; private set; }
    public static bool inHydrantRange { get; private set; }

    public bool pickupProxy;
    public bool rangeProxy;

    private void Start()
    {
        isHydrantPickedUp = false;
        inHydrantRange = false;
        pickupRef.action.performed += PickupHydrant;
        BackHydrant.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        pickupProxy = isHydrantPickedUp;
        rangeProxy = inHydrantRange;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("ReloadArea")) inHydrantRange = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("ReloadArea")) inHydrantRange= false;
    }

    private void PickupHydrant(InputAction.CallbackContext context)
    {
        if (inHydrantRange && !isHydrantPickedUp)
        {
            CurrentHydrant.gameObject.SetActive(false);
            BackHydrant.gameObject.SetActive(true);
            isHydrantPickedUp = true;
            inHydrantRange = false;
            //change interact subscription to attempt place
            pickupRef.action.performed -= PickupHydrant;
            pickupRef.action.performed += PlaceHydrant;
        }
    }

    private void PlaceHydrant(InputAction.CallbackContext context)
    {
        CurrentHydrant.gameObject.SetActive(true);
        CurrentHydrant.transform.position = new Vector3(BackHydrant.transform.position.x, 0, BackHydrant.transform.position.z);
        BackHydrant.gameObject.SetActive(false);
        
        isHydrantPickedUp = false;
        pickupRef.action.performed -= PlaceHydrant;
        pickupRef.action.performed += PickupHydrant;


    }
}
