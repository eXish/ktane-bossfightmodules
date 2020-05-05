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
    public GameObject core;

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
