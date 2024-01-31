using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class whiteElephantScript : MonoBehaviour {
    
    public KMBombInfo Bomb;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public TextMesh PresentText;
    public GameObject[] PresentTag;
    public GameObject[] DisplayTag;
    public GameObject[] ReceiverName;
    public GameObject[] NameTag;
    public TextMesh[] Names;
    public TextMesh[] IDs;
    public KMSelectable[] Presents;
    public KMSelectable[] Receiver;
    public KMSelectable WhiteElephantButton;
    public GameObject WhiteElephant;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private int correctPresent;
    private int time, rec, twitchy;
    public static MonoRandomWhiteElephant rnd;
    private int ruleSeed1, ruleSeed2, ruleSeed3, ruleSeed4; // One: Present and People, Two: People, Three: True Present Receiver, Four: Open Present
    private bool _isSolved;
    private bool selection;
    private bool elephant, selectPres, selectRec, openPres;
    private int[] tag = new int[3];
    private int[] name = new int[3];
    private string[] receiverName = new string[3];
    private string[] receivers = {"James","Robert","John","Michael","David","William","Richard","Joseph","Thomas","Christopher",
        "Charles","Daniel","Matthew","Anthony","Mark","Donald","Steven","Andrew","Paul","Joshua",
        "Mary","Patricia","Jennifer","Linda","Elizabeth","Barbara","Susan","Jessica","Sarah","Karen",
        "Lisa","Nancy","Betty","Sandra","Margaret","Ashley","Kimberly","Emily","Donna","Kathryn" };
    private string correctReceiver = "";
    private int[][] stage2 = {
        new int[3],
        new int[3],
        new int[3]
    };
    private int[] segments = new int[3];
    private bool isAnimating = false;
    string[] ids = { "", "", "" };

    private int[] edgework = { 0, 0, 0, 0, 0, 0, 0 };
    private int[] amount = { 0, 1 };
    private int[] vennColors = new int[8];
    private string[] colors = new string[8];
    private int[][] edgeworkConditions = {
        new int[2],
        new int[2],
        new int[2]
    };
    // Use this for initialization
    void Start () {
        moduleId = moduleIdCounter++;
        _isSolved = false;
        WhiteElephantButton.OnInteract += delegate { if(!isAnimating) activateModule(); return false; };
        Presents[0].OnInteract += delegate { if (!isAnimating) PressPresent(0); return false; };
        Presents[1].OnInteract += delegate { if (!isAnimating) PressPresent(1); return false; };
        Presents[2].OnInteract += delegate { if (!isAnimating) PressPresent(2); return false; };

        Presents[0].OnHighlight = delegate () { if (!isAnimating) ShowTag(0); };
        Presents[0].OnHighlightEnded = delegate () { if (!isAnimating) HideTag(0); };
        Presents[1].OnHighlight = delegate () { if (!isAnimating) ShowTag(1); };
        Presents[1].OnHighlightEnded = delegate () { if (!isAnimating) HideTag(1); };
        Presents[2].OnHighlight = delegate () { if (!isAnimating) ShowTag(2); };
        Presents[2].OnHighlightEnded = delegate () { if (!isAnimating) HideTag(2); };

        Receiver[0].OnInteract += delegate { if (!isAnimating) PressReceiver(0); return false; };
        Receiver[1].OnInteract += delegate { if (!isAnimating) PressReceiver(1); return false; };
        Receiver[2].OnInteract += delegate { if (!isAnimating) PressReceiver(2); return false; };

        Receiver[0].OnHighlight = delegate () { if (!isAnimating) ShowName(0); };
        Receiver[0].OnHighlightEnded = delegate () { if (!isAnimating) HideName(0); };
        Receiver[1].OnHighlight = delegate () { if (!isAnimating) ShowName(1); };
        Receiver[1].OnHighlightEnded = delegate () { if (!isAnimating) HideName(1); };
        Receiver[2].OnHighlight = delegate () { if (!isAnimating) ShowName(2); };
        Receiver[2].OnHighlightEnded = delegate () { if (!isAnimating) HideName(2); };

        beginModule();
	}

    void ShowTag(int i)
    {
        if (!openPres)
        {
            PresentTag[i * 4 + tag[i]].gameObject.SetActive(false);
            PresentText.gameObject.SetActive(true);
            if (segments[i] == 1000)
            {
                PresentText.text = "???";
            }
            else PresentText.text = segments[i].ToString();
            DisplayTag[tag[i]].SetActive(true);
        }
    }

    void HideTag(int i)
    {
        PresentTag[i * 4 + tag[i]].gameObject.SetActive(true);
        PresentText.gameObject.SetActive(false);
        DisplayTag[tag[i]].SetActive(false);
    }

    void ShowName(int i)
    {
        
        IDs[name[i]].text = ids[i];
        Names[name[i]].text = receiverName[i];
        ReceiverName[i * 4 + name[i]].gameObject.SetActive(false);
        NameTag[name[i]].SetActive(true);
        if (ids[i] == ruleSeed1.ToString())
        {
            IDs[name[i]].text = "?????????";
        }
    }

    void HideName(int i)
    {
        ReceiverName[i * 4 + name[i]].gameObject.SetActive(true);
        
        NameTag[name[i]].SetActive(false);
    }

    void PressPresent(int i)
    {
        if (!selection)
        {
            Audio.PlaySoundAtTransform("rap", transform);
            if (i == vennColors[correctPresent])
            {
                PresentTag[i * 4 + tag[i]].gameObject.SetActive(true);
                PresentText.gameObject.SetActive(false);
                DisplayTag[tag[i]].SetActive(false);
                isAnimating = true;
                StartCoroutine(presentsToReceivers());
            }
            else
            {
                WhiteElephantButton.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                WhiteElephantButton.gameObject.SetActive(true);
                BombModule.HandleStrike();
                Debug.LogFormat("[White Elephant #{0}] Strike! Reason: Incorrect Gift", moduleId);
                beginModule();
            }
        }
        else
        {
            Audio.PlaySoundAtTransform("oppenheimer", transform);
            int pressedTime = Mathf.FloorToInt(Bomb.GetTime() % 60) % 10;
            if (time == pressedTime)
            {
                PresentTag[i * 4 + tag[i]].gameObject.SetActive(true);
                PresentText.gameObject.SetActive(false);
                DisplayTag[tag[i]].SetActive(false);
                isAnimating = true;
                StartCoroutine(solveAnim(i));
            }
            else
            {
                WhiteElephantButton.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                WhiteElephantButton.gameObject.SetActive(true);
                Presents[0].gameObject.transform.localPosition = new Vector3(-0.0418f, 0.0278f, 0.0241f);
                Presents[0].gameObject.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
                Presents[1].gameObject.transform.localPosition = new Vector3(0.0044f, 0.0278f, 0.0463f);
                Presents[1].gameObject.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
                Presents[2].gameObject.transform.localPosition = new Vector3(0.0569f, 0.0278f, 0.024f);
                Presents[2].gameObject.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
                BombModule.HandleStrike();
                Debug.LogFormat("[White Elephant #{0}] Strike! Reason: Incorrect Time to Open Gift", moduleId);
                beginModule();
            }
        }
    }

    void PressReceiver(int i)
    {
        if (receiverName[i] == correctReceiver)
        {
            Audio.PlaySoundAtTransform("thankyou", transform);
            ReceiverName[i * 4 + name[i]].gameObject.SetActive(true);
            NameTag[name[i]].gameObject.SetActive(false);
            isAnimating = true;
            rec = i;
            StartCoroutine(receiversToOpen());
        }
        else
        {
            WhiteElephantButton.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            WhiteElephantButton.gameObject.SetActive(true);
            BombModule.HandleStrike();
            Debug.LogFormat("[White Elephant #{0}] Strike! Reason: Incorrect Gift Receiver", moduleId);
            beginModule();
        }
    }

    void beginModule()
    {
        twitchy = 0;
        elephant = true;
        selectPres = false;
        selectRec = false;
        openPres = false;
        selection = false;
        isAnimating = false;
        WhiteElephant.SetActive(false);
        PresentText.gameObject.SetActive(false);
        foreach (GameObject g in DisplayTag)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in NameTag)
        {
            g.SetActive(false);
        }
        foreach (KMSelectable k in Presents)
        {
            k.gameObject.SetActive(false);
        }
        foreach (KMSelectable k in Receiver)
        {
            k.gameObject.SetActive(false);
        }
        generateRuleSeed1();
        for (int i = 0; i < 3; i++)
        {
            tag[i] = Rnd.Range(0, tag.Length);
            for (int j = 0; j < 4; j++)
            {
                if (tag[i] != j)
                {
                    PresentTag[4*i+j].gameObject.SetActive(false);
                }
                else
                {
                    PresentTag[4 * i + j].gameObject.SetActive(true);
                }
            }
        }
        for (int i = 0; i < 3; i++)
        {
            name[i] = Rnd.Range(0, name.Length);
            for (int j = 0; j < 4; j++)
            {
                if (name[i] != j)
                {
                    ReceiverName[4 * i + j].gameObject.SetActive(false);
                }
                else
                {
                    ReceiverName[4 * i + j].gameObject.SetActive(true);
                }
            }
        }

    }

    void generateRuleSeed1()
    {
        for (int i = 0; i < 3; i++)
        {
            segments[i] = Rnd.Range(0, 1000);
        }
        ruleSeed1 = Int32.Parse(segments[0].ToString() + segments[1].ToString() + segments[2].ToString());
        Debug.LogFormat("[White Elephant #{0}] Red Present Segment: {1}", moduleId, segments[0]);
        Debug.LogFormat("[White Elephant #{0}] Gray/Grey Present Segment: {1}", moduleId, segments[1]);
        Debug.LogFormat("[White Elephant #{0}] Green Present Segment: {1}", moduleId, segments[2]);
        Debug.LogFormat("[White Elephant #{0}] Rule Seed #1: {1}", moduleId, ruleSeed1);
        rnd = new MonoRandomWhiteElephant(ruleSeed1);
        ids[0] = ruleSeed1.ToString();
        generatePresentRules();
    }

    void generatePresentRules()
    {
        string hi = "";
        for (int i = 0; i < 8; i++)
        {
            vennColors[i] = rnd.Next(3);
            switch (vennColors[i])
            {
                case 0: colors[i] = "Red"; hi += "r"; break;
                case 1: colors[i] = "Gray/Grey"; hi += "a"; break;
                case 2: colors[i] = "Green"; hi += "g"; break;
            }

        }
        edgework[0] = 2 * Bomb.GetBatteryHolderCount() - Bomb.GetBatteryCount();
        edgework[1] = 2 * (Bomb.GetBatteryCount() - Bomb.GetBatteryHolderCount());
        edgework[2] = Bomb.GetBatteryHolderCount();
        edgework[3] = Bomb.GetPortPlateCount();
        edgework[4] = Bomb.GetPortCount();
        edgework[5] = Bomb.GetOffIndicators().Count();
        edgework[6] = Bomb.GetOnIndicators().Count();
        string log1 = "";
        for (int edge = 0; edge < edgeworkConditions.Length; edge++)
        {
            edgeworkConditions[edge][0] = rnd.Next(edgework.Length);
            edgeworkConditions[edge][1] = amount[rnd.Next(2)];
            if(edge == 1)
            {
                while(edgeworkConditions[edge][0] == edgeworkConditions[edge - 1][0])
                {
                    edgeworkConditions[edge][0] = (edgeworkConditions[edge][0] + 1) % 7;
                }
            }
            if (edge == 2)
            {
                while (edgeworkConditions[edge][0] == edgeworkConditions[edge - 1][0] || edgeworkConditions[edge][0] == edgeworkConditions[edge - 2][0])
                {
                    edgeworkConditions[edge][0] = (edgeworkConditions[edge][0] + 1) % 7;
                }
            }
            switch (edgeworkConditions[edge][0])
            {
                case 0: log1 = "If the number of D Batteries is " + (edgeworkConditions[edge][1] == 0 ? "even" : "odd"); break;
                case 1: log1 = "If the number of AA Batteries is " + (edgeworkConditions[edge][1] == 0 ? "even" : "odd"); break;
                case 2: log1 = "If the number of Battery Holders is " + (edgeworkConditions[edge][1] == 0 ? "even" : "odd"); break;
                case 3: log1 = "If the number of Port Plates is " + (edgeworkConditions[edge][1] == 0 ? "even" : "odd"); break;
                case 4: log1 = "If the number of Ports is " + (edgeworkConditions[edge][1] == 0 ? "even" : "odd"); break;
                case 5: log1 = "If the number of Unlit Indicators is " + (edgeworkConditions[edge][1] == 0 ? "even" : "odd"); break;
                case 6: log1 = "If the number of Lit Indicators is " + (edgeworkConditions[edge][1] == 0 ? "even" : "odd"); break;
            }
            Debug.LogFormat("[White Elephant #{0}] Condition #{1}: {2}", moduleId, edge + 1, log1);

        }
        int pow = 1;
        correctPresent = 0;
        for (int i = 0; i < 3; i++)
        {
            
            
            if (edgeworkConditions[i][1] == 0)
            {
                if (edgework[edgeworkConditions[i][0]] % 2 == 0)
                {
                    correctPresent += pow;
                    Debug.LogFormat("[White Elephant #{0}] Iteration: {1}, Even, Added {2},", moduleId, i + 1, pow);
                }
            }
            if(edgeworkConditions[i][1] == 1)
            {
                if (edgework[edgeworkConditions[i][0]] % 2 == 1)
                {
                    correctPresent += pow;
                    Debug.LogFormat("[White Elephant #{0}] Iteration: {1}, Odd, Added {2},", moduleId, i + 1, pow);
                }
            }
            pow *= 2;
        }

        Debug.LogFormat("[White Elephant #{0}] Correct Present: {1}", moduleId, colors[correctPresent]);
    }

    void generateRuleSeed2()
    {
        string rseed = "";
        for (int i = 0; i < ruleSeed1.ToString().Length; i++)
        {
            rseed += Rnd.Range(1, 10);
        }
        ruleSeed2 = Int32.Parse(rseed);
        Debug.LogFormat("[White Elephant #{0}] Rule Seed #2: {1}", moduleId, ruleSeed2);
        ids[1] = ruleSeed2.ToString();

    }
    void generateRuleSeed3()
    {
        string rseed = "";
        for (int i = 0; i < ruleSeed1.ToString().Length; i++)
        {
            rseed += Rnd.Range(1, 10);
        }
        ruleSeed3 = Int32.Parse(rseed);
        Debug.LogFormat("[White Elephant #{0}] Rule Seed #3: {1}", moduleId, ruleSeed3);
        ids[2] = ruleSeed3.ToString();
    }

    void generateReceiverRules()
    {
        int counter = 0;
        generateReceivers(ruleSeed1,counter);
        counter++;
        generateRuleSeed2();
        generateReceivers(ruleSeed2, counter);
        counter++;
        generateRuleSeed3();
        generateReceivers(ruleSeed3, counter);
        string[] receivers2 = {"James","Robert","John","Michael","David","William","Richard","Joseph","Thomas","Christopher",
            "Charles","Daniel","Matthew","Anthony","Mark","Donald","Steven","Andrew","Paul","Joshua",
            "Mary","Patricia","Jennifer","Linda","Elizabeth","Barbara","Susan","Jessica","Sarah","Karen",
            "Lisa","Nancy","Betty","Sandra","Margaret","Ashley","Kimberly","Emily","Donna","Kathryn" };
        int temp = Rnd.Range(0, 3);
        for (int i = 0; i < receiverName.Length; i++) receiverName[i] = "";

        receiverName[temp] = receivers[stage2[Rnd.Range(0, 3)][Rnd.Range(0, 3)]];
        correctReceiver = receiverName[temp];
        for(int i = 0; i < stage2.Length; i++)
        {
            for(int j = 0; j < stage2[i].Length; j++)
            {
                receivers2[stage2[i][j]] = "";
            }
        }
        for (int i = 0; i < receiverName.Length; i++)
        {

            int tempR = 0;
            while (receiverName[i] == "")
            {
                tempR = Rnd.Range(0, 40);
                receiverName[i] = receivers2[tempR];
            }
            receivers2[tempR] = "";
            
        }
        int temp2 = Rnd.Range(1, 15);
        ids.Shuffle();
        receiverName.Shuffle();
        
        Debug.LogFormat("[White Elephant #{0}] Correct Gift Receiver: {1}", moduleId, correctReceiver);
        Debug.LogFormat("[White Elephant #{0}] Receivers in Order: {1}, {2}, {3}", moduleId, receiverName[0], receiverName[1], receiverName[2]);
    }

    void generateReceivers(int ruleSeed, int wawa)
    {
        rnd = new MonoRandomWhiteElephant(ruleSeed);
        for (int i = 0; i < 3; i++)
        {
            stage2[wawa][i] = rnd.Next(40);
        }

    }

    void generateOpenRules()
    {
        int[] stage3 = new int[3];
        rnd = new MonoRandomWhiteElephant(ruleSeed4);
        for (int i = 0; i < 3; i++)
        {
            int integer = rnd.Next(0, 10);
            stage3[i] = integer;
        }

        time = stage3[rec];

    }

    void activateModule()
    {
        isAnimating = true;
        StartCoroutine(whiteToPresents());
    }

    void PickPresent(int rand)
    {
        twitchy = rand;
        Presents[rand].gameObject.SetActive(true);
        Presents[rand].gameObject.transform.localPosition = new Vector3(0, 0.04f, 0);
        Presents[rand].gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    IEnumerator whiteToPresents()
    {
        elephant = false;
        yield return new WaitForSeconds(0.75f);
        Audio.PlaySoundAtTransform("slay", transform);
        for (int i = 1; i < 81; i++)
        {
            WhiteElephantButton.gameObject.transform.localScale = new Vector3(WhiteElephantButton.gameObject.transform.localScale.x - (0.01f/i), WhiteElephantButton.gameObject.transform.localScale.y - (0.01f / i), WhiteElephantButton.gameObject.transform.localScale.z - (0.01f / i));
            yield return new WaitForSeconds(0.01f);
        }
        WhiteElephantButton.gameObject.SetActive(false);
        foreach (KMSelectable present in Presents)
        {
            for (int i = 1; i < 23; i++)
            {
                present.gameObject.SetActive(true);
                present.gameObject.transform.localScale = new Vector3(present.gameObject.transform.localScale.x - (0.001f / i), present.gameObject.transform.localScale.y - (0.001f / i), present.gameObject.transform.localScale.z - (0.001f / i));
                present.gameObject.transform.localEulerAngles = new Vector3(present.gameObject.transform.localEulerAngles.x, present.gameObject.transform.localEulerAngles.y + 16.3636363636363636f, present.gameObject.transform.localEulerAngles.z);
                if (i < 12)
                    present.gameObject.transform.localPosition = new Vector3(present.gameObject.transform.localPosition.x, present.gameObject.transform.localPosition.y + 0.01f, present.gameObject.transform.localPosition.z);
                if (i > 11)
                    present.gameObject.transform.localPosition = new Vector3(present.gameObject.transform.localPosition.x, present.gameObject.transform.localPosition.y - 0.01f, present.gameObject.transform.localPosition.z);
                yield return new WaitForSeconds(0.01f);
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        isAnimating = false;
        selectPres = true;
        
    }

    IEnumerator presentsToReceivers()
    {
        selectPres = false;
        generateReceiverRules();
        yield return new WaitForSeconds(0.75f);
        foreach (KMSelectable present in Presents)
        {
            for (int i = 1; i < 23; i++)
            {
                present.gameObject.transform.localScale = new Vector3(present.gameObject.transform.localScale.x - (0.001f / i), present.gameObject.transform.localScale.y - (0.001f / i), present.gameObject.transform.localScale.z - (0.001f / i));
                present.gameObject.transform.localEulerAngles = new Vector3(present.gameObject.transform.localEulerAngles.x, present.gameObject.transform.localEulerAngles.y + 16.3636363636363636f, present.gameObject.transform.localEulerAngles.z);
                if (i < 12)
                    present.gameObject.transform.localPosition = new Vector3(present.gameObject.transform.localPosition.x, present.gameObject.transform.localPosition.y + 0.01f, present.gameObject.transform.localPosition.z);
                if (i > 11)
                    present.gameObject.transform.localPosition = new Vector3(present.gameObject.transform.localPosition.x, present.gameObject.transform.localPosition.y - 0.01f, present.gameObject.transform.localPosition.z);
                yield return new WaitForSeconds(0.01f);
            }
            present.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }
        foreach(KMSelectable receiver in Receiver)
        {
            receiver.gameObject.transform.localScale = new Vector3(0,0,0);
            receiver.gameObject.SetActive(true);
            for (int i = 1; i < 23; i++)
            {
                receiver.gameObject.transform.localScale = new Vector3(receiver.gameObject.transform.localScale.x + (0.00004f * i), receiver.gameObject.transform.localScale.y + (0.00004f * i), receiver.transform.localScale.z + (0.00004f * i));
                yield return new WaitForSeconds(0.01f);
            }
        }
        selectRec = true;
        isAnimating = false;
        
    }
    IEnumerator receiversToOpen()
    {
        selectRec = false;
        yield return new WaitForSeconds(0.75f);
        foreach (KMSelectable receiver in Receiver)
        {
            for (int i = 1; i < 23; i++)
            {
                receiver.gameObject.transform.localScale = new Vector3(receiver.gameObject.transform.localScale.x + (-0.00004f * i), receiver.gameObject.transform.localScale.y + (-0.00004f * i), receiver.transform.localScale.z + (-0.00004f * i));
                yield return new WaitForSeconds(0.01f);
            }
            receiver.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }
        int rand = Rnd.Range(0, 3);
        PickPresent(rand);
        Presents[rand].gameObject.SetActive(true);
        for (int i = 1; i < 23; i++)
        {
            Presents[rand].gameObject.transform.localScale = new Vector3(Presents[rand].gameObject.transform.localScale.x + 0.0025f, Presents[rand].gameObject.transform.localScale.y + 0.0025f, Presents[rand].transform.localScale.z + 0.0025f);
            Presents[rand].gameObject.transform.localEulerAngles = new Vector3(Presents[rand].gameObject.transform.localEulerAngles.x, Presents[rand].gameObject.transform.localEulerAngles.y + 16.3636363636363636f, Presents[rand].gameObject.transform.localEulerAngles.z);
            if (i < 12)
                Presents[rand].gameObject.transform.localPosition = new Vector3(Presents[rand].gameObject.transform.localPosition.x, Presents[rand].gameObject.transform.localPosition.y + 0.01f, Presents[rand].gameObject.transform.localPosition.z);
            if (i > 11)
                Presents[rand].gameObject.transform.localPosition = new Vector3(Presents[rand].gameObject.transform.localPosition.x, Presents[rand].gameObject.transform.localPosition.y - 0.01f, Presents[rand].gameObject.transform.localPosition.z);
            yield return new WaitForSeconds(0.01f);
        }
        ruleSeed4 = segments[rand];
        segments[rand] = 1000;
        selection = true;
        openPres = true;
        isAnimating = false;
        generateOpenRules();
        Debug.LogFormat("[White Elephant #{0}] Present Color: {1}", moduleId, colors[rand]);
        Debug.LogFormat("[White Elephant #{0}] Rule Seed #4: {1}", moduleId, ruleSeed4);
        Debug.LogFormat("[White Elephant #{0}] Time to Open: {1}", moduleId, time);
    }

    IEnumerator solveAnim(int bababa)
    {
        openPres = false;
        yield return new WaitForSecondsRealtime(0.75f);
        for (int i = 1; i < 23; i++)
        {
            Presents[bababa].gameObject.transform.localScale = new Vector3(Presents[bababa].gameObject.transform.localScale.x - 0.0025f, Presents[bababa].gameObject.transform.localScale.y - 0.0025f, Presents[bababa].transform.localScale.z - 0.0025f);
            Presents[bababa].gameObject.transform.localEulerAngles = new Vector3(Presents[bababa].gameObject.transform.localEulerAngles.x, Presents[bababa].gameObject.transform.localEulerAngles.y - 16.3636363636363636f, Presents[bababa].gameObject.transform.localEulerAngles.z);
            if (i < 12)
                Presents[bababa].gameObject.transform.localPosition = new Vector3(Presents[bababa].gameObject.transform.localPosition.x, Presents[bababa].gameObject.transform.localPosition.y + 0.01f, Presents[bababa].gameObject.transform.localPosition.z);
            if (i > 11)
                Presents[bababa].gameObject.transform.localPosition = new Vector3(Presents[bababa].gameObject.transform.localPosition.x, Presents[bababa].gameObject.transform.localPosition.y - 0.01f, Presents[bababa].gameObject.transform.localPosition.z);
            yield return new WaitForSeconds(0.01f);
        }
        Presents[bababa].gameObject.SetActive(false);
        WhiteElephant.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        WhiteElephant.SetActive(true);
        for (int i = 1; i < 81; i++)
        {
            WhiteElephant.gameObject.transform.localScale = new Vector3(WhiteElephant.transform.localScale.x + 0.00015f, WhiteElephant.transform.localScale.y + 0.00015f, WhiteElephant.transform.localScale.z + 0.00015f);
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSecondsRealtime(0.2f);
        Audio.PlaySoundAtTransform("slay", transform);
        BombModule.HandlePass();
    }

    // Update is called once per frame
    void Update () {
		
	}

    IEnumerator TwitchHandleForcedSolve()
    {
        if(elephant) WhiteElephantButton.OnInteract();
        yield return new WaitForSecondsRealtime(0.5f);
        while (isAnimating)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        switch (vennColors[correctPresent])
        {
            case 0:
                Presents[0].OnInteract(); break;
            case 1:
                Presents[1].OnInteract(); break;
            case 2:
                Presents[2].OnInteract(); break;
        }
        yield return new WaitForSecondsRealtime(0.5f);
        while (isAnimating)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        switch (Array.IndexOf(receiverName,correctReceiver))
        {
            case 0:
                Receiver[0].OnInteract(); break;
            case 1:
                Receiver[1].OnInteract(); break;
            case 2:
                Receiver[2].OnInteract(); break;
        }
        yield return new WaitForSecondsRealtime(0.5f);
        while (isAnimating)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        while (!(Mathf.FloorToInt(Bomb.GetTime() % 60) % 10 == time))
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.5f);
        Presents[twitchy].OnInteract();
    }

    private string TwitchHelpMessage = "!{0} elephant to press the white elephant. !{0} press <l, m, r> to press left, middle, or right present and receivers. !{0} tags to cycle between the present tags and receiver nametags. !{0} slowcycle to slowly cycle between the present tags and receiver nametags. !{0} open # to open your gift at the specific time (replace # with a digit). ";

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (!isAnimating)
        {
            if (parameters[0].EqualsIgnoreCase("elephant"))
            {
                if (elephant)
                {

                    yield return null;
                    WhiteElephantButton.OnInteract();
                    yield break;
                }
                else
                {
                    yield return "sendtochaterror Is the elephant in the room with us?";
                    yield break;
                }
            }
            else if (parameters[0].EqualsIgnoreCase("tags"))
            {
                if (selectPres)
                {
                    for(int i = 0; i < 3; i++)
                    {
                        yield return null;
                        ShowTag(i);
                        yield return new WaitForSecondsRealtime(1.0f);
                        HideTag(i);
                        yield return new WaitForSecondsRealtime(0.25f);
                    }
                    yield break;
                }
                else if (selectRec)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        yield return null;
                        ShowName(i);
                        yield return new WaitForSecondsRealtime(1.0f);
                        HideName(i);
                        yield return new WaitForSecondsRealtime(0.25f);
                    }
                    yield break;
                }
                else
                {
                    yield return "sendtochaterror You cannot cycle any tags right now!";
                    yield break;
                }
            }
            else if (parameters[0].EqualsIgnoreCase("slowcycle"))
            {
                if (selectPres)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        yield return null;
                        ShowTag(i);
                        yield return new WaitForSecondsRealtime(3.0f);
                        HideTag(i);
                        yield return new WaitForSecondsRealtime(0.25f);
                    }
                    yield break;
                }
                else if (selectRec)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        yield return null;
                        ShowName(i);
                        yield return new WaitForSecondsRealtime(3.0f);
                        HideName(i);
                        yield return new WaitForSecondsRealtime(0.25f);
                    }
                    yield break;
                }
                else
                {
                    yield return "sendtochaterror You cannot cycle any tags right now!";
                    yield break;
                }
            }
            else if (parameters[0].EqualsIgnoreCase("press"))
            {
                if (parameters[1].EqualsIgnoreCase("l") || parameters[1].EqualsIgnoreCase("left"))
                {
                    if (selectPres)
                    {
                        yield return null;
                        Presents[0].OnInteract();
                        yield break;
                    }
                    else if (selectRec)
                    {
                        yield return null;
                        Receiver[0].OnInteract();
                        yield break;
                    }
                    else
                    {
                        yield return "sendtochaterror You cannot press the left, middle, or right of anything here!";
                        yield break;
                    }
                }
                else if (parameters[1].EqualsIgnoreCase("m") || parameters[1].EqualsIgnoreCase("middle"))
                {
                    if (selectPres)
                    {
                        yield return null;
                        Presents[1].OnInteract();
                        yield break;
                    }
                    else if (selectRec)
                    {
                        yield return null;
                        Receiver[1].OnInteract();
                        yield break;
                    }
                    else
                    {
                        yield return "sendtochaterror You cannot press the left, middle, or right of anything here!";
                        yield break;
                    }
                }
                else if (parameters[1].EqualsIgnoreCase("r") || parameters[1].EqualsIgnoreCase("right"))
                {
                    if (selectPres)
                    {
                        yield return null;
                        Presents[2].OnInteract();
                        yield break;
                    }
                    else if (selectRec)
                    {
                        yield return null;
                        Receiver[2].OnInteract();
                        yield break;
                    }
                    else
                    {
                        yield return "sendtochaterror You cannot press the left, middle, or right of anything here!";
                        yield break;
                    }
                }
                else
                {
                    yield return "sendtochaterror What are you pressing??";
                    yield break;
                    
                }
            }
            else if (parameters[0].EqualsIgnoreCase("open"))
            {
                if (openPres)
                {
                    int pressed = 0;

                    if (int.TryParse(parameters[1], out pressed))
                    {
                        while (!(pressed == Mathf.FloorToInt(Bomb.GetTime() % 60) % 10))
                        {
                            yield return null;
                        }
                        yield return null;
                        Presents[twitchy].OnInteract();
                        yield break;
                    }
                }

            }
        }
        else
        {
            yield return "sendtochaterror You cannot send commands during module animation!";
            yield break;
        }
        yield return null;
        
    }
}

public class MonoRandomWhiteElephant
{

    /// <summary>Initializes a new instance of the <see cref="T:System.Random" /> class, using the specified seed value.</summary>
    /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence. If a negative number is specified, the absolute value of the number is used. </param>
    /// <exception cref="T:System.OverflowException">
    ///   <paramref name="seed" /> is <see cref="F:System.Int32.MinValue" />, which causes an overflow when its absolute value is calculated. </exception>
    public MonoRandomWhiteElephant(int seed)
    {
        Seed = seed;
        var num = 161803398 - Math.Abs(seed);
        _seedArray[55] = num;
        var num2 = 1;
        for (var i = 1; i < 55; i++)
        {
            var num3 = 21 * i % 55;
            _seedArray[num3] = num2;
            num2 = num - num2;
            if (num2 < 0)
            {
                num2 += int.MaxValue;
            }
            num = _seedArray[num3];
        }
        for (var j = 1; j < 5; j++)
        {
            for (var k = 1; k < 56; k++)
            {
                _seedArray[k] -= _seedArray[1 + (k + 30) % 55];
                if (_seedArray[k] < 0)
                {
                    _seedArray[k] += int.MaxValue;
                }
            }
        }
        _inext = 0;
        _inextp = 31;
    }

    /// <summary>Returns a random number between 0.0 and 1.0.</summary>
    /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
    protected virtual double Sample()
    {
        if (++_inext >= 56)
        {
            _inext = 1;
        }
        if (++_inextp >= 56)
        {
            _inextp = 1;
        }
        var num = _seedArray[_inext] - _seedArray[_inextp];
        if (num < 0)
        {
            num += int.MaxValue;
        }
        _seedArray[_inext] = num;
        return (double)num * 4.6566128752457969E-10;
    }

    public T ShuffleFisherYates<T>(T list) where T : IList
    {
        // Brings an array into random order using the Fisher-Yates shuffle.
        // This is an inplace algorithm, i.e. the input array is modified.
        var i = list.Count;
        while (i > 1)
        {
            var index = Next(0, i);
            i--;
            var value = list[index];
            list[index] = list[i];
            list[i] = value;
        }
        return list;
    }

    /// <summary>Returns a nonnegative random number.</summary>
    /// <returns>A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue" />.</returns>
    /// <filterpriority>1</filterpriority>
    public virtual int Next()
    {
        return (int)(Sample() * 2147483647.0);
    }

    /// <summary>Returns a nonnegative random number less than the specified maximum.</summary>
    /// <returns>A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue" />; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals zero, <paramref name="maxValue" /> is returned.</returns>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue" /> must be greater than or equal to zero. </param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="maxValue" /> is less than zero. </exception>
    /// <filterpriority>1</filterpriority>
    public virtual int Next(int maxValue)
    {
        if (maxValue < 0)
        {
            throw new ArgumentOutOfRangeException("maxValue");
        }
        return (int)(Sample() * (double)maxValue);
    }

    /// <summary>Returns a random number within a specified range.</summary>
    /// <returns>A 32-bit signed integer greater than or equal to <paramref name="minValue" /> and less than <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />. If <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.</returns>
    /// <param name="minValue">The inclusive lower bound of the random number returned. </param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />. </param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="minValue" /> is greater than <paramref name="maxValue" />. </exception>
    /// <filterpriority>1</filterpriority>
    public virtual int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException("minValue");
        }
        var num = (uint)(maxValue - minValue);
        if (num <= 1u)
        {
            return minValue;
        }
        return (int)((ulong)((uint)(Sample() * num)) + (ulong)((long)minValue));
    }

    /// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
    /// <param name="buffer">An array of bytes to contain random numbers. </param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="buffer" /> is null. </exception>
    /// <filterpriority>1</filterpriority>
    public virtual void NextBytes(byte[] buffer)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(Sample() * 256.0);
        }
    }

    /// <summary>Returns a random number between 0.0 and 1.0.</summary>
    /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
    /// <filterpriority>1</filterpriority>
    public virtual double NextDouble()
    {
        return Sample();
    }

    public int Seed
    {
        get;
        private set;
    }

    public int _inext;
    public int _inextp;
    public readonly int[] _seedArray = new int[56];
}
