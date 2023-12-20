using UnityEngine;

public class SwordAttack : MonoBehaviour
{

    public float damage = 3;
    public float pushForce = 5f;

    Vector2 playerLocation;
    Collider2D swordCollider;
    //bool isAttackExecuted = false; // Flag to check if the attack has been executed
    SpriteRenderer spriteRenderer;
    private bool hit;

    public AudioSource sword_miss_1;
    public AudioSource sword_miss_2;
    public AudioSource sword_miss_3;
    public AudioSource sword_miss_4;

    private void Start()
    {
        swordCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }


    public void AttackUp(Vector2 playerLocation)
    {
        hit = false;
        print("attack up");
        swordCollider.enabled = true;
        transform.position = new Vector2(playerLocation.x - 0.005f, playerLocation.y - 0.035f);
        //  isAttackExecuted = true;
        Invoke("CheckIfHit", 0.1f);
    }

    public void AttackDown(Vector2 playerLocation)
    {
        hit = false;
        print("attack down");
        swordCollider.enabled = true;
        transform.position = new Vector2(playerLocation.x + 0.025f, playerLocation.y - 0.185f);
        //  isAttackExecuted = true;
        Invoke("CheckIfHit", 0.1f);
    }

    public void AttackLeft(Vector2 playerLocation)
    {
        hit = false;
        print("attack left");
        swordCollider.enabled = true;
        transform.position = new Vector2(playerLocation.x - 0.09f, playerLocation.y - 0.145f);
        // isAttackExecuted = true;
        Invoke("CheckIfHit", 0.1f);
    }

    public void AttackRight(Vector2 playerLocation)
    {
        hit = false;
        print("attack right");
        swordCollider.enabled = true;
        transform.position = new Vector2(playerLocation.x + 0.09f, playerLocation.y - 0.145f);
        // isAttackExecuted = true;
        Invoke("CheckIfHit", 0.05f);
    }

    public void StopAttack()
    {
        swordCollider.enabled = false;
        // Reset the position only if an attack has been executed
        //isAttackExecuted = false; // Reset the flag
    }

    private void CheckIfHit()
    {
        if (!hit)
        {
            print("miss");
            MissSound();
        }
    }

    private void MissSound()
    {
        int r = Random.Range(0, 4);

        switch (r)
        {
            case 0: sword_miss_1.Play(); break;
            case 1: sword_miss_2.Play(); break;
            case 2: sword_miss_3.Play(); break;
            case 3: sword_miss_4.Play(); break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {



        if (collision.CompareTag("Enemy"))
        {
            Skeleton skelly = collision.gameObject.GetComponent<Skeleton>();

            if (skelly != null)
            {
                // Deal damage to the enemy
                skelly.Health -= damage;

                // Apply knockback force to the enemy
                Vector2 knockbackDirection = (skelly.transform.position - transform.position).normalized;
                skelly.ApplyKnockback(knockbackDirection, pushForce);
                hit = true;
            }

            Mage mage = collision.gameObject.GetComponent<Mage>();
            if (mage != null)
            {
                mage.Health -= damage;

                Vector2 knockbackDirection = (mage.transform.position - transform.position).normalized;
                mage.ApplyKnockback(knockbackDirection, pushForce);
                hit = true;

            }

        }

        if (collision.CompareTag("Player") || collision.CompareTag("Invader"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            InvaderController invader = collision.gameObject.GetComponent<InvaderController>();


            if (player != null)
            {
                player.Health -= damage * 3;

                Vector2 direction = (
                    new Vector2(transform.position.x, transform.position.y - 0.12f) -
                    new Vector2(player.transform.position.x, player.transform.position.y - 0.12f)
                    ).normalized;

                Vector2 forceDir = new Vector2(-direction.x, -direction.y);
                player.ApplyKnockback(forceDir, pushForce / 5);

            }

            if (invader != null)
            {
                invader.Health -= damage * 3;
                Vector2 direction = (
                    new Vector2(transform.position.x, transform.position.y - 0.12f) -
                    new Vector2(invader.transform.position.x, invader.transform.position.y - 0.12f)
                    ).normalized;

                Vector2 forceDir = new Vector2(-direction.x, -direction.y);
                invader.ApplyKnockback(forceDir, pushForce / 5);

            }
        }

        if (collision.CompareTag("Door"))
        {
            Door door = collision.gameObject.GetComponent<Door>();

            if (door != null)
            {
                // Deal damage to the enemy
                door.Health -= damage;

                hit = true;
            }
        }

        if (collision.CompareTag("Chest"))
        {
            Chest chest = collision.gameObject.GetComponent<Chest>();

            if (chest != null)
            {
                // Deal damage to the enemy
                chest.Health -= damage;

                hit = true;
            }

        }


    }
}
