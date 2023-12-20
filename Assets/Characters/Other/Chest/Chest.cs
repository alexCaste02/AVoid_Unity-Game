using UnityEngine;

public class Chest : MonoBehaviour
{
    public float health;
    private bool hasOpened = false;


    //AudioSource door_break;
    //AudioSource door_hit_1;
    //AudioSource door_hit_2;
    //AudioSource door_hit_3;
    public GameObject healthPotion;

    Animator animator;
    public float Health
    {
        set
        {
            health = value;

            if (health <= 0)
            {
                OpenChest();
            }
        }

        get { return health; }
    }

    private void OpenChest()
    {
        if (!hasOpened)
        {
            animator.Play("open");

            Vector3 potionPos = transform.position;
            potionPos.y += 0.07f;
            Instantiate(healthPotion, potionPos, new Quaternion());

            Debug.Log("Potion created");
            enabled = false;
            hasOpened = true;
        }
    }



    private void Start()
    {
        animator = GetComponent<Animator>();
    }


    void Update()
    {


    }

}
