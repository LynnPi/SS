using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;

public class AudioManager : MonoBehaviour {
    private static AudioManager Instance_;
    public static AudioManager Instance {
        get {
            if( Instance_ == null ) {
                GameObject attach = new GameObject( "__AudioManager__" );
                Instance_ = attach.AddComponent<AudioManager>();
            }
            return Instance_;
        }
    }
    private const string MUSIC_PREFS_KEY = "EnableMusic";
    private const string SOUND_PREFS_KEY = "EnableSound";
    private const int ENABLE_STATE = 1;
    private const int DISABLE_STATE = 0;

    public bool EnablePlayMusic {
        get {
            return PlayerPrefs.GetInt( MUSIC_PREFS_KEY, ENABLE_STATE ) == ENABLE_STATE;
        }
        set {
            PlayerPrefs.SetInt( MUSIC_PREFS_KEY, value ? ENABLE_STATE : DISABLE_STATE );
        }
    }

    public bool EnablePlaySound {
        get {
            return PlayerPrefs.GetInt( SOUND_PREFS_KEY , ENABLE_STATE ) == ENABLE_STATE;
        }
        set {
            PlayerPrefs.SetInt( SOUND_PREFS_KEY, value ? ENABLE_STATE : DISABLE_STATE );
        }
    }

    public bool EnableListener {
        get {
            return !AudioListener.pause;
        }
        set {
            AudioListener.pause = !value;
        }
    }

    private AudioSource MusicAudioSource_;
    private AudioSource MusicAudioSource {
        get {
            if( MusicAudioSource_ == null ) {
                MusicAudioSource_ = Camera.main.GetComponent<AudioSource>();
                if( MusicAudioSource_ == null ) {
                    MusicAudioSource_ = Camera.main.gameObject.AddComponent<AudioSource>();
                }
            }
            return MusicAudioSource_;
        }
    }

    private FadingAudioSource MusicPlayer_;
    public FadingAudioSource MusicPlayer {
        get {
            if( MusicPlayer_ == null ) {
                MusicPlayer_ = Instance.gameObject.AddComponent<FadingAudioSource>();
            }
            return MusicPlayer_;
        }
    }

    private SoundPlayHelper SoundPlayer_;
    public SoundPlayHelper SoundPlayer {
        get {
            if( SoundPlayer_ == null ) {
                GameObject go = new GameObject( "__SoundPlayer__" );
                go.transform.SetParent( Instance.transform );
                go.AddComponent<AudioSource>();
                SoundPlayer_ = go.AddComponent<SoundPlayHelper>();
            }
            return SoundPlayer_;
        }
    }

    private void Awake() {
        MakeSureExistAudioListener();
    }

    private void MakeSureExistAudioListener() {
        if( Camera.main.gameObject.GetComponent<AudioListener>() == null ) {
            Camera.main.gameObject.AddComponent<AudioListener>();
        }
    }

    #region Public Interface
    /// <summary>
    /// Play sound.
    /// </summary>
    /// <param name="clip">target playing audio clip</param>
    /// <param name="position">position play sound at.</param>
    /// <param name="threeD">whether have 3d sound effect</param>
    public void PlaySound(AudioClip clip, Vector3 position, bool threeD = false ) {
        if( !EnablePlaySound ) {
            return;
        }
        SoundPlayer.PlaySound( clip, position, threeD );
    }

    /// <summary>
    /// Play music.
    /// </summary>
    /// <param name="clip">target playing audio clip</param>
    /// <param name="volume">the volume play to</param>
    /// <param name="loop">is loop to play music</param>
    public void PlayMusic( AudioClip clip, float volume, bool loop ) {
        if( !EnablePlayMusic ) {
            return;
        }
        MusicPlayer.Fade( clip, volume, loop );
    }

    /// <summary>
    /// Pause the current playing music.
    /// </summary>
    public void PauseMusic() {
        MusicPlayer.ActualAudioSource.Pause();
    }

    /// <summary>
    /// continue playing current music.
    /// </summary>
    public void UnPauseMusic() {
        MusicPlayer.ActualAudioSource.UnPause();
    }
    #endregion
}

/// <summary>
/// Audio source that fades between clips instead of playing them immediately.
/// </summary>
[RequireComponent( typeof( AudioSource ) )]
public class FadingAudioSource : MonoBehaviour {
    #region Fields
    /// <summary>
    /// Volume to end the previous clip at.
    /// </summary>
    public float FadeOutThreshold = 0.05f;

    /// <summary>
    /// Volume change per second when fading.
    /// </summary>
    public float FadeSpeed = 0.05f;

    /// <summary>
    /// Actual audio source.
    /// </summary>
    public AudioSource ActualAudioSource;

    /// <summary>
    /// Whether the audio source is currently fading, in or out.
    /// </summary>
    private FadeState FadeState_ = FadeState.None;

    /// <summary>
    /// Next clip to fade to.
    /// </summary>
    private AudioClip NextClip_;

    /// <summary>
    /// Whether to loop the next clip.
    /// </summary>
    private bool NextClipLoop_;

    /// <summary>
    /// Target volume to fade the next clip to.
    /// </summary>
    private float NextClipVolume_;

    #endregion

    #region Enums
    public enum FadeState {
        None,

        FadingOut,

        FadingIn
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Current clip of the audio source.
    /// </summary>
    public AudioClip Clip {
        get {
            return this.ActualAudioSource.clip;
        }
        set {
            this.ActualAudioSource.clip = value;
        }
    }

    /// <summary>
    /// Whether the audio source is currently playing a clip.
    /// </summary>
    public bool IsPlaying {
        get {
            return this.ActualAudioSource.isPlaying;
        }
    }

    /// <summary>
    /// Whether the audio source is looping the current clip.
    /// </summary>
    public bool Loop {
        get {
            return this.ActualAudioSource.loop;
        }
    }

    /// <summary>
    /// Current volume of the audio source.
    /// </summary>
    public float Volume {
        get {
            return this.ActualAudioSource.volume;
        }
    }
    #endregion

    #region Public Methods and Operators
    /// <summary>
    /// If the audio source is enabled and playing, fades out the current clip and fades in the specified one, after.
    /// If the audio source is enabled and not playing, fades in the specified clip immediately.
    /// If the audio source is not enabled, fades in the specified clip as soon as it gets enabled.
    /// </summary>
    /// <param name="clip">Clip to fade in.</param>
    /// <param name="volume">Volume to fade to.</param>
    /// <param name="loop">Whether to loop the new clip, or not.</param>
    public void Fade( AudioClip clip, float volume, bool loop ) {
        if( clip == null ) {
            return;
        }

        this.NextClip_ = clip;
        this.NextClipVolume_ = volume;
        this.NextClipLoop_ = loop;

        if( clip == this.ActualAudioSource.clip ) {
            return;
        }

        if( this.ActualAudioSource.enabled ) {
            if( this.IsPlaying ) {
                this.FadeState_ = FadeState.FadingOut;
            }
            else {
                this.FadeToNextClip();
            }
        }
        else {
            this.FadeToNextClip();
        }
    }

    /// <summary>
    /// Continues fading in the current audio clip.
    /// </summary>
    public void Play() {
        this.FadeState_ = FadeState.FadingIn;
        this.ActualAudioSource.Play();
        
    }

    /// <summary>
    /// Stop playing the current audio clip immediately.
    /// </summary>
    public void Stop() {
        this.ActualAudioSource.Stop();
        this.FadeState_ = FadeState.None;
    }

    #endregion

    #region Methods
    private void Awake() {
        ActualAudioSource = GetComponent<AudioSource>();
        this.ActualAudioSource.volume = 0f;
    }

    private void FadeToNextClip() {
        this.ActualAudioSource.clip = this.NextClip_;
        this.ActualAudioSource.loop = this.NextClipLoop_;

        this.FadeState_ = FadeState.FadingIn;

        if( this.ActualAudioSource.enabled ) {
            this.ActualAudioSource.Play();
        }
    }

    private void OnDisable() {
        this.ActualAudioSource.enabled = false;
        this.Stop();
    }

    private void OnEnable() {
        this.ActualAudioSource.enabled = true;
        this.Play();
    }

    private void Update() {
        if( !this.ActualAudioSource.enabled ) {
            return;
        }

        if( this.FadeState_ == FadeState.FadingOut ) {
            if( this.ActualAudioSource.volume > this.FadeOutThreshold ) {
                // Fade out current clip.
                this.ActualAudioSource.volume -= this.FadeSpeed * Time.deltaTime;
            }
            else {
                // Start fading in next clip.
                this.FadeToNextClip();
            }
        }
        else if( this.FadeState_ == FadeState.FadingIn ) {
            if( this.ActualAudioSource.volume < this.NextClipVolume_ ) {
                // Fade in next clip.
                this.ActualAudioSource.volume += this.FadeSpeed * Time.deltaTime;
            }
            else {
                // Stop fading in.
                this.FadeState_ = FadeState.None;
            }
        }
    }
    #endregion
}

/// <summary>
/// Help with playing sounds such as button sound, skill effect sound and so on.
/// </summary>
[RequireComponent( typeof( AudioSource ) )]
public class SoundPlayHelper : MonoBehaviour {
    private AudioClip LastClip_;
    private float LastPlayTime_;

    private AudioSource ActualAudioSource;

    #region Private Interface
    private void Awake() {
        if( ActualAudioSource == null ) {
            ActualAudioSource = GetComponent<AudioSource>();
            if( ActualAudioSource == null ) {
                ActualAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        this.ActualAudioSource.volume = 1f;
    }

    private void GenerateThreeDSound( AudioClip clip, Vector3 position ) {
        GameObject go = new GameObject( "__sound__" + clip.name );
        go.transform.position = position;
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = 320f;
        source.Play();
        StartCoroutine( AutoDestroy( go, clip.length ) );
    }

    private IEnumerator AutoDestroy( GameObject target, float duration ) {
        yield return new WaitForSeconds( duration );
        Destroy( target );
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Play sound at the given position with the given audio clip.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    /// <param name="threeD"></param>
    public void PlaySound( AudioClip clip, Vector3 position, bool threeD = false ) {
        if( LastClip_ == clip && (Time.time - LastPlayTime_) < (clip.length / 2) ) {
            return;
        }

        LastClip_ = clip;
        LastPlayTime_ = Time.time;

        if( threeD ) {
            GenerateThreeDSound( clip, position );
        }
        else {
            ActualAudioSource.clip = clip;
            ActualAudioSource.Play();
        }
    }
    #endregion
}