using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons2 : MonoBehaviour
{
    [SerializeField] Button close;
    // Start is called before the first frame update
    void Start()
    {
        close.onClick.AddListener(Close);
    }

    void Close()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
