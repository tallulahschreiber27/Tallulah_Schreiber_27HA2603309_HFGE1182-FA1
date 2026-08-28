using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootHandler : MonoBehaviour
{
    private PlayerInputActions InputAction;

    [Header("ACTIONS")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileForce = 20f; // Added default fallback force
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private float maxChargeTime = 1f;
    private bool isCharging = false;
    private float chargeTime = 0f;

    [SerializeField] private GameObject punchHitBox;

    [SerializeField] private int currentAmmo = 10; // Added default values so it works out of the box
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private int totalAmmo = 30;

    [Header("CAMERA")]
    [SerializeField] private Camera cam;
    [SerializeField] private float defaultCamFOV = 60f; // FIX: Fallback standard 3D camera view
    [SerializeField] private float zoomCamFOV = 40f;    // FIX: Fallback standard zoom depth view
    private bool isZooming;

    private void Awake()
    {
        InputAction = new PlayerInputActions();

        // Safety check to dynamically grab your camera if one isn't explicitly dropped in
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Start()
    {
        if (punchHitBox != null)
        {
            punchHitBox.SetActive(false);
        }
    }

    private void OnEnable()
    {
        InputAction.Player.Enable();
        // Removed original listeners to rely on the manual Update mouse loops below
    }

    private void OnDisable()
    {
        InputAction.Player.Disable();
    }

    private void Update()
    {
        // --- MANUAL CONTROLS BIPASS ---

        // 1. Left Mouse Click down (Shoot charging)
        if (Input.GetMouseButtonDown(0))
        {
            OnBeginShootManual();
        }

        // 2. Left Mouse Click released (Fire projectile)
        if (Input.GetMouseButtonUp(0))
        {
            OnShootManual();
        }

        // 3. Right Mouse Hold down (Aim Zoom)
        if (Input.GetMouseButton(1))
        {
            isZooming = true;
        }
        else
        {
            isZooming = false; // Automatically unzooms when right click is released
        }

        // 4. Reload manual tap (R Key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnReloadManual();
        }

        ZoomIn();
        ChargeShot();
    }

    public void ChargeShot()
    {
        if (isCharging)
        {
            // Linear progression multiplier tracking
            chargeTime = Mathf.MoveTowards(chargeTime, maxChargeTime, Time.deltaTime);
        }
    }

    private void OnBeginShootManual()
    {
        isCharging = true;
        chargeTime = 0f; // Reset charge timer on click down
    }

    public void OnShootManual()
    {
        isCharging = false;

        // Reload check
        if (currentAmmo <= 0 && totalAmmo > 0)
        {
            OnReloadManual();
            return;
        }
        // Melee check
        else if (currentAmmo <= 0 && totalAmmo <= 0)
        {
            StartCoroutine(PunchObject());
            return;
        }

        currentAmmo--;

        if (projectilePrefab != null && projectileSpawn != null)
        {
            // Spawn the physical ball/bullet
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Calculate charge intensity. Minimum baseline force is 0.2f so uncharged taps still fly forward
                float combinedCharge = Mathf.Max(0.2f, chargeTime / maxChargeTime);

                // Fire projectile straight out of the spawn point's forward orientation
                rb.AddForce(projectileSpawn.forward * projectileForce * combinedCharge, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("[SHOOT ERROR] Cannot fire! Ensure Projectile Prefab and Projectile Spawn slots are filled in the Inspector.");
        }
    }

    public IEnumerator PunchObject()
    {
        if (punchHitBox != null)
        {
            punchHitBox.SetActive(true);
            yield return new WaitForSecondsRealtime(.5f);
            punchHitBox.SetActive(false);
        }
    }

    private void OnReloadManual()
    {
        Debug.Log("Reloading Weapon...");
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
        if (cam != null)
        {
            // Smoothly transitions your FOV values without breaking
            float targetFOV = isZooming ? zoomCamFOV : defaultCamFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 10f);
        }
    }

    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
    }
}
