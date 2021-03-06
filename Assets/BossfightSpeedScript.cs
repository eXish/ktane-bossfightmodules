using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class BossfightSpeedScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;
    public AudioSource music;

    public KMSelectable[] buttons;
    public GameObject[] pieceScalers;
    public GameObject core;
    public GameObject boss;

    static int bossCount = 1;
    static int moduleIdCounter = 1;
    int moduleId;
    int bossID;
    private bool moduleSolved;

    void Awake()
    {
        bossID = bossCount++;
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
        bomb.OnBombExploded += delegate () { bossCount = 1; if (bossID == 1) music.Stop(); };
    }

    void Start () {
        
    }

    void Update()
    {
        if (bossCount == 1 && moduleSolved)
        {
            music.Stop();
        }
    }

    void OnActivate()
    {
        if (bossID == 1)
        {
            music.Play();
        }
        StartCoroutine(rotCore());
        StartCoroutine(scaleCore());
        StartCoroutine(scalePieces());
        StartCoroutine(bossMoves());
    }

    void PressButton(KMSelectable pressed)
    {
        if(moduleSolved != true)
        {
            pressed.AddInteractionPunch(0.25f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            //insert code here
        }
    }

    private IEnumerator rotCore()
    {
        int rotation = 0;
        while (rotation != 90)
        {
            yield return new WaitForSeconds(0.007f);
            core.transform.Rotate(0.0f, 0.0f, 1f, Space.Self);
            rotation++;
        }
        StartCoroutine(rotCore());
    }

    private IEnumerator scaleCore()
    {
        int scale = 0;
        while (scale != 15)
        {
            core.transform.localScale -= new Vector3(0.001f, 0.001f, 0.001f);
            yield return new WaitForSeconds(0.0025f);
            scale++;
        }
        scale = 0;
        while (scale != 15)
        {
            core.transform.localScale += new Vector3(0.001f, 0.001f, 0.001f);
            yield return new WaitForSeconds(0.0025f);
            scale++;
        }
        yield return new WaitForSeconds(0.05f);
        scale = 0;
        while (scale != 15)
        {
            core.transform.localScale -= new Vector3(0.001f, 0.001f, 0.001f);
            yield return new WaitForSeconds(0.0025f);
            scale++;
        }
        scale = 0;
        while (scale != 15)
        {
            core.transform.localScale += new Vector3(0.001f, 0.001f, 0.001f);
            yield return new WaitForSeconds(0.0025f);
            scale++;
        }
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(scaleCore());
    }

    private IEnumerator bossMoves()
    {
        float t = 0f;
        float offset = 0.6f;
        while (t < 1f)
        {
            boss.transform.localPosition = Vector3.Lerp(new Vector3(0f, 0.07f, 0f), new Vector3(0f, 0.07f, 0.01f), t);
            if (t < 0.2f && offset < 1f)
            {
                offset += 0.02f;
            }
            t += Time.deltaTime * offset;
            yield return null;
        }
        yield return new WaitForSeconds(0.12f);
        t = 0f;
        while (t < 1f)
        {
            boss.transform.localPosition = Vector3.Lerp(new Vector3(0f, 0.07f, 0.01f), new Vector3(0f, 0.07f, 0f), t);
            if (t > 0.8f && offset > 0.6f)
            {
                offset -= 0.02f;
            }
            t += Time.deltaTime * offset;
            yield return null;
        }
        StartCoroutine(bossMoves());
    }

    private IEnumerator scalePieces()
    {
        int scale = 0;
        while (scale != 40)
        {
            pieceScalers[0].transform.localScale += new Vector3(0.01f, 0f, -0.01f);
            pieceScalers[1].transform.localScale += new Vector3(-0.01f, 0f, 0.01f);
            pieceScalers[2].transform.localScale += new Vector3(-0.001f, 0.001f, 0f);
            yield return new WaitForSeconds(0.025f);
            scale++;
        }
        scale = 0;
        while (scale != 40)
        {
            pieceScalers[0].transform.localScale += new Vector3(-0.01f, 0f, 0.01f);
            pieceScalers[1].transform.localScale += new Vector3(0.01f, 0f, -0.01f);
            pieceScalers[2].transform.localScale += new Vector3(0.001f, -0.001f, 0f);
            yield return new WaitForSeconds(0.025f);
            scale++;
        }
        StartCoroutine(scalePieces());
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} something [Does something]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*something\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Debug.Log("Did something");
            yield break;
        }
    }
}
