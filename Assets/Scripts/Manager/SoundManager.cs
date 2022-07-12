using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// 音乐组件
    /// </summary>
    AudioSource m_MusicAudio;
    /// <summary>
    /// 音效组件
    /// </summary>
    AudioSource m_SoundAudio;

    /// <summary>
    /// 音效的音量
    /// </summary>
    private float SoundVolume
    {
        get
        {
            return PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        }
        set
        {
            m_SoundAudio.volume = value;
            //存储音量的值
            PlayerPrefs.SetFloat("SoundVolume", value);
        }
    }

    /// <summary>
    /// 音乐的音量
    /// </summary>
    private float MusicVolume
    {
        get
        {
            return PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        }
        set
        {
            m_MusicAudio.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }

    void Awake()
    {
        m_MusicAudio = this.gameObject.AddComponent<AudioSource>();
        m_MusicAudio.playOnAwake = false;
        m_MusicAudio.loop = true;

        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.loop = false;
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="name"></param>
    public void PlayMusic(string name)
    {
        //音量小于0.1就不播放了
        if (this.MusicVolume < 0.1f)
        {
            return;
        }
        //相同的音乐不重复播放
        string oldName = "";
        if (m_MusicAudio.clip != null)
        {
            oldName = m_MusicAudio.clip.name;
        }
        if (oldName == name)
        {
            m_MusicAudio.Play();
            return;
        }
        //播放音乐
        Manager.Resource.LoadMusic(name, (Object obj) =>
        {
            m_MusicAudio.clip = obj as AudioClip;
            m_MusicAudio.Play();
        });
    }

    /// <summary>
    /// 暂停音乐
    /// </summary>
    public void PauseMusic()
    {
        m_MusicAudio.Pause();
    }

    /// <summary>
    /// 继续播放音乐
    /// </summary>
    public void OnUnPauseMusic()
    {
        m_MusicAudio.UnPause();
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    public void StopMusic()
    {
        m_MusicAudio.Stop();
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name"></param>
    public void PlaySound(string name)
    {
        if (this.SoundVolume < 0.1f)
        {
            return;
        }
            
        Manager.Resource.LoadSound(name, (UnityEngine.Object obj) =>
        {
            //音效只播放一次
            m_SoundAudio.PlayOneShot(obj as AudioClip);
        });
    }

    /// <summary>
    /// 设置音乐音量
    /// </summary>
    /// <param name="value"></param>
    public void SetMusicVolume(float value)
    {
        this.MusicVolume = value;
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    /// <param name="value"></param>
    public void SetSoundVolume(float value)
    {
        this.SoundVolume = value;
    }
}
