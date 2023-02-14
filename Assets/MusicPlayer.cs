using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioSource aud = gameObject.GetComponent<AudioSource>();
        if(PlayerPrefs.GetString("Music") == "True")
            aud.Play();
        aud.volume = PlayerPrefs.GetFloat("MusicVolume");
    }
}
