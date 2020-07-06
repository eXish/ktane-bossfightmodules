using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class BossfightPowScript : MonoBehaviour
{

    public KMAudio audio;
    public KMBombInfo bomb;
    public AudioSource music;
    public KMSelectable[] pinks;
    public KMSelectable[] grays;
    public KMSelectable[] pivotsSels;

    public Renderer background;
    public Material[] backMats;
    public GameObject[] sets;
    public GameObject[] pinkObjs;
    public GameObject[] grayObjs;
    public GameObject[] fakeGrayObjs;
    public GameObject[] crystalObjs;
    public GameObject[] crystalRotators;
    public GameObject[] crystalOuterRotators;
    public GameObject[] finishedObjs;
    public Light[] finishedObjsLights;
    public GameObject[] pivots;
    public Material[] pivotcols;
    public Transform[] pinkCubesTrans;
    public Transform[] grayCubesTrans;
    public Transform[] fakeGrayCubesTrans;
    public GameObject crossImg;

    private IDictionary<string, object> tpAPI;

    private Vector3[] introPos = new Vector3[] { new Vector3(-0.07f, 0.03f, 0.03f), new Vector3(-0.03f, 0.03f, 0.07f), new Vector3(0.03f, 0.03f, 0.07f), new Vector3(0.07f, 0.03f, 0.03f), new Vector3(0.07f, 0.03f, -0.03f), new Vector3(0.03f, 0.03f, -0.07f), new Vector3(-0.03f, 0.03f, -0.07f), new Vector3(-0.07f, 0.03f, -0.03f), new Vector3(-0.07f, 0.03f, 0.03f) };
    private Vector3[] attackPos = new Vector3[] { new Vector3(-0.065f, 0.03f, 0.05f), new Vector3(-0.0325f, 0.03f, 0.05f), new Vector3(0f, 0.03f, 0.05f), new Vector3(0.0325f, 0.03f, 0.05f), new Vector3(0.065f, 0.03f, 0.05f), new Vector3(-0.065f, 0.03f, 0f), new Vector3(-0.0325f, 0.03f, 0f), new Vector3(0f, 0.03f, 0f), new Vector3(0.0325f, 0.03f, 0f), new Vector3(0.065f, 0.03f, 0f) };
    private Vector3[] pivotAttackPos = new Vector3[] { new Vector3(-0.05f, 0.021f, -0.03f), new Vector3(0f, 0.021f, -0.03f), new Vector3(0.05f, 0.021f, -0.03f) };
    private Vector3[] crossPos = new Vector3[] { new Vector3(-0.05f, -0.06f, 0.0f), new Vector3(0.0f, -0.06f, 0.0f), new Vector3(0.05f, -0.06f, 0.0f) };

    private List<float> circleSizes = new List<float>();
    private List<float> circleSpeeds = new List<float>();

    private Coroutine[] crystalCoroutines = new Coroutine[10];

    private bool[] pinkPresses = new bool[10];
    private bool[] grayPresses = new bool[10];

    private int pressCt = 0;
    private int pressAmt = -1;
    private int storedMins = -1;
    private bool haveToGrey = false;
    private bool firstPunch = false;

    private int backSel = -1;
    private int clockSel = -1;
    private int pivotpos = 0;
    private int randomTime = 0;
    private bool attacking = false;
    private int nextPivot = -1;

    private float finishMove;
    private float finishMove2;

    private bool finishAnim = false;
    private bool twitchMode = false;
    private bool notAnnounced = false;
    private bool notAnnounced2 = true;

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
        float scalar = transform.lossyScale.x;
        foreach (Light l in finishedObjsLights)
        {
            l.range *= scalar;
        }
        foreach (KMSelectable obj in pinks)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (KMSelectable obj in grays)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (KMSelectable obj in pivotsSels)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
        bomb.OnBombExploded += delegate () { bossCount = 1; if (bossID == 1) music.Stop(); };
    }

    void Start()
    {
        for (int i = 0; i < sets.Length; i++)
        {
            grayObjs[i].SetActive(false);
            if (i > 0)
            {
                sets[i].transform.localPosition = introPos[i - 1];
                sets[i].SetActive(false);
            }
        }
        for (int i = 0; i < 3; i++)
        {
            pivots[i].SetActive(false);
        }
        string[] backNames = new string[] { "Blue", "Grey" };
        string[] clockNames = new string[] { "Counter-Clockwise", "Clockwise" };
        backSel = UnityEngine.Random.Range(0, 2);
        background.material = backMats[backSel];
        clockSel = UnityEngine.Random.Range(0, 2);
        Debug.LogFormat("[Pow #{0}] The background objects are colored {1}!", moduleId, backNames[backSel]);
        Debug.LogFormat("[Pow #{0}] The boss is rotating {1} during the passive phase!", moduleId, clockNames[clockSel]);
        if (backSel == 0 && clockSel == 0)
        {
            pressAmt = 4;
        }
        else if (backSel == 1 && clockSel == 0)
        {
            pressAmt = 2;
        }
        else if (backSel == 0 && clockSel == 1)
        {
            pressAmt = 2;
        }
        else if (backSel == 1 && clockSel == 1)
        {
            pressAmt = 3;
        }
        Debug.LogFormat("[Pow #{0}] The destruction limit is {1}.", moduleId, pressAmt);
    }

    void Update()
    {
        if (bossCount == 1 && moduleSolved)
        {
            music.Stop();
        }
        if (!moduleSolved)
        {
            if (!pinkExists() && notAnnounced2)
            {
                notAnnounced2 = false;
                if (pressAmt != 1)
                {
                    pressAmt = 20;
                    Debug.LogFormat("[Pow #{0}] There is no longer any pink layers of pieces! Can destroy grey layers of pieces with no restriction!", moduleId);
                }
                else
                {
                    Debug.LogFormat("[Pow #{0}] There is no longer any pink layers of pieces, but minutes remaining is still a multiple of 5!", moduleId);
                }
            }
            if (storedMins != ((int)bomb.GetTime() / 60))
            {
                storedMins = (int)bomb.GetTime() / 60;
                Debug.LogFormat("[Pow #{0}] Currently {1} minutes remaining!", moduleId, storedMins);
                if (storedMins % 5 == 0 && storedMins != 0)
                {
                    notAnnounced = true;
                    pressAmt = 1;
                    Debug.LogFormat("[Pow #{0}] Minutes remaining is a multiple of 5! Destruction limit set to 1!", moduleId);
                }
                if (storedMins % 3 == 0 && storedMins != 0)
                {
                    if (backSel == 0 && clockSel == 0)
                    {
                        pressAmt = 4;
                    }
                    else if (backSel == 1 && clockSel == 0)
                    {
                        pressAmt = 2;
                    }
                    else if (backSel == 0 && clockSel == 1)
                    {
                        pressAmt = 2;
                    }
                    else if (backSel == 1 && clockSel == 1)
                    {
                        pressAmt = 3;
                    }
                    haveToGrey = true;
                    Debug.LogFormat("[Pow #{0}] Minutes remaining is a multiple of 3! Can only destroy grey layers of pieces!", moduleId);
                }
                if (storedMins % 5 != 0)
                {
                    if (!pinkExists())
                    {
                        pressAmt = 20;
                        if (notAnnounced)
                        {
                            notAnnounced = false;
                            Debug.LogFormat("[Pow #{0}] Minutes remaining is no longer a multiple of 5 and all pink layers of pieces are destroyed! Can destroy grey layers of pieces with no restriction!", moduleId);
                        }
                    }
                    else
                    {
                        if (backSel == 0 && clockSel == 0)
                        {
                            pressAmt = 4;
                        }
                        else if (backSel == 1 && clockSel == 0)
                        {
                            pressAmt = 2;
                        }
                        else if (backSel == 0 && clockSel == 1)
                        {
                            pressAmt = 2;
                        }
                        else if (backSel == 1 && clockSel == 1)
                        {
                            pressAmt = 3;
                        }
                    }
                }
                if (storedMins % 3 != 0)
                {
                    haveToGrey = false;
                }
            }
            if (storedMins == 0 && (int)bomb.GetTime() <= 30 && pressAmt != 20)
            {
                pressAmt = 20;
                Debug.LogFormat("[Pow #{0}] 30 seconds remaining! There is no longer a destruction limit!", moduleId);
            }
            else if (storedMins == 0 && (int)bomb.GetTime() > 30 && pressAmt != 20)
            {
                if (backSel == 0 && clockSel == 0)
                {
                    pressAmt = 4;
                }
                else if (backSel == 1 && clockSel == 0)
                {
                    pressAmt = 2;
                }
                else if (backSel == 0 && clockSel == 1)
                {
                    pressAmt = 2;
                }
                else if (backSel == 1 && clockSel == 1)
                {
                    pressAmt = 3;
                }
            }
        }
    }

    void OnActivate()
    {
        if (TwitchPlaysActive)
        {
            twitchMode = true;
            GameObject tpAPIGameObject = GameObject.Find("TwitchPlays_Info");
            if (tpAPIGameObject != null)
                tpAPI = tpAPIGameObject.GetComponent<IDictionary<string, object>>();
            else
                twitchMode = false;
        }
        if (bossID == 1)
        {
            music.Play();
        }
        StartCoroutine(randomMoves());
        StartCoroutine(startIntro());
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            if (pinks.Contains(pressed))
            {
                pressed.AddInteractionPunch();
                if (pressCt != pressAmt && !haveToGrey)
                {
                    audio.PlaySoundAtTransform("POWBREAK", pressed.transform);
                    if (!firstPunch)
                        firstPunch = true;
                    pressCt++;
                    pinkPresses[Array.IndexOf(pinks, pressed)] = true;
                    pinkObjs[Array.IndexOf(pinks, pressed)].SetActive(false);
                    grayObjs[Array.IndexOf(pinks, pressed)].SetActive(true);
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the pink layer of piece {1}, which is ok!", moduleId, Array.IndexOf(pinks, pressed) + 1);
                }
                else if (pressCt != pressAmt && haveToGrey)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the pink layer of piece {1}, which is not ok since grey layers can only be destroyed right now! Strike!", moduleId, Array.IndexOf(pinks, pressed) + 1);
                }
                else if (pressCt == pressAmt && !haveToGrey)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the pink layer of piece {1}, which is not ok since you have reached your destruction limit for this passive phase! Strike!", moduleId, Array.IndexOf(pinks, pressed) + 1);
                }
                else if (pressCt == pressAmt && haveToGrey)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the pink layer of piece {1}, which is not ok since grey layers can only be destroyed right now and you have reached your destruction limit for this passive phase! Strike!", moduleId, Array.IndexOf(pinks, pressed) + 1);
                }
            }
            else if (grays.Contains(pressed))
            {
                pressed.AddInteractionPunch();
                if (pinkExists() && !haveToGrey)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the grey layer of piece {1}, which is not ok since you have not destroyed all pink layers yet! Strike!", moduleId, Array.IndexOf(grays, pressed) + 1);
                }
                else if (pressCt != pressAmt)
                {
                    audio.PlaySoundAtTransform("POWBREAK", pressed.transform);
                    pressCt++;
                    grayPresses[Array.IndexOf(grays, pressed)] = true;
                    grayObjs[Array.IndexOf(grays, pressed)].SetActive(false);
                    finishedObjs[Array.IndexOf(grays, pressed)].SetActive(true);
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the grey layer of piece {1}, which is ok!", moduleId, Array.IndexOf(grays, pressed) + 1);
                }
                else if (pressCt == pressAmt && pressAmt != 1)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the grey layer of piece {1}, which is not ok since you have reached your destruction limit for this passive phase! Strike!", moduleId, Array.IndexOf(grays, pressed) + 1);
                }
                else if (pressCt == pressAmt && pressAmt == 1)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Pow #{0}] You tried to destroy the grey layer of piece {1}, which is not ok since you have reached your destruction limit for this minute! Strike!", moduleId, Array.IndexOf(grays, pressed) + 1);
                }
                if (allDestroyed())
                {
                    moduleSolved = true;
                    bossCount--;
                    StopAllCoroutines();
                    StartCoroutine(solveAnim());
                }
            }
            else if (pressed == pivotsSels[0] && pivotpos != 0)
            {
                pivotpos = 0;
                pivots[0].GetComponent<MeshRenderer>().material = pivotcols[1];
                pivots[1].GetComponent<MeshRenderer>().material = pivotcols[0];
                pivots[2].GetComponent<MeshRenderer>().material = pivotcols[0];
            }
            else if (pressed == pivotsSels[1] && pivotpos != 1)
            {
                pivotpos = 1;
                pivots[0].GetComponent<MeshRenderer>().material = pivotcols[0];
                pivots[1].GetComponent<MeshRenderer>().material = pivotcols[1];
                pivots[2].GetComponent<MeshRenderer>().material = pivotcols[0];
            }
            else if (pressed == pivotsSels[2] && pivotpos != 2)
            {
                pivotpos = 2;
                pivots[0].GetComponent<MeshRenderer>().material = pivotcols[0];
                pivots[1].GetComponent<MeshRenderer>().material = pivotcols[0];
                pivots[2].GetComponent<MeshRenderer>().material = pivotcols[1];
            }
        }
    }

    // Checks for all the cubes being destroyed
    private bool allDestroyed()
    {
        if (grayPresses.Contains(false) || pinkPresses.Contains(false))
        {
            return false;
        }
        return true;
    }

    // Checks if all sets (crystals) are in their attack positions
    private bool allReadyToAttack()
    {
        for (int i = 0; i < sets.Length; i++)
        {
            if (!(Vector3.Distance(sets[i].transform.localPosition, attackPos[i]) < 0.001f))
            {
                return false;
            }
        }
        return true;
    }

    // Checks if all sets (crystals) are back at the center position
    private bool allInitial()
    {
        for (int i = 0; i < sets.Length; i++)
        {
            if (!(Vector3.Distance(sets[i].transform.localPosition, new Vector3(0f, 0.03f, 0f)) < 0.001f))
            {
                return false;
            }
        }
        return true;
    }

    // Checks if the specified pivot is being attacking or not
    private bool isAttackingPivot(int set, int piv, bool att)
    {
        if (att)
        {
            if (!(Vector3.Distance(sets[set].transform.localPosition, pivotAttackPos[piv]) < 0.001f))
            {
                return false;
            }
            return true;
        }
        else
        {
            if (!(Vector3.Distance(sets[set].transform.localPosition, attackPos[set]) < 0.001f))
            {
                return false;
            }
            return true;
        }
    }

    // Gets the float offset needed for the snake-like movement of each diamond
    private float getFloat(int set)
    {
        return 0.3f * set;
    }

    // Checks for if grey layers are present
    private bool greyExists()
    {
        if (grayPresses.Contains(false))
        {
            return true;
        }
        return false;
    }

    // Checks for if pink layers are present
    private bool pinkExists()
    {
        if (pinkPresses.Contains(false))
        {
            return true;
        }
        return false;
    }

    // Checks for any finish objects to be active
    private bool finishExists()
    {
        for (int i = 0; i < sets.Length; i++)
        {
            if (finishedObjs[i].activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    // Deals with background movements
    private IEnumerator randomMoves()
    {
        float rando = UnityEngine.Random.Range(0.5f, 2f);
        float rando2 = UnityEngine.Random.Range(0.5f, 2f);
        Vector2 tempscale = background.material.GetTextureScale("_MainTex");
        if (tempscale.x < rando && tempscale.y < rando2)
        {
            while (tempscale.x < rando && tempscale.y < rando2)
            {
                tempscale = background.material.GetTextureScale("_MainTex");
                tempscale.x = tempscale.x + 0.00085f;
                tempscale.y = tempscale.y + 0.00085f;
                background.material.SetTextureScale("_MainTex", tempscale);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        else if (tempscale.x < rando && tempscale.y > rando2)
        {
            while (tempscale.x < rando && tempscale.y > rando2)
            {
                tempscale = background.material.GetTextureScale("_MainTex");
                tempscale.x = tempscale.x + 0.00085f;
                tempscale.y = tempscale.y - 0.00085f;
                background.material.SetTextureScale("_MainTex", tempscale);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        else if (tempscale.x > rando && tempscale.y < rando2)
        {
            while (tempscale.x > rando && tempscale.y < rando2)
            {
                tempscale = background.material.GetTextureScale("_MainTex");
                tempscale.x = tempscale.x - 0.00085f;
                tempscale.y = tempscale.y + 0.00085f;
                background.material.SetTextureScale("_MainTex", tempscale);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        else if (tempscale.x > rando && tempscale.y > rando2)
        {
            while (tempscale.x > rando && tempscale.y > rando2)
            {
                tempscale = background.material.GetTextureScale("_MainTex");
                tempscale.x = tempscale.x - 0.00085f;
                tempscale.y = tempscale.y - 0.00085f;
                background.material.SetTextureScale("_MainTex", tempscale);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        StartCoroutine(randomMoves());
    }

    // Intro sequence: Delays the cube rotations so they start one after another, plays intro anim, and then starts the boss (set) movement(s)
    private IEnumerator startIntro()
    {
        for (int i = 0; i < sets.Length; i++)
        {
            StartCoroutine(rotPCube(i));
            StartCoroutine(rotGCube(i));
            if (i > 0)
                StartCoroutine(introThrow(i));
            yield return new WaitForSeconds(0.1f);
        }
        while (!allInitial()) { yield return new WaitForSeconds(0.1f); }
        randomTime = UnityEngine.Random.Range(3750, 4751);
        //for debugging randomTime = UnityEngine.Random.Range(100, 501);
        for (int i = 0; i < sets.Length; i++)
        {
            circleSizes.Add(0f);
            if (clockSel == 1)
                circleSpeeds.Add(1.5f);
            else
                circleSpeeds.Add(-1.5f);
            StartCoroutine(setCircling(i));
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Moves each set from its intro position to the middle
    private IEnumerator introThrow(int num)
    {
        float t = 0f;
        sets[num].SetActive(true);
        while (!allInitial() && t < 2f)
        {
            sets[num].transform.localPosition = Vector3.Lerp(sets[num].transform.localPosition, new Vector3(0f, 0.03f, 0f), t);
            t += Time.deltaTime * 0.035f;
            yield return null;
        }
    }

    // Rotates the specified pink cube in a loop
    private IEnumerator rotPCube(int cube)
    {
        int rotation = 0;
        while (rotation != 90)
        {
            yield return new WaitForSecondsRealtime(0.007f);
            pinkCubesTrans[cube].Rotate(2f, 0.0f, 0.0f, Space.Self);
            rotation++;
        }
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(rotPCube(cube));
    }

    // Rotates the specified gray cube in a loop
    private IEnumerator rotGCube(int cube)
    {
        int rotation = 0;
        while (rotation != 90)
        {
            yield return new WaitForSecondsRealtime(0.007f);
            grayCubesTrans[cube].Rotate(-2f, 0.0f, 0.0f, Space.Self);
            fakeGrayCubesTrans[cube].Rotate(-2f, 0.0f, 0.0f, Space.Self);
            rotation++;
        }
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(rotGCube(cube));
    }

    // Rotates the specified crystal in a loop
    private IEnumerator rotCrystal(int cube)
    {
        int rotation = 0;
        while (rotation != 90)
        {
            yield return new WaitForSecondsRealtime(0.007f);
            crystalOuterRotators[cube].transform.Rotate(0.0f, 0.0f, -1f, Space.Self);
            rotation++;
        }
        yield return new WaitForSeconds(0.1f);
        crystalCoroutines[cube] = StartCoroutine(rotCrystal(cube));
    }

    // Start set circling
    private IEnumerator setCircling(int set)
    {
        int ct = 0;
        float offset = getFloat(set);
        if (clockSel == 0)
            offset = -offset;
        while (ct < randomTime || !firstPunch)
        {
            Vector3 newPos = new Vector3(Mathf.Sin(Time.time * circleSpeeds[set] - offset) * circleSizes[set], 0.03f, Mathf.Cos(Time.time * circleSpeeds[set] - offset) * circleSizes[set]);
            sets[set].transform.localPosition = newPos;
            if (circleSizes[set] < 0.07f)
                circleSizes[set] += 0.00005f;
            ct++;
            yield return new WaitForSecondsRealtime(0.0001f);
        }
        while (circleSizes[set] > 0f)
        {
            Vector3 newPos = new Vector3(Mathf.Sin(Time.time * circleSpeeds[set] - offset) * circleSizes[set], 0.03f, Mathf.Cos(Time.time * circleSpeeds[set] - offset) * circleSizes[set]);
            sets[set].transform.localPosition = newPos;
            circleSizes[set] -= 0.00005f;
            yield return new WaitForSecondsRealtime(0.0001f);
        }
        while (!allInitial()) { yield return null; }
        if (set == 0)
        {
            //for debugging randomTime = UnityEngine.Random.Range(100, 501);
            randomTime = UnityEngine.Random.Range(4000, 5001);
            for (int i = 0; i < sets.Length; i++)
            {
                if (pinkObjs[i].activeSelf)
                {
                    for (int j = 0; j < sets.Length; j++)
                    {
                        if (pinkObjs[j].activeSelf)
                        {
                            pinkObjs[j].SetActive(false);
                            crystalObjs[j].SetActive(true);
                        }
                        else if (grayObjs[j].activeSelf)
                        {
                            grayObjs[j].SetActive(false);
                            fakeGrayObjs[j].SetActive(true);
                        }
                    }
                    StartCoroutine(attackMode());
                    yield break;
                }
            }
        }
        if (!attacking)
            StartCoroutine(setCircling(set));
    }

    // Starts the bosses attack mode
    private IEnumerator attackMode()
    {
        attacking = true;
        audio.PlaySoundAtTransform("POWTRANSITION", transform);
        pivotpos = UnityEngine.Random.Range(0, 3);
        if (twitchMode && !solving)
        {
            tpAPI["ircConnectionSendMessage"] = "Module " + GetModuleCode() + " (Pow) has entered an attack phase! The green LED is LED " + (pivotpos + 1) + "!";
        }
        for (int i = 0; i < 3; i++)
        {
            if (pivotpos == i)
            {
                pivots[i].GetComponent<MeshRenderer>().material = pivotcols[1];
            }
            else
            {
                pivots[i].GetComponent<MeshRenderer>().material = pivotcols[0];
            }
            pivots[i].SetActive(true);
        }
        float t = 0f;
        while (!allReadyToAttack() && t < 2f)
        {
            pivots[0].transform.localPosition = Vector3.Lerp(new Vector3(-0.05f, 0.006f, -0.06f), new Vector3(-0.05f, 0.021f, -0.06f), t);
            pivots[1].transform.localPosition = Vector3.Lerp(new Vector3(0f, 0.006f, -0.06f), new Vector3(0f, 0.021f, -0.06f), t);
            pivots[2].transform.localPosition = Vector3.Lerp(new Vector3(0.05f, 0.006f, -0.06f), new Vector3(0.05f, 0.021f, -0.06f), t);
            for (int i = 0; i < sets.Length; i++)
            {
                sets[i].transform.localPosition = Vector3.Lerp(new Vector3(0f, 0.03f, 0f), attackPos[i], t);
                crystalRotators[i].transform.localEulerAngles = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(33.5f, 0f, 0f), t);
                t += Time.deltaTime * 0.62f;
                yield return null;
            }
        }
        for (int i = 0; i < sets.Length; i++)
        {
            if (crystalObjs[i].activeSelf)
                crystalCoroutines[i] = StartCoroutine(rotCrystal(i));
        }
        float attRed = 0f;
        bool firstFound = false;
        for (int i = 0; i < sets.Length; i++)
        {
            if (crystalObjs[i].activeSelf && !firstFound)
            {
                firstFound = true;
            }
            else if (crystalObjs[i].activeSelf && firstFound)
            {
                attRed -= 0.05f;
            }
        }
        for (int i = 0; i < sets.Length; i++)
        {
            if (crystalObjs[i].activeSelf)
            {
                nextPivot = UnityEngine.Random.Range(0, 3);
                crossImg.transform.localPosition = crossPos[nextPivot];
                crossImg.SetActive(true);
                if (twitchMode && !solving)
                {
                    tpAPI["ircConnectionSendMessage"] = "LED " + (nextPivot + 1) + " is about to be attacked on Module " + GetModuleCode() + " (Pow)!";
                    yield return new WaitForSeconds(7f);
                }
                audio.PlaySoundAtTransform("POWATTACK", transform);
                t = 0f;
                while (!isAttackingPivot(i, nextPivot, true) && t < 2f)
                {
                    sets[i].transform.localPosition = Vector3.Lerp(attackPos[i], pivotAttackPos[nextPivot], t);
                    t += Time.deltaTime * (1.6f + attRed);
                    yield return null;
                }
                if (nextPivot == pivotpos)
                {
                    Debug.LogFormat("[Pow #{0}] You were hit by a crystal during the boss' attack phase! Strike!", moduleId);
                    GetComponent<KMBombModule>().HandleStrike();
                }
                t = 0f;
                while (!isAttackingPivot(i, nextPivot, false) && t < 2f)
                {
                    sets[i].transform.localPosition = Vector3.Lerp(pivotAttackPos[nextPivot], attackPos[i], t);
                    t += Time.deltaTime * (1.6f + attRed);
                    yield return null;
                }
            }
        }
        for (int i = 0; i < sets.Length; i++)
        {
            if (crystalObjs[i].activeSelf)
            {
                crystalOuterRotators[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                StopCoroutine(crystalCoroutines[i]);
            }
        }
        crossImg.SetActive(false);
        nextPivot = -1;
        audio.PlaySoundAtTransform("POWTRANSITION", transform);
        if (twitchMode && !solving)
        {
            tpAPI["ircConnectionSendMessage"] = "Module " + GetModuleCode() + " (Pow) has exited the attack phase!";
        }
        t = 0f;
        while (!allInitial() && t < 2f)
        {
            for (int i = 0; i < sets.Length; i++)
            {
                pivots[0].transform.localPosition = Vector3.Lerp(new Vector3(-0.05f, 0.021f, -0.06f), new Vector3(-0.05f, 0.006f, -0.06f), t);
                pivots[1].transform.localPosition = Vector3.Lerp(new Vector3(0f, 0.021f, -0.06f), new Vector3(0f, 0.006f, -0.06f), t);
                pivots[2].transform.localPosition = Vector3.Lerp(new Vector3(0.05f, 0.021f, -0.06f), new Vector3(0.05f, 0.006f, -0.06f), t);
                sets[i].transform.localPosition = Vector3.Lerp(attackPos[i], new Vector3(0f, 0.03f, 0f), t);
                crystalRotators[i].transform.localEulerAngles = Vector3.Lerp(new Vector3(33.5f, 0f, 0f), new Vector3(0f, 0f, 0f), t);
                t += Time.deltaTime * 0.62f;
                yield return null;
            }
        }
        pressCt = 0;
        for (int i = 0; i < 3; i++)
        {
            pivots[i].SetActive(false);
        }
        for (int j = 0; j < sets.Length; j++)
        {
            if (crystalObjs[j].activeSelf)
            {
                pinkObjs[j].SetActive(true);
                crystalObjs[j].SetActive(false);
            }
            else if (fakeGrayObjs[j].activeSelf)
            {
                grayObjs[j].SetActive(true);
                fakeGrayObjs[j].SetActive(false);
            }
        }
        attacking = false;
        for (int i = 0; i < sets.Length; i++)
        {
            StartCoroutine(setCircling(i));
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Solving animation base
    private IEnumerator solveAnim()
    {
        audio.PlaySoundAtTransform("POWFINISH", transform);
        StartCoroutine(spread());
        float fadeInTime = 3.0f;
        Color origColor = background.material.color;
        for (float t = 0.01f; t < fadeInTime; t += Time.deltaTime)
        {
            background.material.color = Color.Lerp(origColor, Color.black, Mathf.Min(1, t / fadeInTime));
            yield return null;
        }
        finishAnim = true;
        GetComponent<KMBombModule>().HandlePass();
    }

    // Solving animation spread of pieces base
    private IEnumerator spread()
    {
        for (int i = 0; i < sets.Length; i++)
        {
            finishMove = UnityEngine.Random.Range(-0.075f, 0.075f);
            finishMove2 = UnityEngine.Random.Range(-0.075f, 0.075f);
            StartCoroutine(throwDir(i, finishMove, finishMove2));
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // Spreads a pieces in a specified direction
    private IEnumerator throwDir(int num, float x, float z)
    {
        float t = 0f;
        while (finishExists() && t < 2f)
        {
            if (sets[num].transform.localPosition.x > 0.075f || sets[num].transform.localPosition.x < -0.075f || sets[num].transform.localPosition.z > 0.075f || sets[num].transform.localPosition.z < -0.075f)
            {
                finishedObjs[num].SetActive(false);
            }
            Vector3 currentpos = sets[num].transform.localPosition;
            currentpos.x += x;
            currentpos.z += z;
            sets[num].transform.localPosition = Vector3.Lerp(sets[num].transform.localPosition, currentpos, t);
            t += Time.deltaTime * 0.009f;
            yield return null;
        }
    }

    //twitch plays
    bool TwitchPlaysActive;
    bool solving = false;

    // Deals with TP command handling
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} led <#> [Presses the specified LED when the boss is in an attack phase] | !{0} press <p1> (p2)... [Presses the piece(s) 'p1' (and optionally 'p2' or more) of the boss when it is not in an attack phase] | Valid pieces are 1-10 where 1 is the front and 10 is the back of the boss | Valid LEDs are 1-3 with 1 being leftmost and 3 being rightmost | On TP the module will announce in chat when the boss enters an attack phase and which LED it will attack next | Time between attacks in an attack phase are slightly longer on TP";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*led\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!attacking)
            {
                yield return "sendtochaterror The boss is not currently in an attack phase!";
                yield break;
            }
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp < 1 || temp > 3)
                    {
                        yield return "sendtochaterror The specified LED to press '" + parameters[1] + "' is out of range 1-3!";
                        yield break;
                    }
                    pivotsSels[temp - 1].OnInteract();
                }
                else
                {
                    yield return "sendtochaterror The specified LED to press '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the LED to press!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    int temp = 0;
                    if (!int.TryParse(parameters[i], out temp))
                    {
                        yield return "sendtochaterror The specified piece to press '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                    else if (temp < 1 || temp > 10)
                    {
                        yield return "sendtochaterror The specified piece to press '" + parameters[i] + "' is out of range 1-10!";
                        yield break;
                    }
                    else if (!pinkObjs[temp - 1].activeSelf && !grayObjs[temp - 1].activeSelf)
                    {
                        yield return "sendtochaterror This set of pieces cannot be pressed due to piece '" + parameters[i] + "' not having any layers left or the boss being in an attack phase!";
                        yield break;
                    }
                }
                for (int i = 1; i < parameters.Length; i++)
                {
                    int temp = int.Parse(parameters[i]);
                    if (pinkObjs[temp - 1].activeSelf)
                    {
                        pinks[temp - 1].OnInteract();
                    }
                    else if (grayObjs[temp - 1].activeSelf)
                    {
                        grays[temp - 1].OnInteract();
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp < 1 || temp > 10)
                    {
                        yield return "sendtochaterror The specified piece to press '" + parameters[1] + "' is out of range 1-10!";
                        yield break;
                    }
                    if (pinkObjs[temp - 1].activeSelf)
                    {
                        pinks[temp - 1].OnInteract();
                    }
                    else if (grayObjs[temp - 1].activeSelf)
                    {
                        grays[temp - 1].OnInteract();
                    }
                    else
                    {
                        yield return "sendtochaterror This piece cannot be pressed due to not having any layers left or the boss being in an attack phase!";
                    }
                }
                else
                {
                    yield return "sendtochaterror The specified piece to press '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the piece to press!";
            }
            yield break;
        }
    }

    // Deals with starting the TP force solve
    void TwitchHandleForcedSolve()
    {
        solving = true;
        StartCoroutine(SolveHandler());
    }

    // Actually deals with the TP force solve
    IEnumerator SolveHandler()
    {
        while (!finishAnim)
        {
            while (attacking)
            {
                if (nextPivot == pivotpos)
                {
                    int[] pressers = new int[] { 1, 2, 3 };
                    int press = UnityEngine.Random.Range(0, pressers.Length);
                    while (press == nextPivot)
                    {
                        press = UnityEngine.Random.Range(0, pressers.Length);
                    }
                    pivotsSels[press].OnInteract();
                }
                yield return new WaitForSeconds(0.1f);
            }
            int start = pressCt;
            for (int i = start; i < pressAmt; i++)
            {
                List<int> unpressed = new List<int>();
                if (!haveToGrey && pinkExists())
                {
                    for (int j = 0; j < sets.Length; j++)
                    {
                        if (pinkPresses[j] == false)
                        {
                            unpressed.Add(j);
                        }
                    }
                    pinks[unpressed[UnityEngine.Random.Range(0, unpressed.Count())]].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                else if (greyExists() && greyObjsActive())
                {
                    for (int j = 0; j < sets.Length; j++)
                    {
                        if (grayPresses[j] == false && !pinkObjs[j].activeSelf)
                        {
                            unpressed.Add(j);
                        }
                    }
                    grays[unpressed[UnityEngine.Random.Range(0, unpressed.Count())]].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield return null;
        }
    }

    // Checks if there are physically any grey objects active
    private bool greyObjsActive()
    {
        for (int i = 0; i < sets.Length; i++)
        {
            if (grayObjs[i].activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    // Gets the Twitch Plays ID for the module
    private string GetModuleCode()
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;
        foreach (Transform children in transform.parent)
        {
            var distance = (transform.position - children.position).magnitude;
            if (children.gameObject.name == "TwitchModule(Clone)" && (closest == null || distance < closestDistance))
            {
                closest = children;
                closestDistance = distance;
            }
        }

        return closest != null ? closest.Find("MultiDeckerUI").Find("IDText").GetComponent<UnityEngine.UI.Text>().text : null;
    }
}