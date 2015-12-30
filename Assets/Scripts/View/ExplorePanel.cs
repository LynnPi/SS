using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public enum TouchDir {
    None,
    Left,
    Right,
    Up,
    Down
}

public class ExplorePanel : UIPanelBehaviour {

    public static ExplorePanel Instance {
        get;
        private set;
    }
    public bool IsSettingsOpen = false;
    public bool IsKnapsackOpen = false;

    private Text TravelDistanceLabel_;
    private Text TechnicLabel_;
    private Text ElectricLabel_;
    private Animator Anim_;

    private GameObject[] ArrKnapsack_;
    private Transform[] ArrCenters_;
    private Transform[] ArrIcons_;
    private Transform KnapsackTran_;
    private Transform SettingsTran_;
    private Transform TailTran_;
    private Transform HeadTran_;
    private Vector2 TailPos_ = new Vector2( 9.35f, 17.0f );
    private float TailRotationz_ = 55.0f;
    private Vector3 PrePos_;
    private Vector3 OffsetPos_;
    private Image Hp_;
    private Transform PointerTran_;
    private Image MaskImage_;
    private Button FloatBtn_;
    private Button ThrusterBtn_;
    private Button RelandedBtn_;

    private float Timer_ = 0.0f;
    private float Duration_ = 0.25f;

    private bool IsSettingsForward_ = false;
    private bool IsSettingsReverse_ = false;
    public bool IsKnapsackForward_ = false;
    public bool IsKnapsackReverse_= false;
    private float MaxSlideToLeft_;
    private float RotationAngle_;
    private float TargetAngle_;
    private float AnglePerSec_;
    private float SlideAngleCounter_ = 0.0f;
    private float TouchSensitive_ = 2.0f;
    private float SinglePropRotationz_ = 31.95f;
    private float HeadTailPropRotationz_ = 41.6f;
    private float SlideRatio_ = 0.8f;
    private string[] ArrUINum_ = { "~", "~~", "~~~", "~~~~", "~~~~~" };

    //private float SlideDistPerHeadProp_ = 43.0f;
    //private float SlideDistPerSingleProp_ = 33.0f;
    private GameObject WarningFilter_;

    private int TestPropNum_ = 4;
    private Text BuyOxygenText_;
    private Text BuyFuelText_;
    private Text BuyTipText_;
    private int PropFuelNumber_;
    private int PropFuelNumber {
        get {
            return PropFuelNumber_;
        }
        set {
            PropFuelNumber_ = value;
            BuyFuelText_.text = value.ToString();
        }
    }
    private int PropOxygenNumber_;
    private int PropOxygenNumber {
        get {
            return PropOxygenNumber_;
        }
        set {
            PropOxygenNumber_ = value;
            BuyOxygenText_.text = value.ToString();
        }
    }

    protected override void OnAwake() {
        Instance = this;
        //Cannot change the order of the following assignment-funcs
        Assign();
        AssignSettings();
        AssignKnapsack();
        AssignProps();
        AssignBuyText();

        UpdateMusicBtnView( AudioManager.Instance.EnablePlayMusic );

        InitWarningFilter();
    }


    private void InitWarningFilter() {
        WarningFilter_ = UIManager.Instance.CreateUI( "WarningFilter", UIManager.Instance.PanelCanvas.gameObject );
        WarningFilter_.SetActive( false );
    }

    public void SetWarningFilterActive(bool active) {
        if( WarningFilter_.activeInHierarchy == active ) return;
        WarningFilter_.SetActive( active );
        if( active ) {
            WarningFilter_.transform.SetAsFirstSibling();
        }
    }

    #region Variable Assignment

    private void Assign() {
        Anim_ = GetComponent<Animator>();
        MaskImage_ = GetComponent<Image>();
        TravelDistanceLabel_ = transform.FindChild( "Image_TravelDistance/Text" ).GetComponent<Text>();
        TravelDistanceLabel_.text = "0 m";
        TechnicLabel_ = transform.FindChild( "Image_Technic/Text" ).GetComponent<Text>();
        ElectricLabel_ = transform.FindChild( "Image_Electric/Text" ).GetComponent<Text>();
        Hp_ = transform.FindChild( "BottomPanel/Image_Hp" ).GetComponent<Image>();
        PointerTran_ = transform.FindChild( "BottomPanel/Image_Pointer" );
        FloatBtn_ = transform.FindChild( "Button_Float" ).GetComponent<Button>();
        FloatBtn_.onClick.AddListener( OnClickFloatBtn );
        ThrusterBtn_ = transform.FindChild( "Button_RoleThruster" ).GetComponent<Button>();
        RelandedBtn_ = transform.FindChild( "Button_Relanded" ).GetComponent<Button>();

        Button btn;
        btn = GetComponent<Button>();
        btn.onClick.AddListener(delegate {
            if( IsKnapsackOpen ) {
                OnKnapsackReturnGameClick();
            }
            else if( IsSettingsOpen ) {
                OnSettingsReturnGameClick();
            }
        } );
    }

    private void AssignSettings() {
        Button btn;
        SettingsTran_ = transform.FindChild( "BottomPanel/Settings" );
        btn = transform.FindChild( "BottomPanel/Button_Settings" ).GetComponent<Button>();
        btn.onClick.AddListener( OnSettingsClick );
        btn = SettingsTran_.FindChild( "2/Button_ReturnGame" ).GetComponent<Button>();
        btn.onClick.AddListener( OnSettingsReturnGameClick );
        btn = SettingsTran_.FindChild( "1/Button_Music" ).GetComponent<Button>();
        btn.onClick.AddListener( OnMusicBtnClick );
        btn = SettingsTran_.FindChild( "0/Button_ExitGame" ).GetComponent<Button>();
        btn.onClick.AddListener( OnBackToQuestScene );
    }

    private void AssignKnapsack() {
        Button btn;
        btn = transform.FindChild( "BottomPanel/Button_Knapsack" ).GetComponent<Button>();
        btn.onClick.AddListener( OnKnapsackClick );
        btn = transform.FindChild( "BottomPanel/Props_Knapsack/Tail" ).GetComponent<Button>();
        btn.onClick.AddListener( OnKnapsackReturnGameClick );
        btn = transform.FindChild( "BottomPanel/Props_Knapsack/1" ).GetComponent<Button>();
        btn.onClick.AddListener( OnXXClick );
    }

    private void AssignProps() {
        HeadTran_ = transform.FindChild( "BottomPanel/Knapsack/Head" );
        TailTran_ = transform.FindChild( "BottomPanel/Knapsack/Tail" );
        KnapsackTran_ = transform.FindChild( "BottomPanel/Knapsack" );
        ArrKnapsack_ = new GameObject[9];
        ArrCenters_ = new Transform[ArrKnapsack_.Length + 2];
        ArrIcons_ = new Transform[ArrCenters_.Length];
        for( int i = 0; i < ArrKnapsack_.Length; i++ ) {
            ArrKnapsack_[i] = transform.FindChild( "BottomPanel/Knapsack/" + i + "/" + i ).gameObject;
            ArrCenters_[i + 2] = ArrKnapsack_[i].transform.FindChild( "Center" );
            ArrIcons_[i + 2] = transform.FindChild( "BottomPanel/Props_Knapsack/" + i );
        }
        ArrCenters_[0] = HeadTran_.FindChild( "Center" );
        ArrIcons_[0] = transform.FindChild( "BottomPanel/Props_Knapsack/Head" );
        ArrCenters_[1] = TailTran_.FindChild( "Center" );
        ArrIcons_[1] = transform.FindChild( "BottomPanel/Props_Knapsack/Tail" );

    }

    private void AssignBuyText() {
        BuyFuelText_ = ArrIcons_[2].FindChild( "Text" ).GetComponent<Text>();
        BuyOxygenText_ = ArrIcons_[0].FindChild( "Text" ).GetComponent<Text>();
        BuyFuelText_.transform.parent.GetComponent<Button>().onClick.AddListener( OnFuelClick );
        BuyOxygenText_.transform.parent.GetComponent<Button>().onClick.AddListener( OnOxygenClick );
        BuyTipText_ = transform.FindChild( "Text_BuyTip" ).GetComponent<Text>();
    } 
    #endregion

    protected override void OnShow( params object[] args ) {
        Init();
    }

    private void Init() {
        UpdateElectricityView();
        UpdateFuelView();
        UpdateOxygenView();
        UpdateTechView();
        SetKnapsackInfo();
        PropFuelNumber = 1;
        PropOxygenNumber = 1;
        Hp_.fillAmount = 1.0f;
        PointerTran_.localEulerAngles = Vector3.forward * 270;
        TravelDistanceLabel_.text = ConvertToUINumber(0)+"m";
        MaxSlideToLeft_ = 2 * HeadTailPropRotationz_ + (TestPropNum_ - 2) * SinglePropRotationz_;
        int temp = TestPropNum_ > 5 ? 5 : TestPropNum_;
        RotationAngle_ = (2 * HeadTailPropRotationz_ + (temp - 2) * SinglePropRotationz_);
    }

    protected override void OnUpdate() {
        AdjustIcons();
        PlayTween();
        if( false == IsKnapsackOpen)
            return;

        TouchDir touchDir = GetTouchDir();
        if(touchDir != TouchDir.None ) {
            float angle = - OffsetPos_.x * SlideRatio_;
            //AdjustIcons();
            if (touchDir == TouchDir.Left && KnapsackTran_.localEulerAngles.z+angle> (397 + 32 * (TestPropNum_ - 6))%360) {
                KnapsackTran_.localEulerAngles = Vector3.forward * (397 + 32 * (TestPropNum_ - 6));
                SlideAngleCounter_ = -MaxSlideToLeft_;
                return;
            }else if (touchDir == TouchDir.Right && KnapsackTran_.localEulerAngles.z+angle< 5.75f ) {
                SlideAngleCounter_ = 0;
                KnapsackTran_.localEulerAngles = Vector3.forward*366;
                return;
            }
            KnapsackTran_.localEulerAngles += Vector3.forward * angle;
        }
    }

    private void AdjustIcons() {
        for( int i = 0; i < TestPropNum_; i++ ) {
            ArrIcons_[i].position = ArrCenters_[i].position;
        }
    }

    private void PlayTween() {
        if( IsKnapsackForward_ ) {
            PlayTween( KnapsackTran_, TargetAngle_, ref IsKnapsackForward_ );
            if( false == IsKnapsackForward_ ) {
                IsKnapsackOpen = true;
                SetPropsActive();
            }
        }

        if( IsKnapsackReverse_ ) {
            if( Timer_ == 0.0f ) {
                SetPropsDeactive();
            }
            PlayTween( KnapsackTran_, TargetAngle_, ref IsKnapsackReverse_ );
            if (false == IsKnapsackReverse_ ) {
                IsKnapsackOpen = false;
                AdjustIcons();
            }
        }

        if( IsSettingsForward_ ) {
            PlayTween( SettingsTran_, -116.0f, ref IsSettingsForward_ );
            if (false == IsSettingsForward_ ) {
                IsSettingsOpen = true;
            }
        }

        if( IsSettingsReverse_ ) {
            PlayTween( SettingsTran_, 0,  ref IsSettingsReverse_ );
            if (false == IsSettingsReverse_ ) {
                IsSettingsOpen = false;
            }
        }
    }

    private void PlayTween( Transform tran, float target, ref bool flag ) {
        Timer_ += Time.deltaTime;
        tran.localEulerAngles += Vector3.forward * AnglePerSec_ * Time.deltaTime;
        if( Timer_ >= Duration_ ) {
            tran.localEulerAngles = Vector3.forward * target;
            Timer_ = 0.0f;
            flag = false;
        }
    }

    private TouchDir GetTouchDir() {
        TouchDir touchDir = TouchDir.None;
        if( Input.GetMouseButtonDown( 0 ) ) {
            PrePos_ = Input.mousePosition;
        }

        if( Input.GetMouseButton( 0 ) ) {
            OffsetPos_ = Input.mousePosition - PrePos_;
            if( Mathf.Abs( OffsetPos_.x ) > TouchSensitive_) {
                touchDir = OffsetPos_.x > 0 ? TouchDir.Right : TouchDir.Left;
                PrePos_ = Input.mousePosition;
                SlideAngleCounter_ += OffsetPos_.x* SlideRatio_;
                if( !CanSlide( touchDir) ) {
                    SlideAngleCounter_ += -OffsetPos_.x* SlideRatio_;
                    touchDir = TouchDir.None;
                }
            }
        }
        return touchDir;
    }

    private bool CanSlide( TouchDir touchDir) {
        if( TestPropNum_ <= 5 ) return false;
        if (touchDir == TouchDir.Left ) {
            float r = RotationAngle_ + Mathf.Abs( SlideAngleCounter_ );
            if( r > MaxSlideToLeft_ ) {
                KnapsackTran_.localEulerAngles = Vector3.forward * (397 + 32 * (TestPropNum_ - 6));
                return false;
            }
        }
        else {
            if (SlideAngleCounter_ >= 0 ) {
                KnapsackTran_.localEulerAngles = Vector3.forward * 366;
                return false;
            }
        }
        return true;
    }
    
    private void SetKnapsackInfo() {
        if( TestPropNum_ <= 5 ) {
            for( int i = 0; i < TestPropNum_ - 2; i++ ) {
                ArrKnapsack_[i].gameObject.SetActive( true );
            }
            SetTail();
        }
        else if ( TestPropNum_ > 5 ) {//5-10, >10
            for( int i = 0; i < 4; i++ ) {
                ArrKnapsack_[i].gameObject.SetActive( true );
            }
        }
    }

    private void SetPropsActive() {
        for (int i=4; i<TestPropNum_-2; i++ ) {
            if( i >= ArrKnapsack_.Length )
                break;
            ArrKnapsack_[i].gameObject.SetActive( true );
        }
        
        if( TestPropNum_ <= 10 ) {
            SetTail();
        }
    }

    private void SetPropsDeactive() {
        if( TestPropNum_ <= 5 ) return;
        if( !TailTran_.gameObject.activeSelf ) return;
        TailTran_.gameObject.SetActive( false );
        for (int i=4; i<TestPropNum_; i++ ) {
            if( i >= ArrKnapsack_.Length )
                break;
            ArrKnapsack_[i].gameObject.SetActive( false );
        }
    }

    private void SetTail() {
        TailTran_.gameObject.SetActive( true );
        TailTran_.SetParent( ArrKnapsack_[TestPropNum_ - 2].transform.parent );
        TailTran_.localPosition = TailPos_;
        TailTran_.localEulerAngles = Vector3.forward*TailRotationz_ ;
    }

    private void CloseMenu() {
        if( IsKnapsackOpen ) {
            OnKnapsackReturnGameClick();
        }

        if( IsSettingsOpen ) {
            OnSettingsReturnGameClick();
        }
    }

    #region Settings Event
    public void OnSettingsClick() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        if( ExploreController.Instance.WreUtility.IsShocking ) return;
        Anim_.Play( "Button_Hide" );
        AnglePerSec_ = -116.0f / Duration_;
        IsSettingsForward_ = true;
        MaskImage_.enabled = true;
    }

    private void OnSettingsReturnGameClick() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        Anim_.Play( "Button_Show" );
        AnglePerSec_ = 115.5f / Duration_;
        IsSettingsReverse_ = true;
        MaskImage_.enabled = false;
    }

    private void OnMusicBtnClick() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        string clipName;
        if( !GlobalConfig.MusicToScene.TryGetValue( SceneType.Explore.ToString(), out clipName ) ) {
            return;
        }

        AudioClip clip = AssetBundleLoader.Instance.GetAsset( AssetType.Audio, clipName ) as AudioClip;
        if( clip == null ) {
            return;
        }

        if( AudioManager.Instance.EnablePlayMusic ) {//to be off.
            AudioManager.Instance.EnablePlayMusic = false;
            AudioManager.Instance.PauseMusic();

        }
        else {//to be on.
            AudioManager.Instance.EnablePlayMusic = true;
            float fadeDuration = 1f;
            AudioManager.Instance.MusicPlayer.FadeSpeed = 1 / fadeDuration;
            if(AudioManager.Instance.MusicPlayer.Clip == null ) {
                AudioManager.Instance.PlayMusic( clip, 1f, true );
            }
            else {
                AudioManager.Instance.UnPauseMusic();
            }
        }

        UpdateMusicBtnView( AudioManager.Instance.EnablePlayMusic );
    }

    private void UpdateMusicBtnView(bool on) {
        //临时的，到时候根据美术要求来
        Image img = SettingsTran_.FindChild( "1/Button_Music" ).GetComponent<Image>();
        img.color = on ? Color.white : Color.red;
    }

    private void OnBackToQuestScene() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        CloseMenu();
        StartCoroutine( EnterQuestScene() );
    }

    private IEnumerator EnterQuestScene() {
        while(IsKnapsackOpen || IsSettingsOpen ) {
            yield return null;
        }
        yield return new WaitForSeconds( 0.3f );
        SceneManager.Instance.EnterScene( SceneType.Quest );
    }

    #endregion

    #region Knapsack Event

    private void OnXXClick() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
    }

    private void OnKnapsackClick() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        if( ExploreController.Instance.WreUtility.IsShocking ) return;
        Anim_.Play( "Button_Hide" );
        SlideAngleCounter_ = 0.0f;
        AnglePerSec_ = RotationAngle_ / Duration_;
        TargetAngle_ = KnapsackTran_.localEulerAngles.z + RotationAngle_;
        MaskImage_.enabled = true;
        AdjustIcons();
        IsKnapsackForward_ = true;
    }

    private void OnKnapsackReturnGameClick() {
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        Anim_.Play( "Button_Show" );
        AnglePerSec_ = -(Mathf.Abs( SlideAngleCounter_ ) + RotationAngle_) / Duration_;
        TargetAngle_ = 186.7f;
        IsKnapsackReverse_ = true;
        MaskImage_.enabled = false;

        BuyTipText_.gameObject.SetActive( false );
        BuyTipText_.text = string.Empty;
    }

    private void OnFuelClick() {
        if( ExploreController.Instance.IsGameOver ) return;
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        BuyTipText_.gameObject.SetActive( true );
        if( PropFuelNumber > 0 ) {
            PropFuelNumber--;
            BuyTipText_.text = "使用成功！ 您的<color=#ff0000ff>燃料</color>已加满，您的征途不会停止!";
            ExploreController.Instance.BuyFuelToFull();
            UIManager.Instance.PlayUISound( "Sound/pickup" );
            OnFuelRecovery();
        }
    }

    private void OnOxygenClick() {
        if( ExploreController.Instance.IsGameOver ) return;
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        BuyTipText_.gameObject.SetActive( true );
        if( PropOxygenNumber  > 0 ) {
            PropOxygenNumber--;
            BuyTipText_.text = "使用成功！ 您的<color=#ff0000ff>氧气</color>已加满，您的征途不会停止!";
            ExploreController.Instance.BuyOxygenToFull();
            UIManager.Instance.PlayUISound( "Sound/pickup" );
        }
    }


    #endregion

    #region Set View

    private void SetTechnic( int value ) {
        TechnicLabel_.text = value.ToString();
    }

    private void SetElectric( int value ) {
        ElectricLabel_.text = value.ToString();
    } 
    #endregion

    private void OnClickFloatBtn() {
        //Debugger.Log("点击悬浮按钮");
        UIManager.Instance.PlayUISound( "Sound/click_button" );
        Anim_.Play( "Button_FloatClick" );
        ExploreController.Instance.OnReceivePlayerInputOfDetaching();
    }

    private string ConvertToUINumber( float num ) {
        string str = Convert.ToInt32( num ).ToString();
        if( str.Length > 6 ) return "999999";
        return ArrUINum_[5 - str.Length] + str;
    }
     
    public void UpdateTravelDistanceView(int distance) {
        TravelDistanceLabel_.text = string.Format( "{0}m", ConvertToUINumber( distance ) );
    }

    public void UpdateOxygenView() {
        if (ExploreController.Instance.WreUtility.ShouldUpdateOxygen ) {//防止对电力核心和残骸触电的影响
            float currentPercent = (float)PlayerDataCenter.CurrentRoleInfo.Oxygen / PlayerDataCenter.CurrentRoleInfo.MaxOxygen;
            if( Hp_.fillAmount <= 0 ) return;
            Hp_.fillAmount = currentPercent;
        }
    }

    public void UpdateFuelView() {
        float currentPercent = (float)PlayerDataCenter.CurrentRoleInfo.Fuel / PlayerDataCenter.CurrentRoleInfo.MaxFuel;
        PointerTran_.localEulerAngles = Vector3.forward * (270 + (1 - currentPercent) * 180);
    }

    public void UpdateElectricityView() {
        ElectricLabel_.text = ConvertToUINumber( PlayerDataCenter.CurrentRoleInfo.ElectricityAmount);
    }

    public void UpdateTechView() {
        TechnicLabel_.text = ConvertToUINumber( PlayerDataCenter.CurrentRoleInfo.TechAmount);
    }

    public void OnGameOver() {
        CloseMenu();
    }

    public void SetOxygenWhenLoseElectrc(float currentOxygen ) {
        float currentPercent = currentOxygen / PlayerDataCenter.CurrentRoleInfo.MaxOxygen;
        if( Hp_.fillAmount <= 0 ) {
            return;
        }
        Hp_.fillAmount = currentPercent;
    }

    public void OnFuelUseOut() {
        ThrusterBtn_.interactable = false;
        RelandedBtn_.interactable = false;
        FloatBtn_.gameObject.SetActive( false );
        Player player = ExploreController.Instance.CurrentPlayer;
        player.MyPushHandler.Active = false;
        player.RigidBody.velocity = Vector3.zero;
        player.PushSpeed = 0;
    }

    public void OnFuelRecovery() {
        if( PlayerDataCenter.CurrentRoleInfo.Fuel > 0 ) {
            ThrusterBtn_.interactable = true;
            RelandedBtn_.interactable = true;
            FloatBtn_.gameObject.SetActive( true );
            Player player = ExploreController.Instance.CurrentPlayer;
            player.MyPushHandler.Active = true;
            player.RigidBody.velocity = Vector3.zero;
            player.PushSpeed = 0;
        }
    }
}
