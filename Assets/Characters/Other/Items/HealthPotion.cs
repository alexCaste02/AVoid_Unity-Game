using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public float speed = 0.1f;  // Adjust this value to control the speed of the potion.

    GameObject player;
    GameObject invader;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            player.Health += 50;
            Destroy(gameObject);
            Debug.Log("Potion touched");
        } 
        else if (collision.gameObject.tag.Equals("Invader"))
        {
            InvaderController invader = collision.gameObject.GetComponent<InvaderController>();
            invader.Health += 50;
            Destroy(gameObject);
            Debug.Log("Potion touched");
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        invader = GameObject.FindGameObjectWithTag("Invader");
    }

    void Update()
    {
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y - 0.12f);
        Vector2 invaderPos = new Vector2(invader.transform.position.x, invader.transform.position.y - 0.12f);

        // Check if player and invader are assigned
        if (player != null && invader != null)
        {
            // Calculate distances to player and invader
            float distToPlayer = Vector2.Distance(playerPos, transform.position);
            float distToInvader = Vector2.Distance(invaderPos, transform.position);

            // Determine the target position based on the nearest one
            Vector2 targetPosition = distToPlayer < distToInvader ? playerPos : invaderPos;

            // Move the potion towards the nearest target
            transform.Translate((targetPosition - new Vector2(transform.position.x, transform.position.y))
                .normalized * speed * Time.deltaTime);
        }
    }

}
