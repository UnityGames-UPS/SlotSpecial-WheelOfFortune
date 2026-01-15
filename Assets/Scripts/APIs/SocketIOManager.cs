using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;


public class SocketIOManager : MonoBehaviour
{

    [Header("User Token")]
    [SerializeField] private string TestToken;
    [Header("Managers")]

    [SerializeField] internal JSFunctCalls JSManager;
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;

    [Header("Raycast Blocker")]
    [SerializeField] private GameObject RaycastBlocker;


    [Header("Other Stuff")]
    private Socket gameSocket; //BackendChanges
    protected string nameSpace = "playground"; //BackendChanges
    protected string SocketURI = null;
    protected string TestSocketURI = "http://localhost:5000";
    // protected string TestSocketURI = "https://devrealtime.dingdinghouse.com";
    protected string gameID = "SL-WOF";
    private SocketManager manager;

    private string myAuth = null;
    internal bool isResultdone = false;
    internal bool SetInit = false;
    internal GameData initialData = null;
    internal Player playerData = null;
    internal Root resultData = null;
    internal UiData initUIData = null;
    internal Features features = null;

    [Header("Ping Pong")]
    private bool isConnected = false; //Back2 Start.       //
    private bool hasEverConnected = false;          //
    private const int MaxReconnectAttempts = 5;     //
    private const float ReconnectDelaySeconds = 2f;     //
    private float lastPongTime = 0f;      //
    private float pingInterval = 2f;     //
    private bool waitingForPong = false;     //
    private int missedPongs = 0;            // 
    private const int MaxMissedPongs = 15;       //
    private Coroutine PingRoutine; //Back2 end       //

    private void Awake()
    {
        SetInit = false;
    }

    private void Start()
    {
        OpenSocket();
    }

    void ReceiveAuthToken(string jsonData)
    {
        Debug.Log("Received Auth Token Data: " + jsonData);
        // Parse the JSON data
        var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
        SocketURI = data.socketURL;
        myAuth = data.cookie;
        nameSpace = data.nameSpace;
    }

    private void OpenSocket()
    {
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = TestToken
            };
        };
        options.Auth = authFunction;
        SetupSocketManager(options);
#endif
    }

    private IEnumerator WaitForAuthToken(SocketOptions options)
    {
        // Wait until myAuth is not null
        while (myAuth == null)
        {
            Debug.Log("My Auth is null");
            yield return null;
        }
        while (SocketURI == null)
        {
            Debug.Log("My Socket is null");
            yield return null;
        }
        Debug.Log("My Auth is not null");
        // Once myAuth is set, configure the authFunction
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = myAuth,
            };
        };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
    }

    private void SetupSocketManager(SocketOptions options)
    {
        // Create and setup SocketManager
#if UNITY_EDITOR
        this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif

        if (string.IsNullOrEmpty(nameSpace) | string.IsNullOrWhiteSpace(nameSpace))
        {  //BackendChanges Start
            gameSocket = this.manager.Socket;
        }
        else
        {
            // print("nameSpace: " + nameSpace);
            Debug.Log("NameSpace used:" + nameSpace);
            gameSocket = this.manager.GetSocket("/" + nameSpace);
        }

        gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
        gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnListenEvent);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived); //Back2 Start
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice);

        manager.Open();
    }

    void OnConnected(ConnectResponse resp) //Back2 Start
    {
        Debug.Log("✅ Connected to server.");

        if (hasEverConnected)
        {
            uiManager.CheckAndClosePopups();
        }

        isConnected = true;
        hasEverConnected = true;
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        SendPing();
    } //Back2 end 

    private void OnDisconnected() //Back2 Start
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        isConnected = false;
        uiManager.DisconnectionPopup();
        ResetPingRoutine();
    } //Back2 end

    private void OnPongReceived(string data) //Back2 Start
    {
        // Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        // Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        // Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end

    private void OnError(Error err)
    {
        Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
    }

    private void OnListenEvent(string data)
    {
        // Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
    }

    private void OnSocketState(bool state)
    {
        Debug.Log("Socket State: " + state);
    }

    private void OnSocketError(string data)
    {
        Debug.Log("Socket Error!: " + data);
    }

    private void OnSocketAlert(string data)
    {
        Debug.Log("Socket Alert!: " + data);
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        // _uiManager.ADfunction();
    }

    private void SendPing() //Back2 Start
    {
        ResetPingRoutine();
        PingRoutine = StartCoroutine(PingCheck());
    }

    void ResetPingRoutine()
    {
        if (PingRoutine != null)
        {
            StopCoroutine(PingRoutine);
        }
        PingRoutine = null;
    }

    private IEnumerator PingCheck()
    {
        while (true)
        {
            // Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

            if (missedPongs == 0)
            {
                uiManager.CheckAndClosePopups();
            }

            // If waiting for pong, and timeout passed
            if (waitingForPong)
            {
                if (missedPongs == 2)
                {
                    uiManager.ReconnectionPopup();
                }
                missedPongs++;
                Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

                if (missedPongs >= MaxMissedPongs)
                {
                    Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
                    isConnected = false;
                    uiManager.DisconnectionPopup();
                    yield break;
                }
            }

            // Send next ping
            waitingForPong = true;
            lastPongTime = Time.time;
            // Debug.Log("📤 Sending ping...");
            SendDataWithNamespace("ping");
            yield return new WaitForSeconds(pingInterval);
        }
    } //Back2 end

    private void SendDataWithNamespace(string eventName, string json = null)
    {
        // Send the message
        if (gameSocket != null && gameSocket.IsOpen)
        {
            if (json != null)
            {
                gameSocket.Emit(eventName, json);
                Debug.Log("JSON data sent: " + json);
            }
            else
            {
                gameSocket.Emit(eventName);
            }
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }

    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
    }

    internal IEnumerator CloseSocket() //Back2 Start
    {
        RaycastBlocker.SetActive(true);
        ResetPingRoutine();

        Debug.Log("Closing Socket");

        manager?.Close();
        manager = null;

        Debug.Log("Waiting for socket to close");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
    } //Back2 end


    private void ParseResponse(string jsonObject)
    {
        Debug.Log(jsonObject);
        Root myData = new();
        myData = JsonConvert.DeserializeObject<Root>(jsonObject);

        string id = myData.id;
        playerData = myData.player;

        switch (id)
        {
            case "initData":
                {
                    initialData = myData.gameData;
                    initUIData = myData.uiData;
                    features = myData.features;

                    if (!SetInit)
                    {
                        SetInit = true;
                        // slotManager.shuffleInitialMatrix();
                        // gameManager.StartGame();

                        PopulateSlotGame();
                        // slotBehaviour.SocketConnected = true;
                    }
                    else
                    {
                        // _uiManager.InitialiseUIData(initUIData.paylines);
                    }
                    break;
                }
            case "ResultData":
                {
                    resultData = myData;
                    isResultdone = true;
                    break;
                }
            case "ExitUser":
                {
                    if (this.manager != null)
                    {
                        Debug.Log("Dispose my Socket");
                        gameSocket.Disconnect();
                        this.manager.Close();
                    }
#if UNITY_WEBGL && !UNITY_EDITOR
                  JSManager.SendCustomMessage("onExit");
#endif
                    // exited = true;
                    break;
                }
        }
    }

    private void PopulateSlotGame()
    {
        gameManager.StartGame();
        uiManager.InitialiseUIData(initUIData.paylines, initialData.totalLines);
        slotManager.shuffleInitialMatrix();
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnEnter");
#endif
        RaycastBlocker.SetActive(false);
    }

    internal void ReactNativeCallOnFailedToConnect() //BackendChanges
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
    }


    // private void RefreshUI()
    // {
    //     uIManager.InitialiseUIData(initUIData.AbtLogo.link, initUIData.AbtLogo.logoSprite, initUIData.ToULink, initUIData.PopLink, initUIData.paylines);
    // }

    // private void PopulateSlotSocket(List<string> slotPop, List<string> LineIds)
    // {
    //     slotManager.shuffleInitialMatrix();
    //     for (int i = 0; i < LineIds.Count; i++)
    //     {
    //         slotManager.FetchLines(LineIds[i], i);
    //     }

    //     // slotManager.SetInitialUI();
    //     isLoading = false;

    //     // Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
    // }

    internal void SendData(string eventName, object message = null)
    {

        if (gameSocket == null || !gameSocket.IsOpen)
        {
            Debug.LogWarning("Socket is not connected.");
            return;
        }
        if (message == null)
        {
            gameSocket.Emit(eventName);
            return;
        }
        isResultdone = false;
        string json = JsonConvert.SerializeObject(message);
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);

    }

    internal void AccumulateResult(int currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.type = "SPIN";
        message.payload.betIndex = currBet;

        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }
}

[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
    public string nameSpace;
}

[Serializable]
public class MessageData
{
    public string type;
    public Data payload = new();
}

[Serializable]
public class Data
{
    public int betIndex;
    public string Event;
    public List<int> index;
    public int option;
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
[Serializable]
public class Root
{
    public string id { get; set; }
    public bool success { get; set; }
    public GameData gameData { get; set; }
    public Features features { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }
    public Payload payload { get; set; }

}

[Serializable]
public class GameData
{
    public List<List<int>> lines { get; set; }
    public List<double> bets { get; set; }
    public int totalLines { get; set; }
}

[Serializable]
public class UiData
{
    public Paylines paylines { get; set; }
}

[Serializable]
public class Paylines
{
    public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
    public int id { get; set; }
    public string name { get; set; }
    public List<int> multiplier { get; set; }
    public string description { get; set; }
    public string group { get; set; }
}

[Serializable]
public class Player
{
    public double balance { get; set; }
}

[Serializable]
public class Payload
{
    public List<List<string>> reels { get; set; }
    public List<List<bool>> subSymbols { get; set; }
    public List<WinningLine> winningLines { get; set; }
    public double totalWin { get; set; }
    public WheelBonus wheelBonus { get; set; }
    public GoldSpinBonus goldSpinBonus { get; set; }
    public Features features { get; set; }
}

[Serializable]
public class WinningLine
{
    public int lineIndex { get; set; }
    public List<int> positions { get; set; }
    public string symbolId { get; set; }
    public string symbolName { get; set; }
    public float payout { get; set; }
    public int matchCount { get; set; }
    public bool isGroupWin { get; set; }
    public string groupName { get; set; }
    public int subSymbolMultiplier { get; set; }
    public int subSymbolCount { get; set; }
}

[Serializable]
public class WheelBonus
{
    public bool isTriggered { get; set; }
    public float awardValue { get; set; }
}

[Serializable]
public class GoldSpinBonus
{
    public bool isTriggered;
    public string awardType;
    public int baseAwardValue;
    public double totalWinAmount;
    public SymbolPosition symbolPosition;
    public int wheelStopIndex;
    public int awardValue;

}

[Serializable]
public class SymbolPosition
{
    public int row { get; set; }
    public int col { get; set; }
}


[Serializable]
public class Features
{
    public WheelOfFortune wheelOfFortune { get; set; }
    public GoldSpin goldSpin { get; set; }
}

[Serializable]
public class WheelOfFortune
{
    public bool enabled { get; set; }
    public bool triggered { get; set; }
    public List<int> wheelValues { get; set; }
    public float award { get; set; }
    public int wheelStopIndex { get; set; }
}

[Serializable]
public class GoldSpin
{
    public bool enabled { get; set; }
    public WheelValues wheelValues { get; set; }
    public int jackpotValue { get; set; }
    public bool triggered { get; set; }
}

[Serializable]
public class WheelValues
{
    public int jackpot { get; set; }
    public List<int> coins { get; set; }
    public List<int> multipliers { get; set; }
}
