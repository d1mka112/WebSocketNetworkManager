using System;
using JetBrains.Annotations;
using Newtonsoft.Json;


abstract public class Message
{
    [JsonIgnore]
    public string Json;

    [JsonProperty("event")]
    public string Event;

    [JsonProperty("key")]
    public int Key;

    public Message(string Json)
    {
        this.Json = Json;
        this.Serialize();
    }
    public Message()
    {
        this.Serialize();
    }
    public virtual void Serialize()
    {
        this.Json = JsonConvert.SerializeObject(this);
    }
}
abstract public class Request : Message
{
    public Request(string Json) : base(Json)
    {

    }
    public Request()
    {

    }
    public override void Serialize()
    {
        base.Serialize();
        this.Json = "[\"" + this.Json.Replace("\"", "\\\"") + "\"]";
    }

}
abstract public class Response : Message
{
    [JsonProperty("type")]
    public string Type = "response";
    public Response(string Json) : base(Json)
    {

    }
    public Response()
    {

    }
    public override void Serialize()
    {

        base.Serialize();
        this.Json = "[\"" + this.Json.Replace("\"", "\\\"") + "\"]";
    }
}

public class PingMonitoringRequest : Request
{
    public class RequestData
    {
        [JsonProperty("time")]
        public int Time;
        [JsonProperty("ping")]
        public int Ping;
        public RequestData(int Time, int Ping)
        {
            this.Time = Time;
            this.Ping = Ping;
        }
    }

    [JsonProperty("data")]
    public RequestData Data;
    public PingMonitoringRequest(string Json) : base(Json)
    {

    }
    public PingMonitoringRequest(int Time, int Ping, int Key) : base()
    {
        this.Event = "check_ping_monitoring"; // <-- ??? is this normal
        this.Data = new RequestData(Time, Ping);
        this.Key = Key;

        this.Serialize();
    }
}
public class PingMonitoringResponse : Response
{
    public class ResponseResult
    {
        [JsonProperty("time")]
        public int Time;
        //public ResponseResult(long Time)
        //{
        //    this.Time = Time;
        //}
    }
    public class ResponseData
    {
        [JsonProperty("result")]
        public ResponseResult Result;
        //public ResponseData(long Time)
        //{
        //    Result = new ResponseResult(Time);
        //}
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public PingMonitoringResponse(string Json) : base(Json)
    {
        this.Event = "check_ping_monitoring";
        Serialize();
    }
}

public class PlayerAuthorizationRequest : Request
{
    public class RequestData
    {
        [JsonProperty("login")]
        public string Login;

        [JsonProperty("password")]
        public string Password;

        [JsonProperty("site")]
        public string Site;
        public RequestData(string Login, string Password, string Site)
        {
            this.Login = Login;
            this.Password = Password;
            this.Site = Site;
        }
    }

    [JsonProperty("data")]
    public RequestData Data;

    public PlayerAuthorizationRequest(string Login, string Password, string Site, int Key) : base()
    {
        this.Event = "player_login_password";
        this.Data = new RequestData(Login, Password, Site);
        this.Key = Key;
        //this.Json = "[\"{\\\"event\\\":\\\""+ this.Event + "\\\"," +
        //            "\\\"data\\\":" +
        //            "{\\\"login\\\":\\\"" + Login + "\\\"," +
        //            "\\\"password\\\":\\\"" + Password + "\\\"," +
        //            "\\\"site\\\":\\\"" + Site + "\\\"}," +
        //            "\\\"key\\\":" + Key + "}\"]";

        this.Serialize();
    }
}
public class PlayerAuthorizationResponse : Response
{
    public class ResponseResult
    {
        [JsonProperty("token")]
        public string Token;
        public ResponseResult(string Token)
        {
            this.Token = Token;
        }
    }
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
        // Do we need constructor of class for that? Probably No! Because JsonConvertDeserialize will create objects by itself.
        //
        //public ResponseData(int Code, string Message, string Token)
        //{
        //    this.Code = Code;
        //    this.Message = Message;
        //    this.Result = new ResponseResult(Token);
        //}
    }

    [JsonProperty("data")]
    public ResponseData Data;


    public PlayerAuthorizationResponse(string Json) : base(Json)
    {
        this.Event = "player_login_password";
        Serialize();
    }
}

public class OpenSessionRequest : Request
{
    public class RequestData
    {
        [JsonProperty("token")]
        public string Token;
        public RequestData(string Token)
        {
            this.Token = Token;
        }
    }

    [JsonProperty("data")]
    public RequestData Data;

    public OpenSessionRequest(string Token, int Key) : base()
    {
        this.Event = "player_session_open";
        this.Data = new RequestData(Token);
        this.Key = Key;

        this.Serialize();
    }
}
public class OpenSessionResponse : Response
{
    public class ResponseJackpotField
    {
        [JsonProperty("sum")]
        public int Sum;
    }
    public class ResponseJackpots
    {
        [JsonProperty("first")]
        public ResponseJackpotField First;

        [JsonProperty("second")]
        public ResponseJackpotField Second;
    }
    public class ResponseConfig
    {
        [JsonProperty("scroll_buttons")]
        public bool Scroll_Buttons;

        [JsonProperty("scrollButtons")]
        public bool ScrollButtons;

        [JsonProperty("payment_active")]
        public bool Payment_Active;

        [JsonProperty("paymentActive")]
        public bool PaymentActive;

        [JsonProperty("cursor")]
        public bool Cursor;

        [JsonProperty("language")]
        public string Language;

        [JsonProperty("denominations")]
        public float[] Denominations;

        [JsonProperty("jackpots")]
        public ResponseJackpots Jackpots;

        [JsonProperty("games")]
        public string[] Games;
    }
    public class ResponseResult
    {
        [JsonProperty("player_session_id")]
        public int PlayerSessionId;

        [JsonProperty("player_id")]
        public int PlayerId;

        [JsonProperty("currency")]
        public string Currency;

        [JsonProperty("server_time")]
        public long ServerTime;

        [JsonProperty("login")]
        public string login;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("config")]
        public ResponseConfig Config;

        [JsonProperty("other")]
        public Object Other; // ? this is not normal. What the Other object do?

        [JsonProperty("annonces")]
        public string[] Annonces;

        [JsonProperty("cashback")]
        public int CashBack;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public OpenSessionResponse(string Json) : base(Json)
    {
        this.Event = "player_session_open";
        Serialize();
    }
}

public class StateRequest : Request
{
    public class RequestData
    {

    }

    [JsonProperty("data")]
    public RequestData Data;

    public StateRequest(int Key) : base()
    {
        this.Event = "player_state_get";
        this.Data = new RequestData();
        this.Key = Key;

        this.Serialize();
    }
}
public class StateResponse : Response
{
    public class ResponseBonus
    {
        [JsonProperty("curr_free_games")]
        public int CurrentFreeGames;

        [JsonProperty("count_free_games")]
        public int CountFreeGames;

        [JsonProperty("count_step_free_games")]
        public int CountStepFreeGames;

        [JsonProperty("multiplier_free_games")]
        public int MultiplierFreeGames;

        [JsonProperty("cells")]
        public double[] Cells;

        [JsonProperty("bonus_multiplier")]
        public int BonusMultiplier;

        [JsonProperty("super_multiplier")]
        public int SuperMultiplier;
    }
    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("win")]
        public int Win;

        [JsonProperty("lines")]
        public int Lines;

        [JsonProperty("matrix")]
        public int[][] Matrix;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("bonus")]
        public ResponseBonus Bonus;

        [JsonProperty("other")]
        public Object Other;

        [JsonProperty("line_bet")]
        public int LineBet;

        [JsonProperty("dealer_card")]
        public int DealerCard;

        [JsonProperty("doubling_step")]
        public int DoublingStep;

        [JsonProperty("doubling_multiplier")]
        public int DoublingMultiplier;

        [JsonProperty("denomination")]
        public int Denomination;

        [JsonProperty("game_state_id")]
        public int GameStateID;

        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public StateResponse() : base()
    {
        this.Event = "player_state_get";
        Serialize();
    }
}

public class GameRequest : Request
{
    public class RequestData
    {
        [JsonProperty("type")]
        public string Type;

        public RequestData(string Type)
        {
            this.Type = Type;
        }
    }

    [JsonProperty("data")]
    public RequestData Data;

    public GameRequest(string Type, int Key) : base()
    {
        this.Event = "player_game_get";
        this.Data = new RequestData(Type);
        this.Key = Key;

        this.Serialize();
    }
}
public class GameResponse : Response
{
    public class ResponsePayout
    {
        [JsonProperty("time")]
        public long Time;

        [JsonProperty("balance")]
        public int Balance;
    }
    public class ResponseGameState
    {
        [JsonProperty("game_state_id")]
        public int GameStateID;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("payout")]
        public ResponsePayout Payout;

        [JsonProperty("status")]
        public string Status;
    }
    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("game_states")]
        public ResponseGameState[] GameStates;

        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public GameResponse() : base()
    {
        this.Event = "player_game_get";

        this.Serialize();
    }

}

public class GameSelectRequest : Request
{
    public class RequestData
    {
        [JsonProperty("game_state_id")]
        public int GameStateID;
        public RequestData(int GameStateID)
        {
            this.GameStateID = GameStateID;
        }
    }

    [JsonProperty("data")]
    public RequestData Data;

    public GameSelectRequest(int GameStateID, int Key) : base()
    {
        this.Event = "player_game_select";
        this.Data = new RequestData(GameStateID);
        this.Key = Key;

        this.Serialize();
    }
}
public class GameSelectResponse : Response
{
    public class ResponseBonus
    {
        [JsonProperty("curr_free_games")]
        public int CurrentFreeGames;

        [JsonProperty("count_free_games")]
        public int CountFreeGames;

        [JsonProperty("count_step_free_games")]
        public int CountStepFreeGames;

        [JsonProperty("multiplier_free_games")]
        public int MultiplierFreeGames;

        [JsonProperty("cells")]
        public double[] Cells;

        [JsonProperty("bonus_multiplier")]
        public int BonusMultiplier;

        [JsonProperty("super_multiplier")]
        public int SuperMultiplier;
    }
    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("win")]
        public int Win;

        [JsonProperty("lines")]
        public int Lines;

        [JsonProperty("matrix")]
        public int[][] Matrix;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("bonus")]
        public ResponseBonus Bonus;

        [JsonProperty("other")]
        public Object Other;
        // it's not cool! what kind of info collect other? 
        [JsonProperty("line_bet")]
        public int LineBet;

        [JsonProperty("dealer_card")]
        public int DealerCard;

        [JsonProperty("doubling_step")]
        public int DoublingStep;

        [JsonProperty("doubling_multiplier")]
        public int DoublingStepMultiplier;

        [JsonProperty("denomination")]
        public int Denomintation;

        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public GameSelectResponse() : base()
    {
        this.Event = "player_game_select";

        this.Serialize();
    }
}

public class SpinRequest : Request
{
    public class RequestData
    {
        [JsonProperty("denomination")]
        public float Denomination;

        [JsonProperty("lines")]
        public int Lines;

        [JsonProperty("line_bet")]
        public int LineBet;

        [JsonProperty("extra_bet")]
        public bool ExtraBet;

        public RequestData(float Denomination, int Lines, int LineBet, bool ExtraBet)
        {
            this.Denomination = Denomination;
            this.Lines = Lines;
            this.LineBet = LineBet;
            this.ExtraBet = ExtraBet;
        }
    }

    [JsonProperty("data")]
    public RequestData Data;

    public SpinRequest(float Denomination, int Lines, int LineBet, bool ExtraBet, int Key) : base()
    {
        this.Event = "game_spin_get";
        this.Data = new RequestData(Denomination, Lines, LineBet, ExtraBet);
        this.Key = Key;

        this.Serialize();
    }
}
public class SpinResponse : Response
{
    public class ResponseBonus
    {
        [JsonProperty("curr_free_games")]
        public int CurrentFreeGames;

        [JsonProperty("count_free_games")]
        public int CountFreeGames;

        [JsonProperty("count_step_free_games")]
        public int CountStepFreeGames;

        [JsonProperty("multiplier_free_games")]
        public int MultiplierFreeGames;

        [JsonProperty("cells")]
        public double[] Cells;

        [JsonProperty("bonus_multiplier")]
        public int BonusMultiplier;

        [JsonProperty("super_multiplier")]
        public int SuperMultiplier;
    }

    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("result")]
        public int Result;

        [JsonProperty("matrix")]
        public int[][] Matrix;

        [JsonProperty("bonus")]
        public ResponseBonus Bonus;

        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }

    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public SpinResponse()
    {
        this.Event = "game_spin_get";

        this.Serialize();
    }
}

public class WinRequest : Request
{
    public class RequestData
    {
        [JsonProperty("result")]
        public float Result;

        public RequestData(float Result)
        {
            this.Result = Result;
        }
    }

    [JsonProperty("data")]
    public RequestData Data;

    public WinRequest(float Result, int Key) : base()
    {
        this.Event = "game_win_take";

        this.Data = new RequestData(Result);
        this.Key = Key;

        this.Serialize();
    }
}
public class WinResponse : Response
{
    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("result")]
        public int Result;

        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("result")]
        public ResponseResult Result;

        [JsonProperty("message")]
        public string Message;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public WinResponse() : base()
    {
        this.Event = "game_win_take";

        this.Serialize();
    }
}

public class LeaveRequest : Request
{
    public class RequestData { }

    [JsonProperty("data")]
    public RequestData Data;

    public LeaveRequest(int Key) : base()
    {
        this.Event = "player_game_leave";

        this.Data = new RequestData();
        this.Key = Key;

        this.Serialize();
    }
}
public class LeaveResponse : Response
{
    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }

    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public LeaveResponse() : base()
    {
        this.Event = "player_game_leave";

        this.Serialize();
    }
}

public class CloseSessionRequest : Request
{
    public class RequestData { }

    [JsonProperty("data")]
    public RequestData Data;

    public CloseSessionRequest(int Key) : base()
    {
        this.Event = "player_session_close";

        this.Data = new RequestData();
        this.Key = Key;

        this.Serialize();
    }

}
public class CloseSessionResponse : Response
{
    public class ResponseResult
    {
        [JsonProperty("player_session_id")]
        public int PlayerSessionID;
    }
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;
    public CloseSessionResponse() : base()
    {
        this.Event = "player_session_close";

        this.Serialize();

    }
}

public class FreeSpinRequest : Request
{
    public class RequestData
    {
        public RequestData()
        {

        }
    }

    [JsonProperty("data")]
    public RequestData Data;

    public FreeSpinRequest(int Key) : base()
    {
        this.Event = "game_free_spin_get";
        this.Data = new RequestData();
        this.Key = Key;

        this.Serialize();
    }
}

public class FreeSpinResponse : Response
{
    public class ResponseBonus
    {
        [JsonProperty("curr_free_games")]
        public int CurrentFreeGames;

        [JsonProperty("count_free_games")]
        public int CountFreeGames;

        [JsonProperty("count_step_free_games")]
        public int CountStepFreeGames;

        [JsonProperty("multiplier_free_games")]
        public int MultiplierFreeGames;

        [JsonProperty("cells")]
        public double[] Cells;

        [JsonProperty("bonus_multiplier")]
        public int BonusMultiplier;

        [JsonProperty("super_multiplier")]
        public int SuperMultiplier;
    }

    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("result")]
        public int Result;

        [JsonProperty("bonus")]
        public ResponseBonus Bonus;

        [JsonProperty("matrix")]
        public int[][] Matrix;

        [JsonProperty("spin_result")]
        public int SpinResult;

        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }

    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;
    public FreeSpinResponse()
    {
        this.Event = "game_free_spin_get";

        this.Serialize();
    }
}

public class PlayerBalanceRespone : Response
{
    public class ResponseData
    {
        [JsonProperty("cashback")]
        public int Cashback;

        [JsonProperty("bonus_balance")]
        public int BonusBalance;

        [JsonProperty("default_balance")]
        public int DefaultBalance;

        [JsonProperty("payment_balance")]
        public int PaymentBalance;

        [JsonProperty("balance_type")]
        public string BalanceType;

        [JsonProperty("balance")]
        public int Balance;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public PlayerBalanceRespone() : base()
    {
        this.Event = "player_balance_changed";

        this.Serialize();
    }
}

public class GameStateStatudResponse : Response
{
    public class ResponsePayout
    {
        [JsonProperty("balance")]
        public int Balance;

        [JsonProperty("time")]
        public long Time;
    }

    public class ResponseData
    {
        [JsonProperty("game_state_id")]
        public int GameStateID;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("payout")]
        public ResponsePayout Payout;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    [JsonProperty("target")]
    public string Target;
    public GameStateStatudResponse() : base()
    {
        this.Event = "game_state_status_changed";

        this.Serialize();
    }
}

public class JackpotSumResponse : Response
{
    public class ResponseJackpotField
    {
        [JsonProperty("sum")]
        public int Sum;
    }
    public class ResponseJackpots
    {
        [JsonProperty("first")]
        public ResponseJackpotField First;

        [JsonProperty("second")]
        public ResponseJackpotField Second;

        [JsonProperty("third")]
        public ResponseJackpotField Third;
    }
    public class ResponseData
    {
        [JsonProperty("jackpots")]
        public ResponseJackpots Jackpots;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public JackpotSumResponse()
    {
        this.Event = "jackpot_sum_changed";

        this.Serialize();
    }
}

public class PlayerSessionClosedResponse : Response
{
    public class ResponseData
    {
        [JsonProperty("event")]
        public string Event;

        [JsonProperty("player_session_id")]
        public int PlayerSessionID;

    }

    [JsonProperty("data")]
    public ResponseData Data;

    public PlayerSessionClosedResponse()
    {
        this.Event = "player_session_closed";

        this.Serialize();
    }
}

public class PlayerLoginResponse : Response
{
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;
    }

    [JsonProperty("data")]
    public ResponseData Data;

    public PlayerLoginResponse(string Json) : base(Json)
    {
        this.Event = "player_login_password";

        this.Serialize();
    }
}

public class BonusRequest : Request
{
    public class RequestData
    {
        [JsonProperty("number")]
        public int Number;
        public RequestData(int Number)
        {
            this.Number = Number;
        }
    }
    [JsonProperty("data")]
    public RequestData Data;

    public BonusRequest(int Number, int Key) : base()
    {
        this.Event = "game_bonus_get";

        this.Data = new RequestData(Number);

        this.Key = Key;
        this.Serialize();
    }
}
public class BonusResponse : Response
{
    public class ResponseBonus
    {
        [JsonProperty("curr_free_games")]
        public int CurrentFreeGames;

        [JsonProperty("count_free_games")]
        public int CountFreeGames;

        [JsonProperty("count_step_free_games")]
        public int CountStepFreeGames;

        [JsonProperty("multiplier_free_games")]
        public int MultiplierFreeGames;

        [JsonProperty("cells")]
        public double[] Cells;

        [JsonProperty("bonus_multiplier")]
        public int BonusMultiplier;

        [JsonProperty("super_multiplier")]
        public int SuperMultiplier;
    }

    public class ResponseResult
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("result")]
        public int Result;

        [JsonProperty("bonus")]
        public ResponseBonus Bonus;

        [JsonProperty("multiplier")]
        public double Multiplier;

        [JsonProperty("balance")]
        public int Balance;

        [JsonProperty("cashback")]
        public int Cashback;
    }

    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("result")]
        public ResponseResult Result;
    }

    [JsonProperty("data")]
    public ResponseData Data;
    public BonusResponse() : base()
    {
        this.Event = "game_bonus_get";

        this.Serialize();
    }
}

public class GenericResponse : Response
{
    public class ResponseData
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;
    }

    [JsonProperty("data")]
    public ResponseData Data;
}
