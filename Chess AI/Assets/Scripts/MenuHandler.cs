using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    private GameObject menuItems;
    public GameObject windowsMenu;
    public GameObject androidMenu;
    public GameObject windowsMenuButton;
    public GameObject androidMenuButton;
    public DeepGold whiteAI;
    public DeepGold blackAI;
    public List<string> AIWeightSettings;
    private Dropdown AIWeightDropdown;
    private Slider AIWeightSlider;
    private Text AIWeightText;
    private InputField simulatedTurns;
    private Toggle randomFirstMove;
    private Toggle letterNotation;
    private Gene whiteAIGene;
    private Gene blackAIGene;
    private Gene currentGene;
    private bool isWhiteAI = true;

    public void Awake()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            menuItems = androidMenu;
            androidMenuButton.SetActive(true);
        }
        else
        {
            menuItems = windowsMenu;
            windowsMenuButton.SetActive(true);
        }
    }
    
    public void initializeMenu()
    {
        AIWeightDropdown = menuItems.GetComponentInChildren<Dropdown>();
        AIWeightSlider = menuItems.GetComponentInChildren<Slider>();
        simulatedTurns = menuItems.GetComponentInChildren<InputField>();
        randomFirstMove = menuItems.transform.Find("RandomFirstMove").GetComponent<Toggle>();
        letterNotation = menuItems.transform.Find("UseLetterNotation").GetComponent<Toggle>();
        AIWeightText = menuItems.transform.Find("WeightValue").GetComponentInChildren<Text>();
        AIWeightDropdown.options = new List<Dropdown.OptionData>();
        foreach (string option in AIWeightSettings)
        {
            AIWeightDropdown.options.Add(new Dropdown.OptionData(option));
        }
        AIWeightSlider.value = currentGene.Weights[0];

    }
    
    public void toggleMenu(Text buttonText)
    {
        /*
        foreach(GameObject item in allMenuItems)
        {
            item.SetActive(!item.activeSelf);
        }
        if(allMenuItems[0].activeSelf)
        {
            buttonText.text = "Close";
            updateMenu();
        }
        else
        {
            buttonText.text = "Menu";
            updateAI();
        }
        */
        menuItems.SetActive(!menuItems.activeSelf);
        if (menuItems.activeSelf)
        {
            buttonText.text = "Close";
            if (AIWeightDropdown == null)
            {
                initializeMenu();
            }
            updateMenu();
        }
        else
        {
            buttonText.text = "Menu";
            updateAI();
        }
    }

    public void Update()
    {
        if(whiteAIGene == null || blackAIGene == null)
        {
            whiteAIGene = whiteAI.gene;
            blackAIGene = blackAI.gene;
            currentGene = whiteAI.gene;
        }
        if (AIWeightText)
        {
            AIWeightText.text = "" + AIWeightSlider.value;
        }
    }

    public void switchAI(bool WhiteAI)
    {
        updateAI();
        isWhiteAI = WhiteAI;
        if (whiteAI)
        {
            currentGene = whiteAIGene;
        }
        else
        {
            currentGene = blackAIGene;
        }
        updateMenu();
    }

    public void updateAI()
    {
        if (isWhiteAI)
        {
            whiteAI.setWeights(currentGene);
            whiteAI.setSimulatedTurns(int.Parse(simulatedTurns.text));
            whiteAI.randomFirstMove = randomFirstMove.isOn;
            whiteAI.displayLetters = letterNotation.isOn;
        }
        else
        {
            blackAI.setWeights(currentGene);
            blackAI.setSimulatedTurns(int.Parse(simulatedTurns.text));
            blackAI.randomFirstMove = randomFirstMove.isOn;
            blackAI.displayLetters = letterNotation.isOn;
        }
    }

    public void updateMenu()
    {
        AIWeightDropdown.value = 0;
        AIWeightSlider.value = currentGene.Weights[0];
        if (isWhiteAI)
        {
            simulatedTurns.text = "" + whiteAI.simulatedTurns;
            randomFirstMove.isOn = whiteAI.randomFirstMove;
            letterNotation.isOn = whiteAI.displayLetters;
        }
        else
        {
            simulatedTurns.text = "" + blackAI.simulatedTurns;
            randomFirstMove.isOn = blackAI.randomFirstMove;
            letterNotation.isOn = blackAI.displayLetters;
        }
    }

    public void toggleButton(GameObject button)
    {
        Shapes2D.Shape buttonShape = button.GetComponent<Shapes2D.Shape>();
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText.text.Equals("White AI"))
        {
            buttonShape.settings.fillColor = Color.black;
            buttonText.color = Color.white;
            buttonText.text = "Black AI";
            switchAI(false);
        }
        else
        {
            buttonShape.settings.fillColor = Color.white;
            buttonText.color = Color.black;
            buttonText.text = "White AI";
            switchAI(true);
        }
    }
    public void setActive(bool active)
    {
        menuItems.SetActive(active);
    }

    public void updateDropdown()
    {
        AIWeightSlider.value = currentGene.Weights[AIWeightDropdown.value];
    }
    
    public void updateSlider()
    {
        currentGene.Weights[AIWeightDropdown.value] = AIWeightSlider.value;
    }
}
