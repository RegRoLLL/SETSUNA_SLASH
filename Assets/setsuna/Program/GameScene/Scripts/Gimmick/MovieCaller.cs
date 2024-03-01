using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MovieCaller : SetsunaSlashScript
{
    [SerializeField] VideoClip movie;

    StageManager sm;
    Game_HubScript hub;

    void Start()
    {
        sm = GetComponentInParent<StageManager>();
    }

    void Update()
    {
        if (hub == null) hub = sm.hub;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == playerLayer)
        {
            PlayMovie();
        }
    }

    void PlayMovie()
    {
        hub.bgmAS.Stop();
        hub.moviePlayer.enabled = true;
        hub.moviePlayer.clip = movie;
        hub.moviePlayer.Play();

        Time.timeScale = 0;

        hub.moviePlayer.loopPointReached += (e) =>
        {
            Time.timeScale = 1;
            hub.moviePlayer.enabled = false;

            hub.gm.Back2Title();
        };

        transform.parent = null;
        hub.playingStage.SaveOverWrite(hub.playingStage.currentIndex);

        Destroy(gameObject);
    }
}
