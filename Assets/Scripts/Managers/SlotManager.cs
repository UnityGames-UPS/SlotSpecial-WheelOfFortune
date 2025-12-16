using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.UI.Extensions;
using Newtonsoft.Json;

public class SlotManager : MonoBehaviour
{

    [Header("Sprites")]
    [SerializeField] private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField] private List<SlotImage> slotmatrix;

    [Header("Slots Transforms")]
    [SerializeField] private Transform[] Slot_Transform;
    private Dictionary<int, string> x_string = new Dictionary<int, string>();
    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Animated Sprites")]
    [SerializeField] private Sprite[] bar_Sprite;
    [SerializeField] private Sprite[] fiveBar_Sprite;
    [SerializeField] private Sprite[] trippleSeven_Sprite;
    [SerializeField] private Sprite[] doubleSeven_Sprite;
    [SerializeField] private Sprite[] seven_Sprite;
    [SerializeField] private Sprite[] subBar_Sprite;
    [SerializeField] private Sprite[] subFiveBar_Sprite;
    [SerializeField] private Sprite[] subTrippleSeven_Sprite;
    [SerializeField] private Sprite[] subDoubleSeven_Sprite;
    [SerializeField] private Sprite[] subSeven_Sprite;
    [SerializeField] private Sprite[] wheelofFortuneTripleSeven_Sprite;
    [SerializeField] private Sprite[] wheelofFortuneSpin_Sprite;
    [SerializeField] private Sprite[] goldSpin_Sprite;

    [Header("Slot positions")]
    private float tweenHeight = 0;
    [SerializeField] private float reelHeight = 0;
    [SerializeField] private float stopPosition = 0;

    [Header("Miscellaneous UI")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private Button MaxBet_Button;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text BetPerLine_text;
    [SerializeField] private TMP_Text Total_lines;



    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;
    private Tween BalanceTween = null;
    [SerializeField] private List<ImageAnimation> TempList;

    [SerializeField] private Transform[] slot1Segment;

    [Header("payline")]
    [SerializeField] private Vector2 InitialLinePosition = new Vector2(-315, 100);
    [SerializeField] private int x_Distance;
    [SerializeField] private int y_Distance;
    [SerializeField] private Transform LineContainer;
    [SerializeField] private GameObject Line_Prefab;

    [Header("Managers")]
    [SerializeField] private SocketIOManager socketManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioController audioController;

    internal List<List<int>> paylines = null;
    private void Start()
    {
        tweenHeight = reelHeight + stopPosition;
    }


    private void MoveTest(float delay)
    {

        Vector3[] originalPositions = new Vector3[slot1Segment.Length];
        Transform lastElement;

        for (int i = 0; i < slot1Segment.Length; i++)
        {
            originalPositions[i] = slot1Segment[i].localPosition;
        }


        for (int i = 0; i < slot1Segment.Length - 1; i++)
        {
            slot1Segment[i].DOLocalMoveY(originalPositions[i + 1].y, delay, false).SetEase(Ease.Linear);
        }

        // Move the last element to the first position and then perform reordering
        slot1Segment[slot1Segment.Length - 1].DOLocalMoveY(originalPositions[0].y, delay, false).SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       lastElement = slot1Segment[slot1Segment.Length - 1];

                       for (int i = slot1Segment.Length - 1; i > 0; i--)
                       {
                           slot1Segment[i] = slot1Segment[i - 1];
                       }
                       slot1Segment[0] = lastElement;

                       slot1Segment[0].gameObject.SetActive(true);
                   });
    }

    private IEnumerator Move()
    {
        while (true)
        {
            MoveTest(0.4f);
            yield return new WaitForSeconds(0.4f);
        }

    }


    // TODO: Wf update  bet text
    internal void UpdateBetText(double betperline)
    {
        Debug.Log($"in update text {betperline}");
        if (audioController) audioController.PlayButtonAudio();
        if (BetPerLine_text) BetPerLine_text.text = betperline.ToString();
        if (TotalBet_text) TotalBet_text.text = betperline.ToString();
        // CompareBalance();
    }


    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }



    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();

        bool hasAnimation = true;

        switch (val)
        {
            case 1:
                for (int i = 0; i < trippleSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(trippleSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = trippleSeven_Sprite.Length - 10;
                break;
            case 2:
                for (int i = 0; i < doubleSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(doubleSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = doubleSeven_Sprite.Length - 10;
                break;
            case 3:
                for (int i = 0; i < seven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(seven_Sprite[i]);
                }
                animScript.AnimationSpeed = seven_Sprite.Length - 10;
                break;
            case 4:
                for (int i = 0; i < fiveBar_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(fiveBar_Sprite[i]);
                }
                animScript.AnimationSpeed = fiveBar_Sprite.Length - 10;
                break;
            case 5:
                for (int i = 0; i < bar_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(bar_Sprite[i]);
                }
                animScript.AnimationSpeed = bar_Sprite.Length - 10;
                break;
            case 6:
                for (int i = 0; i < subTrippleSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(subTrippleSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = subTrippleSeven_Sprite.Length - 10;
                break;
            case 7:
                for (int i = 0; i < subDoubleSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(subDoubleSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = subDoubleSeven_Sprite.Length - 10;
                break;
            case 8:
                for (int i = 0; i < subSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(subSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = subSeven_Sprite.Length - 10;
                break;
            case 9:
                for (int i = 0; i < subFiveBar_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(subFiveBar_Sprite[i]);
                }
                animScript.AnimationSpeed = subFiveBar_Sprite.Length - 10;
                break;
            case 10:
                for (int i = 0; i < subBar_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(subBar_Sprite[i]);
                }
                animScript.AnimationSpeed = subBar_Sprite.Length - 10;
                break;
            case 11:
                for (int i = 0; i < wheelofFortuneTripleSeven_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(wheelofFortuneTripleSeven_Sprite[i]);
                }
                animScript.AnimationSpeed = wheelofFortuneTripleSeven_Sprite.Length - 10;
                break;
            case 12:
                for (int i = 0; i < wheelofFortuneSpin_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(wheelofFortuneSpin_Sprite[i]);
                }
                animScript.AnimationSpeed = wheelofFortuneSpin_Sprite.Length - 10;
                break;
            case 13:
                for (int i = 0; i < goldSpin_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(goldSpin_Sprite[i]);
                }
                animScript.AnimationSpeed = goldSpin_Sprite.Length - 10;
                break;
            default:
                hasAnimation = false;
                break;
        }
        if (!hasAnimation)
        {
            animScript.StopAnimation();
            return;
        }

        animScript.AnimationSpeed = Mathf.Max(1, animScript.textureArray.Count - 10);
    }

    internal void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;

        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;
        balance = balance - (bet);

        BalanceTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.5f).OnUpdate(() =>
         {
             if (Balance_text) Balance_text.text = initAmount.ToString("f3");
         });

    }

    internal void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.transform.DOScale(new Vector2(1.3f, 1.3f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.transform.localScale = Vector3.one;
        }
    }


    internal void ResetLinesAndWins()
    {
        foreach (Transform child in LineContainer)
        {
            Destroy(child.gameObject);
        }
        TotalWin_text.text = "0";
    }
    //manage the Routine for spinning of the slots

    internal void InitiateForAnimation(List<List<string>> result)
    {
        Debug.Log(JsonConvert.SerializeObject(result));
        for (int i = 0; i < slotmatrix.Count; i++)
        {
            for (int j = 0; j < slotmatrix[i].slotImages.Count; j++)
            {
                if (int.TryParse(result[i][j], out int symbolId))
                {
                    slotmatrix[i].slotImages[j].sprite = myImages[symbolId];
                }
                PopulateAnimationSprites(slotmatrix[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), symbolId);

            }
        }
    }

    internal void UpdatePlayerData(bool result)
    {
        // DOTween.Kill(Balance_text);
        // DOTween.Kill(TotalWin_text);
        BalanceTween?.Kill();
        if (result)
        {
            TotalWin_text.text = socketManager.resultData.payload.totalWin.ToString("f3");
            Balance_text.text = socketManager.resultData.player.balance.ToString("f3");
        }
        else
        {
            Balance_text.text = socketManager.playerData.balance.ToString("f3");
        }
    }


    internal IEnumerator InitiateSpin()
    {
        float delay = 0.3f;
        float waitTime = 0.2f;

        Tweener tweener = null;
        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            if (gameManager.turboMode)
            {
                delay /= 2;
                waitTime /= 2;
            }
            // int index=i;
            // Slot_Transform[index].DOLocalMoveY(-tweenHeight, 0.25f, false).SetEase(Ease.InExpo).OnComplete(()=>{
            // Slot_Transform[index].transform.localPosition= new Vector2(Slot_Transform[index].transform.localPosition.x,stopPosition);


            // });
            tweener = Slot_Transform[i].DOLocalMoveY(-tweenHeight, delay, false).SetLoops(-1).SetEase(Ease.Linear);
            alltweens.Add(tweener);

            // .SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
            // tweener.Play();
            // InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(waitTime);
        }
    }

    internal IEnumerator TerminateSpin()
    {
        float delay = 0.35f;

        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Pause();
            if (gameManager.turboMode)
                delay /= 2;
            if (gameManager.immediateStop)
                delay = 0;

            Slot_Transform[i].localPosition = new Vector2(Slot_Transform[i].localPosition.x, 0);
            alltweens[i] = Slot_Transform[i].DOLocalMoveY(stopPosition, delay).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(delay);
            alltweens[i].Kill();
            // yield return StopTweening(Slot_Transform[i], i);
        }
        alltweens.Clear();
        // KillAllTweens();
    }


    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < slotmatrix.Count; i++)
        {

            for (int j = 0; j < slotmatrix[i].slotImages.Count; j++)
            {

                slotmatrix[i].slotImages[j].sprite = myImages[UnityEngine.Random.Range(1, myImages.Length)];

            }

        }
    }


    private void StartGameAnimation(GameObject animObjects)
    {
        int i = animObjects.transform.childCount;


        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        // animObjects.transform.GetChild(0).gameObject.SetActive(true);

        temp.StartAnimation();

        TempList.Add(temp);

    }

    //stop the icons animation
    internal void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
            // if (TempList[i].transform.childCount > 0)
            //     TempList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        TempList.Clear();
        // TempList.TrimExcess();
    }

    internal void ProcessPayoutLines(List<WinningLine> winningLines)
    {
        if (winningLines == null || winningLines.Count == 0)
            return;

        foreach (var win in winningLines)
        {
            int lineIndex = win.lineIndex;

            if (lineIndex < 0 || lineIndex >= paylines.Count)
            {
                Debug.LogWarning($"Invalid payline index: {lineIndex}");
                continue;
            }

            GeneratePayline(paylines[lineIndex]);

            HighlightWinningSymbols(win.positions);
        }
    }

    internal void ProcessPointsAnimations(List<WinningLine> winningLines)
    {
        foreach (var win in winningLines)
        {
            HighlightWinningSymbols(win.positions);
        }
    }

    private void HighlightWinningSymbols(List<int> positions)
    {
        for (int reel = 0; reel < positions.Count; reel++)
        {
            int row = positions[reel];

            if (reel < slotmatrix.Count && row < slotmatrix[reel].slotImages.Count)
            {
                StartGameAnimation(slotmatrix[reel].slotImages[row].gameObject);
            }
        }
    }

    private void GeneratePayline(List<int> y_index)
    {
        GameObject MyLineObj = Instantiate(Line_Prefab, LineContainer);
        MyLineObj.transform.localPosition = new Vector2(InitialLinePosition.x, InitialLinePosition.y);
        UILineRenderer MyLine = MyLineObj.GetComponent<UILineRenderer>();
        List<Vector2> pointlist = new List<Vector2>();
        for (int i = 0; i < 3; i++)
        {
            pointlist.Add(new Vector2(i * x_Distance, y_index[i] * -y_Distance));
        }

        MyLine.Points = pointlist.ToArray();
    }

    internal void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }

}


[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}



