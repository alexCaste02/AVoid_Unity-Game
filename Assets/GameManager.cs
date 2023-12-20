using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float VolumeMultiplier
    {
        set
        {
            Math.Clamp(value, 0f, 1f);
        }
        get { return volumeMultiplier; }
    }

    private float volumeMultiplier = 1f;

    private GameObject player;
    private GameObject invader;

    public AudioSource combatMusic;
    public AudioSource endMusic;
    public TMP_Text volumenText;
    AudioSource[] allAudioSources;

    private void Awake()
    {
        // Ensure there's only one instance of GameManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        invader = GameObject.FindGameObjectWithTag("Invader");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) 
            && !player.GetComponent<PlayerController>().getIsControlsEnabled()
            && !invader.GetComponent<InvaderController>().getIsControlsEnabled()
            )
        {
            SceneManager.LoadScene("START");
        }
    }




    private void FixedUpdate()
    {
        AnimatorStateInfo animatorStateInfo = player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (animatorStateInfo.IsName("Player_Dead"))
        {
            StartCoroutine(DeathReload());
        }      

        

        
    }

    private IEnumerator DeathReload()
    {
        yield return new WaitUntil(() => !player.GetComponent<PlayerController>().getIsAlive());
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("TUT_00");
    }

    public void BajarVolumen()
    {
        VolumeMultiplier -= 0.05f;
        allAudioSources = FindObjectsOfType<AudioSource>();
        
    }

    public void SubirVolumen()
    {
        VolumeMultiplier += 0.05f;
        allAudioSources = FindObjectsOfType<AudioSource>();
       

    }

    public void Salir()
    {
        Application.Quit();
    }

    public void Reiniciar()
    {
        SceneManager.LoadScene("MAP_01");
    }

    public void Tuto()
    {
        SceneManager.LoadScene("TUT_00");
    }



}

