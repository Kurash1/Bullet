using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiController : MonoBehaviour
{
    [SerializeField] CharacterController2d player;
    [SerializeField] GameObject pausemenu;
    [SerializeField] private Button continuebutton;
    [SerializeField] private Button menubutton;
    // Start is called before the first frame update
    void Start()
    {
        continuebutton.onClick.AddListener(continuer);
        menubutton.onClick.AddListener(menuer);
        //
        //Debug.Log(continuebutton);
        //Debug.Log(menubutton);
        pausemenu.SetActive(false);
    }
    public void continuer()
    {
        player.Unpause();
    }
    public void menuer()
    {
        player.Unpause();
        SceneManager.LoadScene(0);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
