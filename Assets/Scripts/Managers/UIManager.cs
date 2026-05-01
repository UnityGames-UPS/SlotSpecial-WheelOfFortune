using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{

    [Header("Popus UI")]
    [SerializeField] private GameObject MainPopup_Object;


    [Header("Paytable Texts")]
    [SerializeField] private TMP_Text[] SymbolsText;
    // [SerializeField] private TMP_Text[] SymbolsTripleText;
    [SerializeField] private TMP_Text AnyComboText;
    [SerializeField] private TMP_Text SevenComboText;
    [SerializeField] private TMP_Text BarComboText;
    [SerializeField] private TMP_Text Bonus_Text;
    [SerializeField] private TMP_Text Rule_Text;
    [SerializeField] private Button PaytableExit_Button;
    [SerializeField] private Button Paytable_Button;
    [SerializeField] private GameObject Paytable_Object;
    [SerializeField] private TMP_Text WinText;
    [SerializeField] internal TMP_Text JackpotText;


    [Header("Settings Popup")]
    [SerializeField] private GameObject SettingsPopup_Object;
    [SerializeField] private Button Settings_Button;
    [SerializeField] private Button SettingsExit_Button;

    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SoundSlider;

    [Header("all Win Popup")]
    [SerializeField] private TMP_Text Win_text_title;
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private TMP_Text Win_Text;
    [SerializeField] private Button SkipWin_Button;


    [Header("low balance popup")]
    [SerializeField] private GameObject LowBalancePopup_Object;
    [SerializeField] private Button Close_Button;


    [Header("disconnection popup")]
    // [SerializeField] private Button CloseDisconnect_Button;
    [SerializeField] private GameObject DisconnectPopup_Object;
    [SerializeField] private Button DisconnectCloseButton;

    [Header("Re-connection popup")]
    // [SerializeField] private Button CloseDisconnect_Button;
    [SerializeField] private GameObject ReconnectPopup_Object;

    [Header("Quit Popup")]
    [SerializeField] private GameObject quitPopupObject;
    [SerializeField] private Button yes_Button;
    [SerializeField] private Button GameExit_Button;
    [SerializeField] private Button no_Button;
    internal Action closeSocket;


    [Header("Splash Screen")]
    [SerializeField] private GameObject spalsh_screen;
    [SerializeField] private Image progressbar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField]
    private Button QuitSplash_button;

    [Header("AnotherDevice Popup")]
    [SerializeField] private Button CloseAD_Button;
    [SerializeField] private GameObject ADPopup_Object;
    //private int FreeSpins;
    [Header("Pagination")]
    [SerializeField] private int CurrentIndex = 0;
    [SerializeField] private GameObject[] paytableList;
    [SerializeField] private Button RightBtn;
    [SerializeField] private Button LeftBtn;

    [Header("coin panel ")]
    [SerializeField] private Button CoinBtn;
    [SerializeField] private GameObject CoinPanel;
    [SerializeField] private Button ExitCoinbtn;
    [SerializeField] private Button OkBtn;
    [SerializeField] private Button ExitBtn;
    [SerializeField] private Button OptBtnPrefab;
    [SerializeField] private Transform GridParent;
    [SerializeField] private TMP_Text coinValue;
    [SerializeField] private TMP_Text totalValue;
    [SerializeField] private Sprite btnHighlight;
    [SerializeField] private Sprite btnNormal;

    private bool isExit = false;

    internal GameObject ActivePopup = null;
    [Header("Classes ")]
    [SerializeField] private SocketIOManager socketManager;
    [SerializeField] private AudioController audioController;
    [SerializeField] private GameManager gameManager;
    internal Action PlayButtonAudio;

    internal Action<float, string> ToggleAudio;


    internal float currentCoin = 10f;
    private double selectedBet = 1;
    private int selectedBtnIndex;

    private void Start()
    {
        SetButton(DisconnectCloseButton, CallOnExitFunction);
        SetButton(yes_Button, CallOnExitFunction);
        SetButton(no_Button, () => ClosePopup());
        SetButton(GameExit_Button, () => OpenPopup(quitPopupObject));

        SetButton(Settings_Button, () => OpenPopup(SettingsPopup_Object));
        SetButton(SettingsExit_Button, () => ClosePopup());

        SetButton(Paytable_Button, () => OpenPopup(Paytable_Object));
        SetButton(PaytableExit_Button, () => ClosePopup());

        MusicSlider.onValueChanged.AddListener(ToggleMusic);
        SoundSlider.onValueChanged.AddListener(ToggleSound);

        SetButton(LeftBtn, () => Slide(false));
        SetButton(RightBtn, () => Slide(true));

        // if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        // if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(delegate { CallOnExitFunction(); socketManager.ReactNativeCallOnFailedToConnect(); }); //BackendChanges


        SetButton(Close_Button, () => ClosePopup());
        SetButton(QuitSplash_button, () => OpenPopup(quitPopupObject));

        SetButton(CloseAD_Button, CallOnExitFunction);

        SkipWin_Button.onClick.AddListener(() => CloseWinPopUp());

        SetButton(ExitCoinbtn, () => ClosePopup());
        SetButton(CoinBtn, () =>
        {
            OpenPopup(CoinPanel);
            ChangeBtuntext(gameManager.BetCounter);
        });

        OkBtn.onClick.RemoveAllListeners();
        OkBtn.onClick.AddListener(() =>
        {
            PlayButtonAudio?.Invoke();
            gameManager.BetCounter = selectedBtnIndex;
            ClosePopup();
            gameManager.SetTotalBet(selectedBtnIndex);


        });

    }

    void SetButton(Button button, Action action)
    {
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                PlayButtonAudio?.Invoke();
                action();
            });
        }
    }
    internal void PopulateWin(int value, double amount)
    {
        switch (value)
        {
            case 1:
                if (Win_text_title) Win_text_title.text = "Big Win";
                break;
            case 2:
                if (Win_text_title) Win_text_title.text = "Huge Win";
                break;
            case 3:
                if (Win_text_title) Win_text_title.text = "Mega Win";
                break;
        }

        double initAmount = 0;

        OpenPopup(WinPopup_Object);

        DOTween.To(() => initAmount, (val) => initAmount = val, amount, 3f).OnUpdate(() =>
        {
            if (Win_Text) Win_Text.text = initAmount.ToString("f3");
        });

        Invoke(nameof(CloseWinPopUp), 4f);
    }

    void CloseWinPopUp()
    {
        GameManager.checkWin = false;
        DOTween.Kill(Win_Text.transform);
        ClosePopup();
    }

    internal void LowBalPopup()
    {
        OpenPopup(LowBalancePopup_Object);
    }

    internal void InitialiseUIData(Paylines paylines, int linescount)
    {
        for (int i = 1; i < SymbolsText.Length; i++)
        {
            SymbolsText[i].text = (paylines.symbols[i].payout * socketManager.initialData.bets[gameManager.BetCounter]).ToString();
        }

        AnyComboText.text = "Any 3x " + socketManager.features.anyPayouts.defaults.ToString();
        SevenComboText.text = "Any 3x " + socketManager.features.anyPayouts.sevens.ToString();
        BarComboText.text = "Any 3x " + socketManager.features.anyPayouts.bars.ToString();

        WinText.text = $"No of Positions = {linescount * 3} \nWinnings are calculated based on bet per line \nBet per line = Total bet / No of Positions ";

    }

    internal void ADPopUp()
    {
        OpenPopup(ADPopup_Object);
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        StartCoroutine(socketManager.CloseSocket());
    }


    private void OpenPopup(GameObject Popup)
    {
        // if(playAudio)
        audioController.PlayButtonAudio();

        if (ActivePopup != null && !DisconnectPopup_Object.activeSelf)
        {
            ActivePopup.SetActive(false);
            ActivePopup = null;
        }

        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        ActivePopup = Popup;
        paytableList[CurrentIndex = 0].SetActive(true);
    }

    private void ClosePopup()
    {
        // if(playAudio)
        audioController.PlayButtonAudio();
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
            if (ActivePopup) ActivePopup.SetActive(false);
        }
        paytableList[CurrentIndex].SetActive(false);
    }



    private void Slide(bool inc)
    {
        if (inc)
        {
            CurrentIndex++;
            if (CurrentIndex > paytableList.Length - 1)
                CurrentIndex = 0;


        }
        else
        {

            CurrentIndex--;
            if (CurrentIndex < 0)
                CurrentIndex = paytableList.Length - 1;

        }

        for (int i = 0; i < paytableList.Length; i++)
        {
            paytableList[i].SetActive(false);
        }

        paytableList[CurrentIndex].SetActive(true);




    }

    internal void DisconnectionPopup()
    {

        // ClosePopup(ReconnectPopup_Object);
        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }

    }
    internal void ReconnectionPopup()
    {
        OpenPopup(ReconnectPopup_Object);
    }

    internal void CheckAndClosePopups()
    {
        if (ReconnectPopup_Object.activeInHierarchy)
        {
            CloseConnectionPopup(ReconnectPopup_Object);
        }
        if (DisconnectPopup_Object.activeInHierarchy)
        {
            CloseConnectionPopup(DisconnectPopup_Object);
        }
    }

    private void CloseConnectionPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    private void ToggleMusic(float value)
    {

        ToggleAudio?.Invoke(value, "bg");
    }

    private void ToggleSound(float value)
    {
        ToggleAudio?.Invoke(value, "button");
        ToggleAudio?.Invoke(value, "wl");

    }




    private List<Button> spawnedButtons = new List<Button>();


    internal void ChangeBtuntext(int index)
    {
        foreach (var item in spawnedButtons)
        {
            item.image.sprite = btnNormal;
        }
        Debug.Log("Button Clicked+.  " + index);
        selectedBtnIndex = index;
        spawnedButtons[index].image.sprite = btnHighlight;
        UpdateTotal(socketManager.initialData.bets[index]);
    }
    internal void SpawnButtons()
    {
        for (int i = 0; i < socketManager.initialData.bets.Count; i++)
        {
            int index = i;
            double value = socketManager.initialData.bets[i];

            Button btn = Instantiate(OptBtnPrefab, GridParent);
            spawnedButtons.Add(btn);

            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            txt.text = value.ToString();

            // Click Select
            btn.onClick.AddListener(() =>
            {
                SelectBet(value);
                ChangeBtuntext(index);
            });

            // Hover Events
            AddHoverEvent(btn, value);
        }
    }
    private void AddHoverEvent(Button btn, double bet)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) =>
        {
            UpdateTotal(bet);
        });
        trigger.triggers.Add(enter);

        // Pointer Exit
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) =>
        {
            UpdateTotal(selectedBet);
        });
        trigger.triggers.Add(exit);
    }
    private void SelectBet(double bet)
    {
        selectedBet = bet;
        UpdateTotal(selectedBet);

        Debug.Log("Selected Bet: " + selectedBet);
    }

    private void UpdateTotal(double bet)
    {
        selectedBet = bet;
        totalValue.text = (currentCoin * bet).ToString("0.##");
    }
}
