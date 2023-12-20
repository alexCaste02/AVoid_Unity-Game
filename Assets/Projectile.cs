using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f; // Adjust this value based on your game's requirements

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the projectile hit a player or an invader
        PlayerController player = collision.GetComponent<PlayerController>();
        InvaderController invader = collision.GetComponent<InvaderController>();

        if (player != null)
        {
            // Deal damage to the player
            if (!player.getIsDashing())
            {
                player.Health -= damage;
                Destroy(gameObject); // Destroy the projectile on impact
            }
        }
        else if (invader != null)
        {
            // Deal damage to the invader
            invader.Health -= damage;
            Destroy(gameObject); // Destroy the projectile on impact
        }

        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }

        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }


    }
}
