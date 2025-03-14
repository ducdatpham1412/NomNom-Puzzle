using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    [SerializeField] Dictionary<MusicSource, AudioClip> MusicSources = new Dictionary<MusicSource, AudioClip>();
    [SerializeField] Dictionary<SF, AudioClip> SFSources = new Dictionary<SF, AudioClip>();
    public AudioSource Music;
    AudioSource SFAudio;

    public event Action<bool> OnMusicPlaying;

    void Awake() {
        Music = gameObject.AddComponent<AudioSource>();
        SFAudio = gameObject.AddComponent<AudioSource>();

        MusicSources[MusicSource.background] = LoadMusic("mc_newdayagain");
        MusicSources[MusicSource.lifeWandering] = LoadMusic("mc_life_wandering");
        MusicSources[MusicSource.mathBeat] = LoadMusic("mc_math_beat");

        SFSources[SF.KnockWood] = LoadSF("sf_knockwood");
        SFSources[SF.Whoosh] = LoadSF("sf_whoosh");
        SFSources[SF.Stretch] = LoadSF("sf_stretch");
        SFSources[SF.Bonk] = LoadSF("sf_bonk");
        SFSources[SF.Splat] = LoadSF("sf_splat");
        SFSources[SF.Nope] = LoadSF("sf_nope");
        SFSources[SF.Haha] = LoadSF("sf_haha");
        SFSources[SF.NiceShot] = LoadSF("sf_nice_shot");
        SFSources[SF.Correct] = LoadSF("sf_correct");
        SFSources[SF.Fight] = LoadSF("sf_fight");
        SFSources[SF.NewTing] = LoadSF("sf_new_ting");
        SFSources[SF.SwitchItem] = LoadSF("sf_switch_item");
        SFSources[SF.Buy] = LoadSF("sf_buy");
        SFSources[SF.Sell] = LoadSF("sf_sell");

        AudioSource[] sources = GetComponents<AudioSource>();
        for (int i = 0; i < sources.Length; i++) {
            if (i == 0) {
                sources[i].loop = true;
                sources[i].playOnAwake = true;
            }
            else {
                sources[i].loop = false;
                sources[i].playOnAwake = false;
            }
        }
    }

    private AudioClip LoadMusic(string name) {
        return Resources.Load<AudioClip>($"Sounds/Musics/{name}");
    }

    private AudioClip LoadSF(string name) {
        return Resources.Load<AudioClip>($"Sounds/SFs/{name}");
    }

    public void PauseUnPauseMusicBackground() {
        if (Music != null) {
            if (Music.isPlaying) {
                Music.Pause();
                OnMusicPlaying?.Invoke(false);
            }
            else {
                Music.UnPause();
                OnMusicPlaying?.Invoke(true);
            }
        }
    }

    public void PlaySF(SF sf) {
        if (SFSources.ContainsKey(sf)) {
            SFAudio.PlayOneShot(SFSources[sf]);
        }
    }


    public void PlayMusic(MusicSource source) {
        if (MusicSources.ContainsKey(source)) {
            if (Music.isPlaying) {
                Music.Pause();
            }
            Music.clip = MusicSources[source];
            Music.Play();
            OnMusicPlaying?.Invoke(true);
        }
    }


    public enum SF {
        KnockWood,
        Whoosh,
        Stretch,
        Bonk,
        Splat,
        Nope,
        Haha,
        NiceShot,
        Correct,
        Fight,
        None,
        NewTing,
        SwitchItem,
        Buy,
        Sell,
    }
    public enum MusicSource {
        background,
        lifeWandering,
        mathBeat,
    }
}
