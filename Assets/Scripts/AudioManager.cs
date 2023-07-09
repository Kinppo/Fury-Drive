using System.Collections;
using Lofelt.NiceVibrations;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; protected set; }

    public AudioSource backgroundMusic;
    public AudioSource soundEffectSource;
    public AudioSource engineSoundSource;
    public bool musicIsOn, soundIsOn, vibroIsOn;
    public AudioClip click;
    public AudioClip rpgLaunch;
    public AudioClip rocketHit;
    public AudioClip explosion;
    public AudioClip carBump1;
    public AudioClip carBump2;
    public AudioClip carSquish;
    public AudioClip count1;
    public AudioClip count2;
    public AudioClip count3;
    public AudioClip count4;
    public AudioClip swordHit;
    public bool isHeavyVibration;
    private float engineVolume;

    void Awake()
    {
        Instance = this;
    }

    public void InitializeStates(bool music, bool sound, bool vibro)
    {
        musicIsOn = music;
        soundIsOn = sound;
        vibroIsOn = vibro;
        engineSoundSource.volume = soundIsOn ? 0.1f : 0;
    }

    public void UpdateMusicState()
    {
        musicIsOn = !musicIsOn;
        backgroundMusic.mute = !musicIsOn;
    }

    public void UpdateSoundState()
    {
        soundIsOn = !soundIsOn;
        engineSoundSource.volume = soundIsOn ? 0.1f : 0;
    }

    public void UpdateVibrationState()
    {
        vibroIsOn = !vibroIsOn;
    }

    public void Vibrate()
    {
        if (vibroIsOn)
        {
            var freq = 0.4f;
            if (isHeavyVibration)
            {
                isHeavyVibration = false;
                freq = 0.9f;
            }

            HapticPatterns.PlayEmphasis(freq, 0.0f);
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (soundIsOn)
        {
            soundEffectSource.clip = clip;
            soundEffectSource.Play();
        }
    }

    public void ChangeEngineVolume(bool state)
    {
        if (!soundIsOn) return;
        if ((state && engineVolume > 0) || (!state && engineVolume < 0.1f))
            StartCoroutine(UpdateEngineVolume(state));
    }

    private IEnumerator UpdateEngineVolume(bool state)
    {
        if (state && engineVolume != 0)
        {
            engineVolume -= 0.005f;
        }
        else if (!state && engineVolume < 0.1f)
            engineVolume += 0.005f;

        engineSoundSource.volume = engineVolume;

        yield return new WaitForSeconds(0.1f);
        if ((state && engineVolume > 0) || (!state && engineVolume < 0.1f))
            StartCoroutine(UpdateEngineVolume(state));
    }

    public void ClickSound()
    {
        PlaySoundEffect(click);
        Vibrate();
    }
}
