using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("PROJECTILE SETTINGS")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 5f;

    [Header("ROTATION ADJUSTMENT")]
    [Tooltip("Adjust this if your model flies sideways. Try 90, -90, or 0.")]
    [SerializeField] private float tiltAngleX = 90f;

    private Rigidbody rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasHit && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(rb.linearVelocity);
            Quaternion flatFlightRotation = lookRotation * Quaternion.Euler(tiltAngleX, 0f, 0f);
            transform.rotation = flatFlightRotation;
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (hasHit || col.gameObject == null) return;

        hasHit = true;

        ContactPoint contact = col.contacts[0];
        Vector3 surfaceNormal = contact.normal;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // POSITION POSITION AND FLAT ALIGNMENT FIXED HERE:
        transform.position = contact.point + (surfaceNormal * 0.02f);

        // FIX: Match the arrow's flat side plane (Z/X axes) to the surface normal instead of standing up
        Quaternion impactLook = Quaternion.LookRotation(-surfaceNormal, Vector3.up);
        transform.rotation = impactLook * Quaternion.Euler(90f, 0f, 0f);

        transform.SetParent(col.transform);

        switch (col.gameObject.tag)
        {
            case "Player":
                ShootHandler shooter = col.gameObject.GetComponent<ShootHandler>();
                if (shooter != null) shooter.AddAmmo(1);
                Destroy(gameObject);
                break;

            case "Enemy":
            case "NPC":
            case "Crate":
                HealthHandler health = col.gameObject.GetComponent<HealthHandler>();
                if (health != null) health.DamageHandler("Arrow", damage);
                break;
        }
    }
}
