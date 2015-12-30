using UnityEngine;
using System.Collections;
using System;

public class ExploreController : MonoBehaviour {
    public static ExploreController Instance { get; private set; }
    public ExploreGuideHelper MyExploreGuideHelper;
    public bool IsGameOver { get; set; }
    public float BuringFrequency = 1f;//update value every "BuringFrequency" second.
    public int FuelBurningSpeed;
    public int OxygenBurningSpeed;
    private int DecOxygenWhenMeetLoseElectric_=20;
    public bool IsPackageOpen {
        get {
            return UIDisplayer.IsKnapsackOpen || UIDisplayer.IsSettingsOpen;
        }
    }
    private int CurrentFuelValue_;
    public int CurrentFuelValue {
        get {
            return CurrentFuelValue_;
        }
        set {
            if( value <= 0 ) {
                CurrentFuelValue_ = PlayerDataCenter.CurrentRoleInfo.Fuel = 0;
                OnFuelUseOut();
            }
            else if (value>=PlayerDataCenter.CurrentRoleInfo.MaxFuel){
                CurrentFuelValue_ = PlayerDataCenter.CurrentRoleInfo.Fuel = PlayerDataCenter.CurrentRoleInfo.MaxFuel;
            }
            else {
                CurrentFuelValue_ = PlayerDataCenter.CurrentRoleInfo.Fuel = value;
            }
            UIDisplayer.UpdateFuelView();
        }
    }

    #region fuel and oxygen warning 
    private bool IsWaringFuelUsingOut_;
    private bool IsWarningOxygenUsingOut_;
    private float UseOutWarningPerecent_ = 0.2f;
    private float FuelWarningDuration_ = 2.5f;
    private float OxygenWarningDuration_ = 1.2f;

    private void EliminateFuelWarning() {
        IsWaringFuelUsingOut_ = false;
    }

    private void EliminateOxygenWarning() {
        //UIDisplayer.SetWarningFilterActive( false );
        IsWarningOxygenUsingOut_ = false;
    }

    private void CheckFuelWillBeUsingOut() {
        if( IsWaringFuelUsingOut_ || IsGameOver ) return;
        float fuelRemainPercent = (float)CurrentFuelValue_ / PlayerDataCenter.CurrentRoleInfo.MaxFuel;
        if( fuelRemainPercent <= UseOutWarningPerecent_ && fuelRemainPercent > 0.05 ) {
            Debugger.LogFormat( "Now fuel remains {0}%, will be using out!", UseOutWarningPerecent_ * 100 );
            UIManager.Instance.PlayUISound( "Sound/fuel_warning" );
            IsWaringFuelUsingOut_ = true;
            Invoke( "EliminateFuelWarning", FuelWarningDuration_ );
            
        }
    }

    private void CheckOxygenWillBeUsingOut() {
        if( IsWarningOxygenUsingOut_ || IsGameOver ) return;
        float oxygenRemainPercent = (float)CurrentOxygenValue_ / PlayerDataCenter.CurrentRoleInfo.MaxOxygen;
        if( oxygenRemainPercent <= UseOutWarningPerecent_ && oxygenRemainPercent > 0.05 ) {
            Debugger.LogFormat( "Now oxygen remains {0}%, will be using out!", UseOutWarningPerecent_ * 100 );
            UIManager.Instance.PlayUISound( "Sound/oxygen_warning" );
            IsWarningOxygenUsingOut_ = true;
           
            Invoke( "EliminateOxygenWarning", OxygenWarningDuration_ );
        }
    }

    private void CheckShowOxygenEffect() {
        if( IsGameOver ) return;
        float oxygenRemainPercent = (float)CurrentOxygenValue_ / PlayerDataCenter.CurrentRoleInfo.MaxOxygen;
        if( oxygenRemainPercent <= UseOutWarningPerecent_ && oxygenRemainPercent > 0.05 ) {
            UIDisplayer.SetWarningFilterActive( true );
        }
        else {
            UIDisplayer.SetWarningFilterActive( false );
        }
    }
    #endregion

    private int CurrentOxygenValue_;
    public int CurrentOxygenValue {
        get {
            return CurrentOxygenValue_;
        }
        set {
            if( value <= 0 ) {
                CurrentOxygenValue_ = PlayerDataCenter.CurrentRoleInfo.Oxygen = 0;
                OnOxygenUseOut();
            }
            else if (value >= PlayerDataCenter.CurrentRoleInfo.MaxOxygen){
                CurrentOxygenValue_ = PlayerDataCenter.CurrentRoleInfo.Oxygen = PlayerDataCenter.CurrentRoleInfo.MaxOxygen;
            }
            else {
                CurrentOxygenValue_ = PlayerDataCenter.CurrentRoleInfo.Oxygen = value;
            }
            UIDisplayer.UpdateOxygenView();
        }
    }

    private LevelGenerator LevelSystem_;
    public LevelGenerator LevelSystem {
        get {
            if( LevelSystem_ == null ) {
                LevelSystem_ = LevelGenerator.Create( transform );
            }
            return LevelSystem_;
        }
    }

    public Player CurrentPlayer { get; private set; }

    private ExplorePanel UIDisplayer_;
    private ExplorePanel UIDisplayer {
        get {
            if( UIDisplayer_ == null ) {
                Transform root = UIManager.Instance.PanelCanvas.transform;
                UIDisplayer_ = root.FindChild( "ExplorePanel" ).GetComponent<ExplorePanel>();
            }
            return UIDisplayer_;
        }
    }

    private PickUpHelper PickUp_;
    public PickUpHelper PickUp {
        get {
            if (null == PickUp_ ) {
                GameObject pickupHelper = new GameObject( "__PickUpHelper__" );
                pickupHelper.transform.SetParent( transform );
                PickUp_ = pickupHelper.AddComponent<PickUpHelper>();
            }
            return PickUp_;
        }
    }

    private WreckageUtility WreUtility_;
    public WreckageUtility WreUtility {
        get {
            if (null == WreUtility_ ) {
                GameObject wreUtility = new GameObject( "__WreckageUtility__" );
                wreUtility.transform.SetParent( transform );
                WreUtility_ = wreUtility.AddComponent<WreckageUtility>();
            }
            return WreUtility_;
        }
    }

    private IEnumerator BurningFuel() {
        while( CurrentFuelValue > 0 ) {
            CurrentFuelValue -= FuelBurningSpeed;
            yield return new WaitForSeconds( BuringFrequency );
        }
    }

    private IEnumerator BurningOxygen() {
        while( CurrentOxygenValue > 0 ) {
            CurrentOxygenValue -= OxygenBurningSpeed;
            yield return new WaitForSeconds( BuringFrequency );
        }
    }

    private void SetLights() {
        GameObject lightRoot = SceneManager.Instance.SceneRoot.FindChild( "Lights" ).gameObject;
        lightRoot.SetActive( true );
    }

    private void OnDestroy() {
        if( CurrentPlayer ) {
            Destroy( CurrentPlayer.gameObject );
        }
    }

    private void Awake() {
        Instance = this;
        PlayerDataCenter.InitPlayerStatus();
    }

    private IEnumerator Start() {
        yield return StartCoroutine( LevelSystem.Init() );
        SetLights();
        SetCameraInfo();
        InitPlayerState();
        StartExploreLogic();
        SetPositionGuider();
    }

    private void Update() {
        CheckFuelWillBeUsingOut();
        CheckOxygenWillBeUsingOut();
        CheckShowOxygenEffect();
    }

    private void InitPlayerState() {
        //需等到关卡生成完毕后，才能进入该状态，不然无法获取到当前的目标残骸
        CurrentPlayer = Player.CreateInExplore();
        CurrentPlayer.FSM.SetContext( CurrentPlayer );
        CurrentPlayer.FSM.SendEvent( "StartExploring" );
        CurrentPlayer.OnTravelDistanceChanged += UIDisplayer.UpdateTravelDistanceView;
    }

    private void SetCameraInfo() {
        CameraManager.Instance.SetCameraView_WhenInExploreScene();
    }

    private void StartExploreLogic() {
        CurrentFuelValue = PlayerDataCenter.CurrentRoleInfo.Fuel;
        CurrentOxygenValue = PlayerDataCenter.CurrentRoleInfo.Oxygen;
        OxygenBurningSpeed = PlayerDataCenter.CurrentRoleInfo.OxygenSpeed;
        StartCoroutine( BurningFuel() );
        StartCoroutine( BurningOxygen() );
    }
    
    private void OnOxygenUseOut() {
        UIManager.ShowPanel<GameOverPanel>();
        PickUp.OnOxygenUseOut();
        IsGameOver = true;
    }

    private void OnFuelUseOut() {
        //UIManager.ShowPanel<GameOverPanel>();
        //IsGameOver = true;
        ExplorePanel.Instance.OnFuelUseOut();
    }

    private void UpdateElectricityAmount() {
        UIDisplayer.UpdateElectricityView();
    }

    private void UpdateTechAmount() {
        UIDisplayer.UpdateTechView();
    }

    private void SetPositionGuider() {
        GameObject attach = new GameObject( "__ExploreGuideHelper__" );
        attach.transform.SetParent( transform );
        MyExploreGuideHelper = attach.AddComponent<ExploreGuideHelper>();
    }

    public void OnReceivePlayerInputOfDetaching() {
        CurrentPlayer.FSM.SendEvent( "TakingOff" );
    }

    public void OnMeetLoseElectric() {
        CurrentOxygenValue = CurrentOxygenValue_ - DecOxygenWhenMeetLoseElectric_;
    }

    public int GetDecValWhenLoseElecric() {
        return DecOxygenWhenMeetLoseElectric_;
    }

    public void Die() {
        CurrentOxygenValue = 0;
    }

    public void AddPointWhenPickedOxygen(OxygenPoint oxygenPoint) {
        CurrentOxygenValue += oxygenPoint.Point;
    }

    public void AddPointWhenPickedFuel( FuelPoint fuelPoint ) {
        int temp = CurrentFuelValue;
        CurrentFuelValue += fuelPoint.Point;
        if( temp <= 0 ) {
            StartCoroutine( BurningFuel() );
        }
    }

    public void BuyOxygenToFull() {
        CurrentOxygenValue = PlayerDataCenter.CurrentRoleInfo.MaxOxygen;
    }

    public void BuyFuelToFull() {
        int temp = CurrentFuelValue;
        CurrentFuelValue = PlayerDataCenter.CurrentRoleInfo.MaxFuel;
        if ( temp <= 0) {
            StartCoroutine( BurningFuel() );
        }
    }

    public void SetOxygenWhenLoseElectrc(float currentOxygen) {
        UIDisplayer.SetOxygenWhenLoseElectrc(currentOxygen);
    }
}
