using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialLogic : MonoBehaviour
{


    // COSAS QUE HACER
    // --------------

    // Recieve hit sound

    // MAS TIPOS ENEMIGO(DASH/TANK)
    // DIFERENTES TIPOS DEL MISMO ENEMIGO
    // BOSS
    // HABILIDADES (OBTENER TIPO ISAAC)

    // WIN CONDITIONs
    // HABITACION "CHALLENGE"
    // Fade In endcard + letters appear 1 by 1

    Transform playerLocation;
    AudioSource bgMusic;

    void Start()
    {
        playerLocation = GameObject.FindGameObjectWithTag("Player").transform;
        bgMusic = GetComponent<AudioSource>();
        bgMusic.Play();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(playerLocation.position.x < -1.8)
        {
            Application.Quit();
        }

        if(playerLocation.position.x > 1.8)
        {
            SceneManager.LoadScene("MAP_01_MULTI");
        }

        if (playerLocation.position.y > 1.15)
        {
            SceneManager.LoadScene("MAP_01");
        }
    }
}
