using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Music : NetworkBehaviour
{
    private void Start()
    {
        if (IsServer) {
            StartCoroutine(PlayMusic());
        }
    }

    private IEnumerator PlayMusic()
    {
        yield return new WaitForSeconds(3f);
        GetComponent<AudioPlayer>().Play("Music");
    }
}
