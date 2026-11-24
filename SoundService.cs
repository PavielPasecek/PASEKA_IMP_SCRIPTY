using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundService : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<AudioClip> HlasyPostav;

    public AudioSource BattleMusicStart;
    public AudioSource BattleMusic;
    public AudioClip BattleMusicClip;
    public AudioClip BossMusicClip;
    public List<AudioClip> AttackSounds;
    public AudioSource HitSound;
    public AudioClip TalkSound;
    public AudioClip Ambient;
    public AudioClip VictorySound;
    public AudioClip GameOver;

    AudioSource SoundSource;

    float fade = 0f;

    public void PlayMusic(bool isBoss)
    {
        BattleMusic.Stop();
        BattleMusic.volume = 0.5f;
        BattleMusic.resource = isBoss ? BossMusicClip : BattleMusicClip;
        BattleMusicStart.Play();
        if (BattleMusic.resource && BattleMusicStart.resource && !isBoss)
            BattleMusic.PlayScheduled(AudioSettings.dspTime + BattleMusicStart.clip.length);
        else
        {
            BattleMusic.Play();
        }
    }

    public void StopVoices()
    {
        SoundSource.Stop();
    }
    public void Victory()
    {
        SoundSource.resource = VictorySound;
        SoundSource.Play();
    }
    public void GameOverSound()
    {
        SoundSource.resource = GameOver;
        SoundSource.Play();
    }
    public void StopMusic()
    {
        BattleMusicStart.Stop();
        fade = 0.5f;
        Victory();
    }
    public void PlayBattleSound(int index)
    {
        if (index >= 16)
        {
            index = 16;
        }
        if (index == 0 || index == 1 || index == 4 || index == 5 || index == 16)
            HitSound.Play();
        SoundSource.resource = AttackSounds[index];
        SoundSource.Play();
    }
    public void PlayTalkSound()
    {
        SoundSource.resource = TalkSound;
        if (SoundSource.isPlaying == false)
        {
            SoundSource.Play();
        }
        
    }
    void Start()
    {
        SoundSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fade > 0f)
        {
            fade -= Time.deltaTime;
            BattleMusic.volume = fade;
            if (fade < 0f)
            {
                BattleMusic.Stop();
            }
        }
    }
}
