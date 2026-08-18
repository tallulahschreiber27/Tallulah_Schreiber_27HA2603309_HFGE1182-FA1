using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;


public class ShootHandler : MonoBehaviour
{
    private PlayerInputActions InputAction;
    
    [Header("ACTIONS")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileForce;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private float maxChargeTime = 1f;
    private bool isCharging = false;
    private float chargeTime = 0f;
    
    [SerializeField] private GameObject punchHitBox;
    
    [SerializeField] private int currentAmmo;
    [SerializeField] private int maxAmmo;
    [SerializeField] private int totalAmmo;

    [Header("CAMERA")]
    [SerializeField] private Camera cam;
    [SerializeField] private float defaultCamFOV;
    [SerializeField] private float zoomCamFOV;
    private bool isZooming;
    
    
    private void Awake()
    {
        InputAction = new PlayerInputActions();
        cam = Camera.main;
    }

    private void Start()
    {
        punchHitBox.SetActive(false);
    }

    private void OnEnable()
    {
        InputAction.Player.Enable();
        InputAction.Player.Attack.performed += OnBeginShoot;
        InputAction.Player.Attack.canceled += OnShoot;
        InputAction.Player.Reload.performed += OnReload;
        InputAction.Player.Aim.performed += OnAim;
        InputAction.Player.Aim.canceled += OnAimCanceled;
    }

    private void OnDisable()
    {
        InputAction.Player.Attack.performed -= OnBeginShoot;
        InputAction.Player.Attack.canceled -= OnShoot;
        InputAction.Player.Reload.performed -= OnReload;
        InputAction.Player.Aim.performed -= OnAim;
        InputAction.Player.Aim.canceled -= OnAimCanceled;
        InputAction.Player.Disable();
    }

    private void Update()
    {
        ZoomIn();
        ChargeShot();
    }

    public void ChargeShot()
    {
        if (isCharging)
        {
            chargeTime = Mathf.Lerp(chargeTime, maxChargeTime, Time.deltaTime);
        }
        else
        {
            chargeTime = 0f;
        }
    }
    private void OnBeginShoot(InputAction.CallbackContext context)
    {
        isCharging = true;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (currentAmmo <= 0 && totalAmmo > 0)
        {
            OnReload(new InputAction.CallbackContext());
            isCharging = false;
            return;
        }
        else if (currentAmmo <= 0 && totalAmmo <= 0)
        {
            StartCoroutine(PunchObject());
            isCharging = false;
            return;
        }

        currentAmmo--;
        
        if (projectilePrefab != null && projectileSpawn != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.AddForce(projectile.gameObject.transform.forward * projectileForce * chargeTime, ForceMode.Impulse);
            isCharging = false;
        }
      
    }

    public IEnumerator PunchObject()
    {
        punchHitBox.SetActive(true);
        yield return new WaitForSecondsRealtime(.5f);
        punchHitBox.SetActive(false);
    }
    
    private void OnReload(InputAction.CallbackContext context)
    {
        if (totalAmmo < maxAmmo)
        {
            currentAmmo = totalAmmo;
            totalAmmo = 0;
        }
        else
        {
            totalAmmo -= maxAmmo;
            currentAmmo = maxAmmo;
        }
    }

    private void ZoomIn()
    {
        if (isZooming)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomCamFOV, .1f);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, defaultCamFOV, .1f);
        }
    }
    
    private void OnAim(InputAction.CallbackContext context)
    {
        isZooming = true;
    }
    
    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        isZooming = false;
    }
    
    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
    }
}
