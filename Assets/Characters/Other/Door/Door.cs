using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public float health;
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.01f;

    private float initialPositionY;
    private float shakeTimer = 0f;

    AudioSource door_break;
    AudioSource door_hit_1;
    AudioSource door_hit_2;
    AudioSource door_hit_3;

    public GameObject blockedRoom;

    public float Health
    {
        set
        {
            health = value;
            ShakeDoor();


            if (health <= 0)
            {
                door_break.Play();
                blockedRoom.SetActive(true);
                StartCoroutine(WaitForSoundAndDestroy());
                Destroy(gameObject);
            }
            else
            {
                HitSound();
            }
        }
        get { return health; }
    }

    IEnumerator WaitForSoundAndDestroy()
    {
        // Wait for the end of the frame before checking the isPlaying status
        yield return new WaitForEndOfFrame();

        // Wait until the sound has finished playing
        while (door_break.isPlaying)
        {
            yield return null;
        }

        // Destroy the GameObject after the sound has finished playing
        Destroy(gameObject);
    }

    private void HitSound()
    {
        int r = Random.Range(0, 3);

        switch (r)
        {
            case 0: door_hit_1.Play(); break;
            case 1: door_hit_2.Play(); break;
            case 2: door_hit_3.Play(); break;
        }
    }

    private void Start()
    {
        door_break = GameObject.Find("door_break").GetComponent<AudioSource>();
        door_hit_1 = GameObject.Find("door_hit_1").GetComponent<AudioSource>();
        door_hit_2 = GameObject.Find("door_hit_2").GetComponent<AudioSource>();
        door_hit_3 = GameObject.Find("door_hit_3").GetComponent<AudioSource>();
    }

    private void ShakeDoor()
    {
        initialPositionY = transform.position.y;
        shakeTimer = shakeDuration;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            Shake();
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            transform.position = new Vector3(transform.position.x, initialPositionY, transform.position.z);
        }
    }

    void Shake()
    {
        float offsetY = Mathf.Sin(Time.time * Mathf.PI * 20f) * shakeMagnitude;
        transform.position = new Vector3(transform.position.x, initialPositionY + offsetY, transform.position.z);
    }


}
