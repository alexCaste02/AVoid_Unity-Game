using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InvaderController : MonoBehaviour
{
    #region Attributes

    // Health Attributes
    public float maxHealth;
    public float health;
    [SerializeField] StatBar healthBar;

    // Stamina Attributes
    public float maxStamina;
    volatile public float stamina;
    [SerializeField] StatBar staminaBar;
    public float timeToRecoupStamina;
    public float staminaRecoverySpeed;

    // Movement Attributes
    public float moveSpeed;
    public float dashSpeed;
    public float dashDuration;
    public float attackMoveSpeed;
    public float collisionOffset;
    public float knockbackDuration;
    public ContactFilter2D movementFilter;

    // Sword Attack
    public SwordAttack SwordAttack;

    // Controls and Flags
    bool isControlsEnabled = true;
    bool hasMoved;
    bool isAttacking;
    bool isDashing;
    bool isAlive;
    volatile bool isExhausted;
    float timeSinceLostStamina = 0;

    // Direction Vectors
    Vector2 movementInput;
    Vector2 moveDirection;
    Vector2 lookDirection;

    // UI Elements
    GameManager gameManager;
    static Color DefaultColor = new Color(0.8f, 0.5f, 0.95f);

    public Canvas UI;
    public Canvas EndCard;
    public Canvas DeathCard;
    public GameObject Menu;

    // Audio Sources
    public AudioSource dash_audio;
    public AudioSource player_steps;

    // Afterimage Attributes
    public GameObject afterimagePrefab;
    public float afterimageDelay;
    public float afterimageDuration;
    private List<GameObject> afterimages = new List<GameObject>();
    private float afterimageTimer = 0f;

    #endregion

    #region Properties

    // Health Property
    public float Health
    {
        set
        {
            if (!isDashing && enabled)
            {
                health = value;

                Debug.Log("Player hp: " + health);
                healthBar.UpdateStat(health, maxHealth);
            }

            if (health <= 0 && isAlive)
            {
                if (isAlive)
                {
                    StartCoroutine(PlayerDeath());
                }

            }

            if (health > maxHealth)
            {
                health = maxHealth;

            }
        }
        get { return health; }
    }

    // Stamina Property
    public float Stamina
    {
        set
        {

            if ((stamina <= 0.1f) && !isExhausted)
            {

                Debug.Log("Rec: " + isExhausted);
                StartCoroutine(EnterRecoveryMode());

            }

            if (value < stamina)
            {
                timeSinceLostStamina = 0;
            }

            stamina = value;
            //Debug.Log("Player sp: " + stamina);
            staminaBar.UpdateStat(stamina, maxStamina);


        }
        get { return stamina; }
    }

    // Singleton Instance Property
    public static InvaderController Instance { get; private set; }

    #endregion

    #region Unity Methods

    Rigidbody2D rb;
    Animator animator;
    //private bool transitionFinished = false;

    void Start()
    {
        hasMoved = false;
        isAttacking = false;
        isDashing = false;
        isAlive = true;
        isExhausted = false;

        moveDirection = Vector2.up;
        lookDirection = Vector2.up;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();


        dash_audio = GameObject.Find("dash_audio").GetComponent<AudioSource>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("More than one instance of PlayerController found!");
        }

        StartCoroutine(CountTimeSinceLostStamina());
    }

    private void FixedUpdate()
    {

        if (!isDashing)
        {


            // Reset hasMoved at the beginning of each FixedUpdate
            hasMoved = false;

            if (movementInput != Vector2.zero && isControlsEnabled)
            {
                float currentMoveSpeed = GetMoveSpeed();

                Vector2 desiredMovement = movementInput.normalized * moveSpeed * Time.fixedDeltaTime;

                MoveWithCollision(desiredMovement);

                // If there has been movement, set hasMoved to true
                hasMoved = true;


                // Update moveDirection based on the movement direction
                moveDirection = desiredMovement.normalized;

                // Update look direction based on the last movement
                if (!isAttacking)
                {
                    lookDirection = moveDirection;
                }

            }
        }


        UpdateAnimator();

        if (hasMoved && !player_steps.isPlaying)
        {
            player_steps.UnPause();

        }

        if (!hasMoved)
        {
            player_steps.Pause();
        }

        if ((timeSinceLostStamina > timeToRecoupStamina) && !isExhausted && (stamina < maxStamina))
        {
            Stamina += staminaRecoverySpeed;
        }

    }

    private void Update()
    {
        if (transform.position.y < -5.7)
        {
            SceneManager.LoadScene("TUT_00");
        }

        if (transform.position.y > 17)
        {
            ShowEndCard();
        }


    }

    private void ShowEndCard()
    {
        healthBar.enabled = false;
        staminaBar.enabled = false;

        EndCard.enabled = true;


        GameManager.Instance.combatMusic.Stop();
        GameManager.Instance.endMusic.Play();

        foreach (var item in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(item);
        }

        print("b4 enabled");
        enabled = false;
        isControlsEnabled = false;
        player_steps.Stop();

        print("after enabled");



        //TMP_Text textMesh = GameObject.Find("Text1").GetComponent<TMP_Text>();
        //float letterAppearDelay = 0.1f;
        //StartCoroutine(ShowTextLetterByLetter(textMesh, letterAppearDelay));
        //StartCoroutine(ShowTextLetterByLetter(textMesh, letterAppearDelay));
        //StartCoroutine(ShowTextLetterByLetter(textMesh, letterAppearDelay));
    }

    private IEnumerator ShowTextLetterByLetter(TMP_Text textMesh, float letterAppearDelay)
    {
        int totalCharacters = textMesh.text.Length;
        int visibleCharacters = 0;

        while (visibleCharacters <= totalCharacters)
        {
            textMesh.maxVisibleCharacters = visibleCharacters; // Set the number of visible characters

            // Wait for the specified delay before showing the next letter
            yield return new WaitForSeconds(letterAppearDelay);

            visibleCharacters++;
        }
    }

    #endregion

    #region Player Actions

    void OnMove(InputValue movementValue)
    {
        if (isControlsEnabled)
        {
            movementInput = movementValue.Get<Vector2>();
        }

    }

    void OnFire()
    {
        if (!isDashing && isAlive && !isAttacking && isControlsEnabled
            && !isExhausted && (stamina > 0.1f))
        {
            Stamina -= 10;
            animator.SetTrigger("swordAttack");
        }
    }

    void OnDash()
    {

        if (!isAttacking && !isDashing && isAlive && isControlsEnabled
            && !isExhausted && (stamina > 0.1f))
        {

            Stamina -= 25;
            dash_audio.Play();
            StartCoroutine(Dash());
        }
    }

    #endregion

    #region Player Actions Logic

    private IEnumerator CountTimeSinceLostStamina()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            //print("time!" + timeSinceLostStamina);
            timeSinceLostStamina += 0.25f;
        }
    }

    private IEnumerator EnterRecoveryMode()
    {
        isExhausted = true;
        GameObject.Find("InvaderStaminaFill").GetComponent<Image>().color = Color.gray;

        while ((stamina < maxStamina) && (timeSinceLostStamina > timeToRecoupStamina))
        {
            Stamina += staminaRecoverySpeed / 2;

            yield return new WaitForFixedUpdate();
        }

        GameObject.Find("InvaderStaminaFill").GetComponent<Image>().color = new Color(0, 0.4f, 0);
        isExhausted = false;

    }

    private IEnumerator Dash()
    {
        isDashing = true;
        animator.SetBool("isDashing", true);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(0, 0, 0, 0.3f);

        // Set the dash speed
        float initialMoveSpeed = moveSpeed;
        moveSpeed = dashSpeed;

        float dashTimer = 0f;

        while (dashTimer < dashDuration)
        {

            // Calculate the movement direction based on your game's logic
            Vector2 desiredMovement = moveSpeed * Time.deltaTime * movementInput.normalized; // Replace Vector2.right with your intended dash direction

            // Move the character using the desiredMovement
            MoveWithCollision(desiredMovement);

            // Update hasMoved to true during the dash
            hasMoved = true;


            afterimageTimer += Time.fixedDeltaTime;

            if (afterimageTimer >= afterimageDelay)
            {

                CreateAfterimage();
                afterimageTimer = 0f;
            }


            dashTimer += Time.deltaTime;

            yield return new WaitForFixedUpdate(); // Use WaitForFixedUpdate to sync with physics updates

            if (dashTimer >= dashDuration)
            {
                // Reset the move speed and dash flag

                isDashing = false;
                moveSpeed = initialMoveSpeed;
                animator.SetBool("isDashing", false);
            }

        }

        sr.color = DefaultColor;

    }

    private void CreateAfterimage()
    {
        //Debug.Log("image created");
        // Instantiate a new afterimage at the player's current position
        GameObject afterimage = Instantiate(afterimagePrefab, transform.position, transform.rotation);
        SpriteRenderer afterimageRenderer = afterimage.GetComponent<SpriteRenderer>();
        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();

        // Set afterimage sprite and color to match the player
        afterimageRenderer.sprite = playerRenderer.sprite;
        afterimageRenderer.color = playerRenderer.color;

        // Add the afterimage to the list for later management
        afterimages.Add(afterimage);

        // Destroy the afterimage after a certain time (tweak as needed)
        StartCoroutine(DestroyAfterimage(afterimage));
    }

    private IEnumerator DestroyAfterimage(GameObject afterimage)
    {
        yield return new WaitForSeconds(0.2f); // Adjust as needed

        //Debug.Log("after destroy");
        // Remove the afterimage from the list and destroy it
        afterimages.Remove(afterimage);
        Destroy(afterimage);
    }

    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce)
    {
        // Normalize the direction vector to ensure consistent speed in all directions
        if (!isDashing)
        {
            knockbackDirection.Normalize();

            // Disable player movement during knockback
            StartCoroutine(DoKnockback(knockbackDirection, knockbackForce, knockbackDuration));
        }
    }

    private IEnumerator DoKnockback(Vector2 knockbackDirection, float knockbackForce, float knockbackDuration)
    {
        // Disable player controls
        isControlsEnabled = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;


        // Apply force to the Rigidbody2D based on the direction and force
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Wait for the specified knockback duration
        yield return new WaitForSeconds(knockbackDuration);

        // Reset the velocity to the original velocity after knockback duration
        rb.velocity = new Vector2(0, 0);

        sr.color = DefaultColor;
        isControlsEnabled = true;

    }

    private IEnumerator PlayerDeath()
    {
        Debug.Log("player died");
        animator.SetBool("isAlive", false);
        animator.Play("Player_Death");
        isAlive = false;
        enabled = false;

        UI.enabled = false;
        isControlsEnabled = false;

        DeathCard.enabled = true;
        StartCoroutine(SmoothColorTransition(
            DeathCard.GetComponent<Image>(),
            new Color(0, 0, 0, 0.85f),
            20f
            ));

        AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitUntil(() => animatorStateInfo.IsName("Player_Dead"));
        yield return new WaitForSeconds(2f);
        print("end");

    }

    private IEnumerator SmoothColorTransition(Image targetImage, Color endColor, float transitionDuration)
    {
        float elapsedTime = 0f;
        Color currentColor = targetImage.color;

        while (elapsedTime < transitionDuration)
        {
            targetImage.color = Color.Lerp(targetImage.color, endColor, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetImage.color = endColor; // Ensure the final color is set correctly

    }

    private void UpdateAnimator()
    {
        // Set animator parameters based on movement
        animator.SetBool("isMoving", hasMoved);

        // Set Animator parameters for smooth transitions
        animator.SetFloat("moveX", moveDirection.x);
        animator.SetFloat("moveY", moveDirection.y);

        animator.SetBool("isAttacking", isAttacking);
        animator.SetFloat("moveSpeed", GetMoveSpeed());

        // Flip the character if moving left
        float signX = Mathf.Sign(lookDirection.x);
        transform.localScale = new Vector3(signX, 1, 1);

        // Set the look direction parameter
        animator.SetFloat("lookX", lookDirection.x);
        animator.SetFloat("lookY", lookDirection.y);
    }

    void MoveWithCollision(Vector2 desiredMovement)
    {
        // Perform a cast to check for collisions
        RaycastHit2D[] hits = new RaycastHit2D[10]; // Increase the size if needed
        int count = rb.Cast(desiredMovement, movementFilter, hits, collisionOffset);



        if (count == 0)
        {
            // If no collisions, move the character
            rb.MovePosition(rb.position + desiredMovement);

            // Set moveDirection based on the movement direction
            moveDirection = desiredMovement.normalized;

            // Set isMoving to true since there has been movement
            hasMoved = true;
        }
        else
        {
            bool collisionFound = false;

            // Loop through the hits and check for collisions with specific layers to ignore
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Enemy")) // Change "Enemy" to your desired tag
                {
                    // Ignore collision with enemies during the dash
                    continue;
                }

                // Check collision with other objects
                Vector2 horizontalMovement = new Vector2(desiredMovement.x, 0f);
                Vector2 verticalMovement = new Vector2(0f, desiredMovement.y);

                count = rb.Cast(horizontalMovement, movementFilter, new List<RaycastHit2D>(), collisionOffset);
                if (count == 0)
                {
                    rb.MovePosition(rb.position + horizontalMovement);
                    moveDirection.x = horizontalMovement.normalized.x;
                    hasMoved = true;
                    collisionFound = true;
                }

                count = rb.Cast(verticalMovement, movementFilter, new List<RaycastHit2D>(), collisionOffset);
                if (count == 0)
                {
                    rb.MovePosition(rb.position + verticalMovement);
                    moveDirection.y = verticalMovement.normalized.y;
                    hasMoved = true;
                    collisionFound = true;
                }

                if (collisionFound)
                {
                    break;
                }
            }
        }
    }

    float GetMoveSpeed()
    {
        // Return the appropriate move speed based on conditions (e.g., if attacking or dashing)
        if (isAttacking)
        {
            return attackMoveSpeed;
        }
        else if (isDashing)
        {
            return dashSpeed;
        }
        else
        {
            return moveSpeed;
        }
    }

    public void startAttacking()
    {
        isAttacking = true;

        AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (animatorStateInfo.IsName("Player_Attack_B"))
        {
            SwordAttack.AttackDown(transform.position);
        }
        else if (animatorStateInfo.IsName("Player_Attack_T"))
        {
            SwordAttack.AttackUp(transform.position);
        }
        else
        {
            if (lookDirection.x < 0)
            {
                SwordAttack.AttackLeft(transform.position);
            }
            else
            {
                SwordAttack.AttackRight(transform.position);
            }
        }
    }

    public void stopAttacking()
    {
        SwordAttack.StopAttack();
        isAttacking = false;
    }

    #endregion

    public bool getIsAlive()
    {
        return isAlive;
    }

    public bool getIsDashing()
    {
        return isDashing;
    }

    public bool getIsControlsEnabled()
    {
        return isControlsEnabled;
    }


    void OnPause()
    {

        //if (Menu.active)
        //{
        //    Menu.SetActive(false);
        //    //GetComponent<PlayerInput>().enabled = true;
        //} else
        //{
        //    Menu.SetActive(true);
        //    //GetComponent<PlayerInput>().enabled = false;
        //}
    }




    //TODO: algun dia
    //private Color colorTransition(Color targetColor, float transitionTime, SpriteRenderer sr)
    //{
    //    if (transitionTime <= Time.deltaTime)
    //    {
    //        // transition complete
    //        // assign the target color
    //        GetComponent<SpriteRenderer>().color = targetColor;

    //        // start a new transition
    //        targetColor = ne
    //    }
    //    else
    //    {
    //        // transition in progress
    //        // calculate interpolated color


    //        // update the timer
    //        timeLeft -= Time.deltaTime;
    //    }

    //    return targetColor;
    //}



    // Start is called before the first frame update




    // FixedUpdate is used for physics-related updates

}
