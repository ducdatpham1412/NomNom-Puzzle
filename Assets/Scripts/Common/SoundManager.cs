using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    Dictionary<MusicSource, AudioClip> MusicSources = new Dictionary<MusicSource, AudioClip>();
    Dictionary<SF, AudioClip> SFSources = new Dictionary<SF, AudioClip>();
    List<AudioSource> SFAudios = new List<AudioSource>();

    public AudioSource Music;

    void Awake() {
        Music = gameObject.AddComponent<AudioSource>();
        SFAudios.Add(gameObject.AddComponent<AudioSource>());

        MusicSources[MusicSource.background] = LoadMusic("mc_life_wandering");

        SFSources[SF.KnockWood] = LoadSF("sf_knockwood");
        SFSources[SF.NewTing] = LoadSF("sf_new_ting");
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

    public void PauseUnPauseMusicBackground(MusicSource source = MusicSource.background) {
        if (Music == null) return;

        if (Music.isPlaying) {
            Music.Pause();
            return;
        }

        if (Music.clip == null) {
            PlayMusic(source);
        }
        else {
            Music.UnPause();
        }
    }

    public void PlaySF(SF sf) {
        if (GameManager.Instance.profile.sfx && SFSources.ContainsKey(sf)) {
            AudioSource sfFree = SFAudios.Find(audio => !audio.isPlaying);
            if (sfFree != null) {
                sfFree.PlayOneShot(SFSources[sf]);
            }
            else {
                AudioSource newAudio = gameObject.AddComponent<AudioSource>();
                newAudio.PlayOneShot(SFSources[sf]);
                SFAudios.Add(newAudio);
            }
        }
    }

    AudioClip LoadMusic(string name) {
        return Resources.Load<AudioClip>($"Sounds/Musics/{name}");
    }

    AudioClip LoadSF(string name) {
        return Resources.Load<AudioClip>($"Sounds/SFs/{name}");
    }


    public void PlayMusic(MusicSource source) {
        if (MusicSources.ContainsKey(source)) {
            if (Music.isPlaying) {
                Music.Pause();
            }
            Music.clip = MusicSources[source];
            Music.Play();
        }
    }

    public void Initialize() { }


    public enum SF {
        KnockWood,
        None,
        NewTing,
        Sell,
    }
    public enum MusicSource {
        background,
    }
}
