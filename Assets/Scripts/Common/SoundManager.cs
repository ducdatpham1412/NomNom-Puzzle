using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    Dictionary<MusicSource, AudioClip> MusicSources = new Dictionary<MusicSource, AudioClip>();
    Dictionary<SF, AudioClip> SFSources = new Dictionary<SF, AudioClip>();
    List<AudioSource> SFAudios = new List<AudioSource>();

    public AudioSource Music;

    void Awake() {
        Music = gameObject.AddComponent<AudioSource>();
        AudioSource sfx = gameObject.AddComponent<AudioSource>();
        SFAudios.Add(sfx);

        Music.playOnAwake = true;
        Music.loop = true;
        sfx.playOnAwake = false;
        sfx.loop = false;

        MusicSources[MusicSource.Kid] = LoadMusic("mc_kid");

        SFSources[SF.Pop_01] = LoadSF("sf_pop_01");
        SFSources[SF.Marimba_01] = LoadSF("sf_marimba_01");
        SFSources[SF.Marimba_02] = LoadSF("sf_marimba_02");
        SFSources[SF.Marimba_03] = LoadSF("sf_marimba_03");
        SFSources[SF.Marimba_04] = LoadSF("sf_marimba_04");
        SFSources[SF.Whoosh_Transition] = LoadSF("sf_whoosh_transition");
        SFSources[SF.Bubble] = LoadSF("sf_bubble");
        SFSources[SF.Win01] = LoadSF("sf_win_01");
        SFSources[SF.Win02] = LoadSF("sf_win_02");
    }

    public void PauseUnPauseMusicBackground(MusicSource source = MusicSource.Kid) {
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

    public AudioSource PlaySF(SF sf, [UnityEngine.Internal.DefaultValue("1.0F")] float volumeScale = 1f) {
        if (GameManager.Instance.profile.sfx && SFSources.ContainsKey(sf)) {
            AudioSource sfFree = SFAudios.Find(audio => !audio.isPlaying);
            if (sfFree != null) {
                sfFree.PlayOneShot(SFSources[sf], volumeScale);
                return sfFree;
            }
            AudioSource newAudio = gameObject.AddComponent<AudioSource>();
            newAudio.playOnAwake = false;
            newAudio.PlayOneShot(SFSources[sf], volumeScale);
            SFAudios.Add(newAudio);
            return newAudio;
        }
        return null;
    }

    public void PlayMusic(MusicSource source) {
        if (MusicSources.ContainsKey(source)) {
            if (Music.isPlaying) {
                Music.Pause();
            }
            Music.clip = MusicSources[source];
            Music.Play();
            Music.volume = 0.1f;
        }
    }

    public void RemoveAudioSource(AudioSource audio) {
        if (SFAudios.Contains(audio)) {
            SFAudios.Remove(audio);
            Destroy(audio);
        }
    }

    public void Initialize() { }

    AudioClip LoadMusic(string name) {
        return Resources.Load<AudioClip>($"Sounds/Musics/{name}");
    }

    AudioClip LoadSF(string name) {
        return Resources.Load<AudioClip>($"Sounds/SFs/{name}");
    }

    public enum SF {
        None,
        Pop_01,
        Pop_02,
        Marimba_01,
        Marimba_02,
        Marimba_03,
        Marimba_04,
        Whoosh_Transition,
        Bubble,
        Win01,
        Win02,
    }
    public enum MusicSource {
        Kid,
    }
}
