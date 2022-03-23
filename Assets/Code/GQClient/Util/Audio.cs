﻿using System.Collections;
using System.Collections.Generic;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.Model.mgmt.quests;
using Code.GQClient.Util.http;
using UnityEngine;
using UnityEngine.Networking;

namespace Code.GQClient.Util
{
    public class Audio
    {
        private static Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

        public static void Clear()
        {
            List<AudioSource> deleteList = new List<AudioSource>();
            deleteList.AddRange(audioSources.Values);
            foreach (AudioSource audioSrc in deleteList)
            {
                audioSrc.Stop();
                if (audioSrc.clip != null)
                {
                    // in case the clip has not finished loading yet:
                    audioSrc.clip.UnloadAudioData();
                }

                audioSrc.clip = null;
                Base.Destroy(audioSrc);
            }

            audioSources = new Dictionary<string, AudioSource>();
        }

        /// <summary>
        /// Plays audio from media store.
        /// </summary>
        /// <returns>The length of the played audio in seconds.</returns>
        /// <param name="Url">URL.</param>
        /// <param name="loop">If set to <c>true</c> loop.</param>
        /// <param name="stopOtherAudio">If set to <c>true</c> stop other audio.</param>
        public static float PlayFromMediaStore(string Url, bool loop = false, bool stopOtherAudio = true)
        {
            if (QuestManager.Instance.MediaStore.TryGetValue(Url, out _))
            {
                return PlayFromFile(Url, loop, stopOtherAudio);
            }
            else
            {
                Log.SignalErrorToAuthor("Audio file referenced at {0} not locally stored.", Url);
                return 0f;
            }
        }

        /// <summary>
        /// Plays audio from given file.
        /// </summary>
        /// <returns>The length of the played audio in seconds.</returns>
        /// <param name="path">Path.</param>
        /// <param name="loop">If set to <c>true</c> loop.</param>
        /// <param name="stopOtherAudio">If set to <c>true</c> stop other audio.</param>
        public static float PlayFromFile(string path, bool loop, bool stopOtherAudio)
        {
            // lookup the dictionary of currently prepared audiosources
            if (audioSources.TryGetValue(path, out var audioSource))
            {
                Debug.Log("Audio: locally found: start playing ...");
                _internalStartPlaying(audioSource, loop, stopOtherAudio);
                return audioSource.clip.length;
            }
            else
            {
                Debug.Log("Audio: NOT found: start loading ...");
                // NEW:
                AbstractDownloader loader;
                string loadPath = null;
                if (QuestManager.Instance.MediaStore.ContainsKey(path))
                {
                    QuestManager.Instance.MediaStore.TryGetValue(path, out var mediaInfo);
                    loadPath = mediaInfo.LocalPath;
                    // loader = new LocalFileLoader(mediaInfo.LocalPath, new DownloadHandlerBuffer());
                }
                else
                {
                    loadPath = path;
                }

                CoroutineStarter.Instance.StartCoroutine(GetAudioClip(loadPath, audioSource, loop, stopOtherAudio));

                // {
                //     loader = new Downloader(
                //         url: path,
                //         new DownloadHandlerBuffer(),
                //         timeout: Config.Current.timeoutMS,
                //         maxIdleTime: Config.Current.maxIdleTimeMS
                //     );
                //     // TODO store the image locally ...
                // }
                // loader.OnSuccess += (AbstractDownloader d, DownloadEvent e) =>
                // {
                //     var go = new GameObject("AudioSource for " + path);
                //     go.transform.SetParent(Base.Instance.transform);
                //     audioSource = go.AddComponent<AudioSource>();
                //     audioSources[path] = audioSource;
                //
                //     audioSource.clip = d.WebRequest.GetAudioClip(path, AudioType.UNKNOWN);
                //     _internalStartPlaying(audioSource, loop, stopOtherAudio);
                //     // Dispose www including it s Texture and take some logs f  or preformace surveillance:
                //     d.WebRequest.Dispose();
                // };
                // loader.Start();

                return 0f;
            }
        }

        static IEnumerator GetAudioClip(string path, AudioSource audioSource, bool loop, bool stopOtherAudio)
        {
            Debug.Log($"Audio: GetAudioClip path: {path}");
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log($"Audio: Problem loading from {path} message: {www.error}");
            }
            else
            {
                Debug.Log($"Audio: going to load load clip from {path}");

                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                var go = new GameObject("AudioSource for " + path);
                go.transform.SetParent(Base.Instance.transform);
                audioSource = go.AddComponent<AudioSource>();
                audioSources[path] = audioSource;
                
                Debug.Log($"Audio: loaded clip {audioSource.clip.name} attached to go: {go.name}");

                _internalStartPlaying(audioSource, loop, stopOtherAudio);
            }
        }


        private static IEnumerator PlayAudioFileAsynch(string path, bool loop, bool stopOtherAudio)
        {
            GameObject go = new GameObject("AudioSource for " + path);
            go.transform.SetParent(Base.Instance.transform);
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSources[path] = audioSource;
            // new AudioSource is stored in dictionary so it can be stopped already by Clear() etc.

            WWW audioWWW = new WWW(path);

            while (!audioWWW.isDone && audioSource != null)
            {
                // we wait until audio file is loaded and still audio not has been stopped in between:

                yield return null;
            }

            if (audioSource != null)
            {
                audioSource.clip = audioWWW.GetAudioClip(false, true);
                _internalStartPlaying(audioSource, loop, stopOtherAudio);
            }

            audioWWW.Dispose();
            yield break;
        }

        static void _internalStartPlaying(AudioSource audioSource, bool loop, bool stopOtherAudio)
        {
            Debug.Log("Audio: _internalStartPlaying begun ...");
            audioSource.loop = loop;
            if (stopOtherAudio)
                foreach (AudioSource audioSrc in audioSources.Values)
                {
                    if (!audioSrc.Equals(audioSource))
                    {
                        audioSrc.Stop();
                    }
                }

            audioSource.Play();
        }

        public static void StopAllAudio()
        {
            foreach (AudioSource audioSrc in audioSources.Values)
            {
                audioSrc.Stop();
            }
        }
    }
}