using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Skeleton : MonoBehaviour
{
    public float maxHealth;
    public float health;
    public float attack;
    public float pushForce;
    public float moveSpeed = 3f;
    public float maxSpeed = 5f;
    public bool isAlive = true;
    public float smoothTime = 0.1f;
    [SerializeField] public float distanceThreshold = 5f;

    private Vector2 velocitySmooth;
    private Transform playerTransform;
    private Transform invaderTransform;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 posicionJugador;
    private Vector2 posicionInvasor;
    public AudioSource skelly_death;
    public AudioSource hit_1;
    public AudioSource hit_2;
    public AudioSource hit_3;
    public AudioSource hit_4;
    public AudioSource hit_5;
    public AudioSource hit_6;

    [SerializeField] private StatBar healthBar;
    public Canvas healthBarCanvas;

    public float Health
    {
        set
        {
            health = value;
            UpdateHealthBar();
            if (health <= 0)
            {
                Defeated();
            }
            else
            {
                HitSound();
            }
        }
        get { return health; }
    }

    private void HitSound()
    {
        int r = Random.Range(0, 6);

        switch (r)
        {
            case 0: hit_1.Play(); break;
            case 1: hit_2.Play(); break;
            case 2: hit_3.Play(); break;
            case 3: hit_4.Play(); break;
            case 4: hit_5.Play(); break;
            case 5: hit_6.Play(); break;
        }
    }

    private void UpdateHealthBar()
    {
        healthBarCanvas.enabled = true;
        healthBar.UpdateStat(health, maxHealth);
    }

    private void Start()
    {
        InitializeComponents();
    }

    private void FixedUpdate()
    {
        CheckPlayerDistance(); // Check distance before deciding to move
        StabilizeRotation();
    }

    

    private void FlipSprite(float directionX)
    {
        spriteRenderer.flipX = directionX < 0;
    }

    private IEnumerator SmoothRotateTowards(Vector3 targetDirection, float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetDirection) * startRotation;

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            SmoothlyRotateEnemy(startRotation, targetRotation, duration, ref timeElapsed);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    private void StabilizeRotation()
    {
        Vector3 desiredUp = playerTransform.up;
        StartCoroutine(SmoothRotateTowards(desiredUp, smoothTime));
    }

    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce)
    {
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    private void Defeated()
    {
        skelly_death.Play();
        SetDefeatedAnimation();
        DisableEnemy();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision);
    }

    #region Helper Methods

    private void InitializeComponents()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        invaderTransform = GameObject.FindGameObjectWithTag("Invader")?.transform;
        health = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        healthBar = GetComponentInChildren<StatBar>();
        animator = GetComponent<Animator>();

        moveSpeed = Random.Range(moveSpeed - 1.25f, moveSpeed + 1.25f);
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform != null)
        {
            posicionJugador = new Vector2(playerTransform.position.x, playerTransform.position.y - 0.12f);

            if (invaderTransform != null)
            {
                posicionInvasor = new Vector2(invaderTransform.position.x, invaderTransform.position.y - 0.12f);
            } 
            else
            {
                posicionInvasor = posicionJugador;
            }
            

            float distPlayer = Vector2.Distance(posicionJugador, rb.position);
            float distInv = Vector2.Distance(posicionInvasor, rb.position);

            Vector2 closerDirection = distPlayer < distInv ? posicionJugador : posicionInvasor;
            Vector2 direction = closerDirection - rb.position;
            direction.Normalize();

            if (Mathf.Min(distInv,distPlayer) <= distanceThreshold) // Only move when within the threshold
            {
                CalculateEnemyMovement(direction);
            }
            else
            {
                rb.velocity = Vector2.SmoothDamp(rb.velocity, new Vector2(0, 0), ref velocitySmooth, smoothTime);
            }
        }
    }

    private void CalculateEnemyMovement(Vector2 direction)
    {    
        Vector2 targetVelocity = direction * moveSpeed;
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocitySmooth, smoothTime);
        FlipSprite(direction.x);
    }



    private void SmoothlyRotateEnemy(Quaternion startRotation, Quaternion targetRotation, float duration, ref float timeElapsed)
    {
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / duration);
        timeElapsed += Time.deltaTime;
    }

    private void SetDefeatedAnimation()
    {
        animator.SetBool("isDead", true);
        moveSpeed = 0;
        maxSpeed = 1;
        isAlive = false;
        healthBar.GetComponent<Slider>().gameObject.SetActive(false);
    }

    private void DisableEnemy()
    {
        enabled = false;
    }

    private void HandlePlayerCollision(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        InvaderController invader = collision.gameObject.GetComponent<InvaderController>();

        if (player != null && isAlive)
        {
            player.Health -= attack;
            Vector2 direction = (new Vector2(transform.position.x, transform.position.y) - posicionJugador).normalized;
            Vector2 forceDir = new Vector2(-direction.x, -direction.y);
            player.ApplyKnockback(forceDir, pushForce);
            ApplyKnockback(-forceDir, pushForce);
        }

        if (invader != null && isAlive)
        {
            invader.Health -= attack;
            Vector2 direction = (new Vector2(transform.position.x, transform.position.y) - posicionInvasor).normalized;
            Vector2 forceDir = new Vector2(-direction.x, -direction.y);
            invader.ApplyKnockback(forceDir, pushForce);
            ApplyKnockback(-forceDir, pushForce);
        }
    }

  

    #endregion
}
