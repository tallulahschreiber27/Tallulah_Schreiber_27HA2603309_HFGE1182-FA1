using UnityEngine;

public class HealthPickup : Pickup
{
    protected override void ApplyEffect(GameObject player)
    {
        // DEBUG LOG 9: Confirm we reached the ultimate health calculation stage
        Debug.Log("[DEBUG 9] Executing HealthPickup ApplyEffect on: " + player.name);

        HealthHandler health = player.GetComponent<HealthHandler>();

        if (health != null)
        {
            // DEBUG LOG 10: Component found, executing the final heal!
            Debug.Log("[DEBUG 10] HealthHandler script found! Sending Heal value: " + value);
            health.HealHandler(value);
            DestroyPickup();
        }
        else
        {
            Debug.LogError("[DEBUG ERROR] The Player object has the 'Player' tag, but is completely missing the HealthHandler.cs script component!");
        }
    }
}
