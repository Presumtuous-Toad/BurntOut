using System.IO.MemoryMappedFiles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Camera mainCamera;
    [SerializeField] private ParticleSystem hose;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference reloadAction;
    [SerializeField] private int maxAmmo;
    [field: SerializeField]public int ammo { get; private set; }
    private bool reloading;
    private bool firing;
    [SerializeField] private float reloadSpeed; //how long it takes to reload
    [SerializeField] private ReloadBar ReloadBar;
    public Slider ammoSlider;

    private void Start()
    {
        mainCamera = Camera.main;
        attackAction.action.started += OnHoseStarted;
        attackAction.action.canceled += OnHoseStopped;
        reloadAction.action.performed += OnReaoad;

        firing = false;
        ammo = maxAmmo;
        reloading = false;

        ammoSlider.maxValue = maxAmmo;
        ammoSlider.value = ammo;
        
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);
            Vector3 lookDirection = targetPoint - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-lookDirection);
            }
        }
        if(firing)
        {
            ammo--;
            if (ammo < 0)
            {
                ammo = 0;
                hose.Stop();
                firing = false;
            }
        }
        ammoSlider.value = ammo;
    }

    private void OnHoseStarted(InputAction.CallbackContext context)
    {
        if (!hose.isPlaying && ammo > 0 && !reloading)
        {
            firing = true;
            hose.Play();
        }
    }

    private void OnHoseStopped(InputAction.CallbackContext context)
    {
        if(hose.isPlaying) hose.Stop();
        firing = false;
    }

    private void OnReaoad(InputAction.CallbackContext context)
    {
        if (!reloading)
        {            
            reloading = true;
            ReloadBar.reload();
            Invoke(nameof(ReloadComplete), reloadSpeed);
        }
        
    }

    private void ReloadComplete()
    {
        ammo = maxAmmo;
        reloading = false;
    }

    private void OnDestroy()
    {
        attackAction.action.started -= OnHoseStarted;
        attackAction.action.canceled -= OnHoseStopped;
        reloadAction.action.performed -= OnReaoad;
    }
}
