using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public delegate void OnInvalidAuthorization();
public delegate void OnCorrectAuthorization();
public delegate void OnSessionOpen(OpenSessionResponse response);
public delegate void OnPlayerSessionClosed(PlayerSessionClosedResponse response);
public delegate void OnInvalidSessionOpen(OpenSessionResponse response);
public delegate void OnPlayerStateGet(StateResponse response);
public delegate void OnGameStateIsBusy(GameResponse response);
public delegate void OnGameSelect(GameSelectResponse response);
public delegate void OnSpinGet(SpinResponse response);
public delegate void OnBonusResponse(BonusResponse response);
public delegate void OnLeaveGame(LeaveResponse response);
public delegate void OnFreeSpin(FreeSpinResponse response);
public delegate void OnWinTake(WinResponse response);


public class NetworkManager : Singleton<NetworkManager>
{
	private SlotsAPI _api;
	private readonly WebManager _wm = new WebManager();

	private Request _currentRequest;
	private Response _currentResponse;

	public OnInvalidAuthorization OnInvalidAuthorization;
	public OnCorrectAuthorization OnCorrectAuthorization;
	public OnSessionOpen OnSessionOpen;
	public OnPlayerSessionClosed OnPlayerSessionClosed;
	public OnInvalidSessionOpen OnInvalidSessionOpen;
	public OnPlayerStateGet OnPlayerStateGet;
	public OnGameStateIsBusy OnGameStateIsBusy;
	public OnGameSelect OnGameSelect;
	public OnSpinGet OnSpinGet;
	public OnBonusResponse OnBonusResponse;
	public OnLeaveGame OnLeaveGame;
	public OnFreeSpin OnFreeSpin;
	public OnWinTake OnWinTake;

	
	public int CurretPlayerStateID = -1;


	private Mkey.SlotPlayer slotPlayer { get { return Mkey.SlotPlayer.Instance; } }

	private List<GameResponse.ResponseGameState> ArrayOfGameStatesID;

	private bool _initialized = false;

	void Start()
	{
		if (!_initialized)
		{
            System.Random rnd = new System.Random();
			string _sessId = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
			string _server = rnd.Next(1000).ToString();

			this._api = new SlotsAPI("wss://webapi.slots.rent/transport/1/" + _server  + "/" + _sessId + "/websocket");
			//this._api = new SlotsAPI("wss://webapi.slots.rent/transport/1/628/1j4cevhj/websocket");
			//this._api = new SlotsAPI("ws://138.68.101.7:2083/transport/1/628/1j4cevhj/websocket");
			//this._api = new SlotsAPI("ws://165.227.158.107:2083/transport/1/628/1j4cevhj/websocket");

			CurretPlayerStateID = -1;

			_initialized = true;
			

			ArrayOfGameStatesID = new List<GameResponse.ResponseGameState>();

			this._api.OnMessage += MessageHandler;
			this._api.openConnection();

			StartingScreen.Instance.ShowStartingScreen();

			OnInvalidAuthorization += () =>
			{ };

			OnSessionOpen += (response) =>
			{
				if (response.Data.Message == "Ok")
				{
					Instance.OnCorrectAuthorization();
				}
			};

			OnPlayerStateGet += (response) =>
			{
				if (response.Data.Message == "Ok")
				{
					if (response.Data.Result.Type != "menu")
					{
						if (response.Data.Result.Status != "idle")
						{
							Instance._api.SelectGame(response.Data.Result.GameStateID);
							SceneManager.LoadScene("FirstLobby");
							SceneManager.LoadScene(response.Data.Result.Type);
							CurretPlayerStateID = response.Data.Result.GameStateID;
						}
						else
						{
							CurretPlayerStateID = -1;
						}
					}
					else
					{
						CurretPlayerStateID = -1;
					}
					NetworkManager.Instance.slotPlayer?.SetCoinsCount(response.Data.Result.DefaultBalance);
				}
				else
				{
					CurretPlayerStateID = -1;
				}
			};

			OnPlayerSessionClosed += (response) =>
			{
				SceneManager.LoadScene("LoginPanel");
				this._wm.JsChangeBackground("FirstLobby");
				LoginPanel loginPanel = FindObjectOfType<LoginPanel>();
				if (loginPanel != null)
				{
					//StartCoroutine(loginPanel.ShowErrorMessage("Session has been terminated"));
				}
			};

			OnGameStateIsBusy += (response) =>
			{
				//Debug.Log($"[Game States] [{response.Data.Message}]");
			};

			OnGameSelect += (response) =>
			{
				//Debug.Log($"[Game selected] Message: {response.Data.Message}");
			};

			OnSpinGet += (response) =>
			{
				if (response.Data.Message == "Ok")
				{
					//Debug.Log($"[Spin was get] Status: {response.Data.Result.Status}");
					//Debug.Log($"[Spin was get] Result: {response.Data.Result.Result}");
					//Debug.Log($"[Spin was get] Matrix: [{response.Data.Result.Matrix[1][0]}] [{response.Data.Result.Matrix[1][1]}] [{response.Data.Result.Matrix[1][2]}]");
					//Debug.Log($"[Spin was get] Balance: {response.Data.Result.Balance}");
				}
			};

			OnBonusResponse += (response) =>
			{
				if (response.Data.Message == "Ok")
				{
					string s = "[";
					foreach (var cell in response.Data.Result.Bonus.Cells)
					{
						s += cell.ToString() + ", ";
					}
					s += "]";
				}
			};

			OnLeaveGame += (response) =>
			{
				if (response.Data.Message == "Ok")
				{
					//Debug.Log($"[Leave] Message: {response.Data.Message}");
				}
			};

			OnFreeSpin += (response) =>
			{
				//Debug.Log("Free Spin get from the server...");
			};

			_api.OnConnectionClosedAction += () =>
			{
				if (Application.isPlaying) 
				{ 
					SceneManager.LoadScene("LoginPanel");
					this._wm.JsChangeBackground("FirstLobby");
					LoginPanel loginPanel = FindObjectOfType<LoginPanel>();
					if (loginPanel != null)
					{
						//StartCoroutine(loginPanel.ShowErrorMessage("Session has been terminated"));
					}
					_api.openConnection();
				}
			};

			OnWinTake += (response) =>
			{

			};

			StartCoroutine("PingRequest");
		}	
	}

	void Update()
	{
		this._api.update();
	}

    IEnumerator PingRequest()
	{
		while(true)
		{
            _api.Ping();
            yield return new WaitForSeconds(5f);
		}
	}

	private void OnApplicationQuit()
	{
		this._api.closeConnection();
	}
	
	public static void MessageHandler(string recieved)
    {
		switch (recieved)
		{
			// Spin
			case string r when r.Contains("game_spin_get"):
				SpinResponse spinResponse = JsonConvert.DeserializeObject<SpinResponse>(r);
				Instance.OnSpinGet(spinResponse);
				break;

			// Free Spin
			case string r when r.Contains("game_free_spin_get"):
				FreeSpinResponse response = JsonConvert.DeserializeObject<FreeSpinResponse>(r);
				Instance.OnFreeSpin(response);
				break;

			// Player Balance is changed
			case string r when r.Contains("player_balance_changed"):
				PlayerBalanceRespone playerResponse = JsonConvert.DeserializeObject<PlayerBalanceRespone>(r);
				int balance = playerResponse.Data.Balance;
				NetworkManager.Instance.slotPlayer?.SetCoinsCount(balance);
				break;

			// Game Win Take
			case string r when r.Contains("game_win_take"):
				WinResponse winResponse = JsonConvert.DeserializeObject<WinResponse>(r);
				Instance.OnWinTake(winResponse);
				break;

			// Bonus
			case string r when r.Contains("game_bonus_get"):
				BonusResponse bonusResponse = JsonConvert.DeserializeObject<BonusResponse>(r);
				Instance.OnBonusResponse(bonusResponse);
				break;

			// Get Game States
			case string r when r.Contains("player_game_get"):
				GameResponse playerGameResponse = JsonConvert.DeserializeObject<GameResponse>(recieved);
				if (playerGameResponse.Data.Message == "Ok")
				{
					if (playerGameResponse.Data.Result.GameStates == null)
					{
						Instance.OnGameStateIsBusy(playerGameResponse);
					}
					else
					{
						Instance.ArrayOfGameStatesID.Clear();
						Instance.ArrayOfGameStatesID = playerGameResponse.Data.Result.GameStates.ToList();

						GameResponse.ResponseGameState GameState = Instance.GetFirstState();

						if (GameState != null)
						{
							Instance.CurretPlayerStateID = GameState.GameStateID;
							Instance._api.SelectGame(GameState.GameStateID);
						}
						else
						{
							Debug.LogWarning("All terminals is busy");
						}
					}
				}
				break;

			// Select Game
			case string r when r.Contains("player_game_select"):
				GameSelectResponse gameSelectResponse = JsonConvert.DeserializeObject<GameSelectResponse>(r);

				if (gameSelectResponse.Data.Message == "Ok")
					Instance.OnGameSelect(gameSelectResponse);

				if (gameSelectResponse.Data.Message == "Terminal is busy")
                {
					SceneManager.LoadScene("FirstLobby");
					Instance._wm.JsChangeBackground("FirstLobby");
				}
					
				break;

			// Get Player State
			case string r when r.Contains("player_state_get"):
				StateResponse playerStateResponse = JsonConvert.DeserializeObject<StateResponse>(r);
				Instance.OnPlayerStateGet(playerStateResponse);
				break;

			// Leave Game State
			case string r when r.Contains("player_game_leave"):
				LeaveResponse laeveResponse = JsonConvert.DeserializeObject<LeaveResponse>(r);
				Instance.OnLeaveGame(laeveResponse);
				break;

			// Session is closed
			case string r when r.Contains("player_session_closed"):
				PlayerSessionClosedResponse closedSessResponse = JsonConvert.DeserializeObject<PlayerSessionClosedResponse>(r);
				Instance.OnPlayerSessionClosed(closedSessResponse);
				break;

			// Session is open
			case string r when r.Contains("player_session_open"):
				OpenSessionResponse sessOpenResponse = JsonConvert.DeserializeObject<OpenSessionResponse>(recieved);
				Instance.OnSessionOpen(sessOpenResponse);
				Instance._api.PlayerStateGet();
				break;

			// Login
			case string r when r.Contains("player_login_password"):
				PlayerAuthorizationResponse loginResponse = JsonConvert.DeserializeObject<PlayerAuthorizationResponse>(r);
				if (loginResponse.Data.Message == "Ok")
				{
					Instance._api.OpenSession(loginResponse.Data.Result.Token);
				}
				else if (loginResponse.Data.Message == "Invalid login or password")
				{
					Instance.OnInvalidAuthorization();
				}
				break;
		}

		// Check for generic use cases
		GenericResponse genericResponse = JsonConvert.DeserializeObject<GenericResponse>(recieved);
		if (!recieved.Contains("player_game_leave") && genericResponse.Data.Message == "Not authorized node")
		{
			SceneManager.LoadScene("LoginPanel");
			Instance._wm.JsChangeBackground("FirstLobby");
			Instance._api.openConnection(true);
		}
	}
	[ContextMenu("GetBonus")]
	public void GetBonus(int Number)
    {
		_api.GetBonus(Number);
	}
	public void GetWin(float Result)
    {
		_api.GetWin(Result);
    }

	[ContextMenu("GetWin 0")]
	public void GetWin()
    {
		_api.GetWin(0);
		
    }		

	[ContextMenu("GetFreeSpin")]
	public void Authorize(string accountNumber)
	{
		string login = accountNumber.Substring(0, 6);
		string pass = accountNumber.Substring(6);

		_api.Authorize(login, pass);
	}
	public void GetFreeSpin()
	{
		_api.GetFreeSpin();
	}
	public void GameSelect(string SceneName)
	{
		string PrevSceneName = SceneManager.GetActiveScene().name;
		if (SceneName != "FirstLobby" && SceneName != "LoginPanel")
        {
			_api.GameSelect(SceneName);
		}
		else if(PrevSceneName != "LoginPanel")
        {
			LeaveGameState();
		}	
	}
	public void LeaveGameState()
    {
		_api.LeaveGameState();
    }
	public void GetSpin(float Denomination, int Lines, int LineBet, bool ExtraBet)
    {
		_api.GetSpin(Denomination, Lines, LineBet, ExtraBet);
	}
	public void CloseSession()
	{
		_api.CloseSession();
	}

	public GameResponse.ResponseGameState GetFirstState()
	{
		for(int i = 0; i < ArrayOfGameStatesID.Count; i++)
		{
			if (ArrayOfGameStatesID[i].Status == "idle")
			{
				return ArrayOfGameStatesID[i];
			}
		}
		return null;
	}
}
