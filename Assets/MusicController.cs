using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MusicController : MonoBehaviour
{
    [SerializeField] AudioSource aud;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (aud.isPlaying)
            {
                aud.Stop();
                PlayerPrefs.SetString("Music", "False");
            }
            else
            {
                aud.Play();
                PlayerPrefs.SetString("Music", "True");
            }
        });
        Scrollbar sb = GameObject.Find("MusicSB").GetComponent<Scrollbar>();
        sb.value = PlayerPrefs.GetFloat("MusicVolume");
        aud.volume = sb.value;
        sb.onValueChanged.AddListener((float a) =>
        {
            aud.volume = sb.value;
            PlayerPrefs.SetFloat("MusicVolume", sb.value);
        });
    }
}