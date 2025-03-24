using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace RegUtility
{
    public class RegIO
    {
        /// <summary>
        /// csv�t�@�C������s���ǂݍ����string�z���List�Ƃ��ĕԂ��B
        /// </summary>
        /// <param name="path">�p�X�B</param>
        /// <returns></returns>
        static public List<string[]> ReadCSV(TextAsset asset)
        {
            return ReadCSV(asset.text);
        }

        /// <summary>
        /// csv�t�@�C������s���ǂݍ����string�z���List�Ƃ��ĕԂ��B
        /// </summary>
        /// <param name="path">�p�X�B</param>
        /// <returns></returns>
        static public List<string[]> ReadCSV(string path) => ReadCSV(path, false);
        static public List<string[]> ReadCSV(string path,bool skipFirstLine)
        {
            var list = new List<string[]>();
            var sr = new StreamReader(path);
            int n = skipFirstLine ? 0 : 1;

            while (sr.Peek() != -1)
            {
                string text = sr.ReadLine();
                if (n++ == 0) continue;
                list.Add(text.Split(','));
            }

            sr.Close();

            return list;
        }

        /// <summary>
        /// txt�t�@�C������s���ǂݍ����string��List�Ƃ��ĕԂ��B
        /// </summary>
        /// <param name="path">�p�X�B</param>
        /// <returns></returns>
        static public List<string> ReadLines(TextAsset asset)
        {
            return new List<string>(asset.text.Split("\r\n"));
        }

        /// <summary>
        /// txt�t�@�C������s���ǂݍ����string��List�Ƃ��ĕԂ��B
        /// </summary>
        /// <param name="path">�p�X�B</param>
        /// <returns></returns>
        static public List<string> ReadLines(string path)
        {
            var sr = new StreamReader(path);
            var list = new List<string>(sr.ReadToEnd().Split("\r\n"));
            sr.Close();

            return list;
        }

        /// <summary>
        /// �摜�t�@�C����ǂݍ����Sprite�Ƃ��ĕԂ��B
        /// </summary>
        /// <param name="path">�p�X�B</param>
        /// <returns></returns>
        static public Sprite LoadSprite(string path)
        {
            //Debug.Log("loadsprite: " + path);

            var rawData = File.ReadAllBytes(path);
            Texture2D tex2d = new Texture2D(0, 0);
            tex2d.LoadImage(rawData);
            var sprite = Sprite.Create(tex2d, new Rect(0f, 0f, tex2d.width, tex2d.height), Vector2.one / 2f, 100f);

            return sprite;
        }

        /// <summary>
        /// ��΃p�X�����ɉ����t�@�C����ǂݍ����AudioClip�Ƃ��ĕԂ��B
        /// ��) AudioClip ac = await new RegIO().LoadAudioClip(��΃p�X);
        /// </summary>
        /// <param name="path">�t�@�C���p�X�B��΃p�X�ɂ��邱�ƁB</param>
        /// <returns></returns>
        static public async UniTask<AudioClip>  LoadAudioClip(string path)
        {
            string path_url = "file:///" + path;

            AudioType audioType = new AudioType();

            switch (Path.GetExtension(path_url))
            {
                case ".wav":
                case ".WAV":
                    audioType = AudioType.WAV;
                    break;
                case ".ogg":
                case ".OGG":
                    audioType = AudioType.OGGVORBIS;
                    break;
                case ".mp3":
                case ".MP3":
                    audioType = AudioType.MPEG;
                    break;
                case ".m4a":
                    Debug.LogError("No supported audio format.--" + Path.GetExtension(path));
                    break;
                default:
                    audioType = AudioType.WAV;
                    break;
            }

            var req = UnityWebRequestMultimedia.GetAudioClip(path_url, audioType);

            ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = true;

            await req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(req.error);
                    return null;
                }

            return DownloadHandlerAudioClip.GetContent(req);
        }

        /*
        /// <summary>
        /// WWW�N���X��p���Đ�΃p�X�����ɉ����t�@�C����ǂݍ����AudioClip�Ƃ��ĕԂ��B
        /// ��) AudioClip ac = new RegIO().LoadAudioClipWWW(��΃p�X);
        /// </summary>
        /// <param name="path">�t�@�C���p�X�B��΃p�X�ɂ��邱�ƁB</param>
        /// <returns></returns>
        public AudioClip LoadAudioClipWWW(string path)
        {
            var www = new WWW("file:///" + path);
            return www.GetAudioClip(true,true);
        }
        */
    }

    public class RegSceneManagement
    {
        /// <summary>
        /// �V�[�����ēǂݍ��݂���
        /// </summary>
        /// <param name="sceneName">�V�[����</param>
        /// <returns></returns>
        static public async UniTask ReloadScene(Scene scene)
        {
            if (SceneManager.sceneCount == 1)
            {
                await SceneManager.LoadSceneAsync(scene.buildIndex);
            }
            else
            {
                var unload = SceneManager.UnloadSceneAsync(scene);
                var load = SceneManager.LoadSceneAsync(scene.buildIndex, LoadSceneMode.Additive);

                await UniTask.WaitUntil(() => load.isDone && unload.isDone);
            }
        }

        static public async UniTask ExchangeScene(Scene currentScene, string afterSceneName)
        {
            if (SceneManager.sceneCount == 1)
            {
                await SceneManager.LoadSceneAsync(afterSceneName);
            }
            else
            {
                var unload = SceneManager.UnloadSceneAsync(currentScene);
                var load = SceneManager.LoadSceneAsync(afterSceneName, LoadSceneMode.Additive);

                await UniTask.WaitUntil(() => load.isDone && unload.isDone);
            }
        }
    }
}