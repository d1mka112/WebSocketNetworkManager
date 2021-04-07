using System;
using UnityEngine;
using Newtonsoft.Json;
using NativeWebSocket;

public class SlotsAPI
{
    public delegate void MessageHandler(string message);
    public delegate void OnConnectionClosed();

    public MessageHandler OnMessage;

    public OnConnectionClosed OnConnectionClosedAction;

    public static bool OnOpenWithSessionToken = false;

    public Response response;

    WebSocket websocket;

    

    public bool isConnected = false;
    public string Url;

    private int _requestCounter;
    private int _ping = 0;
    private int _ping_monitoring_curr_tick = 0;

    public static string SessIDKey = "SID";

    public SlotsAPI(string Url)
    {
        this.Url = Url;
        this._requestCounter = 1;
    }
    public async void openConnection(bool reset = false)
    {       
        websocket = new WebSocket(this.Url);
        if(reset)
        {
            PlayerPrefs.DeleteKey(SessIDKey);
        }

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            isConnected = true;

            if (PlayerPrefs.HasKey(SessIDKey) && (PlayerPrefs.GetString(SessIDKey) != ""))
            {
                SlotsAPI.OnOpenWithSessionToken = true;

                Debug.Log("Loggin in with token: " + PlayerPrefs.GetString(SessIDKey));
                this.OpenSession(PlayerPrefs.GetString(SessIDKey));
            }
            else
            {
                StartingScreen.Instance.HideStartingScreen();
            }
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            isConnected = false;
            OnConnectionClosedAction();

            SlotsAPI.OnOpenWithSessionToken = false;
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);

            if (message[0] == 'a')
            {
                message = GetJsonFromMessage(message);

                // Ping
                if (message.Contains("check_ping_monitoring"))
                {
                    PingMonitoringResponse pingResponse = JsonConvert.DeserializeObject<PingMonitoringResponse>(message);

                    this._ping = this.GetUnixEpoch(DateTime.Now) - pingResponse.Data.Result.Time;
                    this._ping_monitoring_curr_tick--;
                    // Debug.Log("Check ping monitoring " + "Tick: " + this._ping_monitoring_curr_tick + " Ping: " + this._ping);
                } 
                else
                {
                    OnMessage(message);
                    if (!message.Contains("jackpot_sum_changed") && !message.Contains("check_ping_monitoring"))
                    {
                        Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
                    }
                }
            }
        };

        await websocket.Connect();
        Debug.Log("Connect");
    }

    public void update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
        #endif
    }

    public async void closeConnection()
    {
        await websocket.Close();
    }

    public void SendWebSocketMessage(string request)
    {
        if (websocket.State == WebSocketState.Open)
        {
            websocket.SendText(request);
        }
    }

    private int getKey()
    {
        _requestCounter++;
        return _requestCounter;
    }

    private int GetUnixEpoch(DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime() -
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return (int)unixTime.TotalSeconds;
    }

    public string GetJsonFromMessage(string recieved)
    {
        bool first = false;
        int start = 0;
        int end = 0;

        for (int i = 0; i < recieved.Length; i++)
        {
            if (!first && recieved[i] == '{') { first = true; start = i; }
            else if (recieved[i] == '}') { end = i; }
        }

        string message = recieved.Substring(start, end - start + 1);

        message = message.Replace(@"\", "");

        return message;
    }

    public void Ping()
    {
        this._ping_monitoring_curr_tick++;

        if (this._ping_monitoring_curr_tick > 5)
        {
            this.closeConnection();
        } else
        {
            PingMonitoringRequest request = new PingMonitoringRequest(
                Time: this.GetUnixEpoch(DateTime.Now),
                Ping: this._ping,
                Key: this.getKey()
                );

            this.SendRequest(request);
        }
    }

    public void OpenSession(string Token)
    {
        PlayerPrefs.SetString(SessIDKey, Token);

        Request request = new OpenSessionRequest(
                    Token: Token,
                    Key: this.getKey()
                    );

        this.SendRequest(request);
    }

    public void GetBonus(int Number)
    {
        BonusRequest request = new BonusRequest(
            Number: Number,
            Key: this.getKey()
            );

        this.SendRequest(request);
    }

    public void GameSelect(string game)
    {
        GameRequest request = new GameRequest(
                Type: game,
                Key: this.getKey()
                );
        this.SendRequest(request);
    }

    public void LeaveGameState()
    {
        LeaveRequest request = new LeaveRequest(
                    Key: this.getKey()
                    );
        this.SendRequest(request);
    }

    public void GetSpin(float Denomination, int Lines, int LineBet, bool ExtraBet)
    {
        SpinRequest request = new SpinRequest(
            Denomination: Denomination,
            Lines: Lines,
            LineBet: LineBet,
            ExtraBet: ExtraBet,
            Key: this.getKey()
            );
        this.SendRequest(request);
    }

    public void CloseSession()
    {
        PlayerPrefs.DeleteKey(SessIDKey);

        CloseSessionRequest request = new CloseSessionRequest(
            Key: this.getKey()
            );

        this.SendRequest(request);
    }

    public void GetFreeSpin()
    {
        FreeSpinRequest request = new FreeSpinRequest(
            Key: this.getKey()
            );
        this.SendRequest(request);
    }

    public void GetWin(float Result)
    {
        WinRequest request = new WinRequest(
                    Result: Result,
                    Key: this.getKey()
                    );
        this.SendRequest(request);
    }

    public void PlayerStateGet()
    {
        StateRequest request = new StateRequest(
                Key: this.getKey()
                );

        this.SendRequest(request);
    }

    public void Authorize(string login, string pass)
    {
        PlayerAuthorizationRequest request = new PlayerAuthorizationRequest(
			Login: login,
			Password: pass,
			Site: "web.slots.rent",
			Key: this.getKey()
            );

        this.SendRequest(request);
    }

    public void SelectGame(int StateID)
    {
        GameSelectRequest request = new GameSelectRequest(
                                GameStateID: StateID,
                                Key: this.getKey()
                                );

        this.SendRequest(request);
    }

    public void SendRequest(Request Request)
    {
        SendWebSocketMessage(Request.Json);
        if (!Request.Json.Contains("check_ping_monitoring"))
        {
            Debug.Log(Request.Json);
        }
    }
}



