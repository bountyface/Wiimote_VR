using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Button optionAButton;
    public ColorBlock optionAColorblock;
    public bool optionAClicked = false;
    void Start()
    {
        //optionAButton = GetComponent<Button>();
        optionAColorblock = optionAButton.colors;
        optionAColorblock.highlightedColor = new Color32(255,100,100,255);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OptionAClick()
    {
        
        Debug.Log("Option A clicked");
        //optionAColorblock.normalColor = optionAColorblock.pressedColor;
        //optionAColorblock.normalColor = new Color32(255,100,100,255);
        if (!optionAClicked)
        {
            optionAColorblock.highlightedColor = new Color32(255,100,100,255);
            optionAColorblock.pressedColor = new Color32(255,100,100,255);
            optionAColorblock.normalColor = new Color32(255,100,100,255);
            optionAButton.colors = optionAColorblock;
            
            optionAClicked = true;
        }
        else
        {
            
            optionAColorblock.pressedColor = new Color32(255,255,255,255);
            optionAColorblock.normalColor = new Color32(255,255,255,255);
            optionAButton.colors = optionAColorblock;
            optionAClicked = false;
        }
        
    }
}
