using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class upgradeselector : MonoBehaviour
{
    Image img;
    InputField text;
    // Start is called before the first frame update
    void Start()
    {
        img = transform.parent.GetChild(0).GetChild(0).GetComponent<Image>();
        text = gameObject.GetComponent<InputField>();
        text.text = PlayerPrefs.GetString("upgrade");
        img.sprite = Resources.Load<Sprite>("upgrades/" + text.text);
        text.onValueChanged.AddListener(delegate {
            img.sprite = Resources.Load<Sprite>("upgrades/" + text.text);
            PlayerPrefs.SetString("upgrade", text.text);
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
