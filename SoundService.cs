using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundService : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<AudioClip> HlasyPostav;

    public AudioSource BattleMusicStart;
    public AudioSource BattleMusic;
    public List<AudioClip> AttackSounds;
    public AudioSource AttackSource;
    public AudioSource HitSound;
    public AudioClip TalkSound;
    public AudioClip WalkSound;

    AudioSource SoundSource;

    float waitForStartToEnd = 0f;

    public void PlayMusic()
    {
        BattleMusicStart.Play();
        if (BattleMusic.resource)
            waitForStartToEnd = BattleMusicStart.clip.length;
    }

    void PlayLoop()
    {
        BattleMusicStart.Stop();
        BattleMusic.Play();
    }

    public void StopVoices()
    {
        SoundSource.Stop();
    }
    public void StopMusic()
    {
        BattleMusicStart.Stop();
        BattleMusic.Stop();
    }
    public void PlayBattleSound(int indexHlasuPostavy, int indexUtoku, bool playHit)
    {
        PlayVoice(indexHlasuPostavy);
        PlayAttackSound(indexUtoku,playHit);
    }
    void PlayVoice(int index)
    {
        SoundSource.resource = HlasyPostav[index];
        SoundSource.Play();
    }
    public void PlayAttackSound(int index, bool playHit)
    {
        if (playHit)
            HitSound.Play();
        SoundSource.resource = AttackSounds[index];
        SoundSource.Play();
    }
    public void PlayTalkSound()
    {
        SoundSource.resource = TalkSound;
        SoundSource.Play();
    }
    void Start()
    {
        SoundSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (waitForStartToEnd > 0f)
        {
            waitForStartToEnd -= Time.deltaTime;
            if (waitForStartToEnd < 0f)
                PlayLoop();
        }
    }
}
