using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Analytics;

public class backdoorKeypadScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMSelectable[] TwoXTwo;
    public TextMesh[] TwoXTwoText;
    public SpriteRenderer[] TwoXTwoSprite;
    public KMSelectable[] FourXFour;
    public TextMesh[] FourXFourText;
    public SpriteRenderer[] FourXFourSprite;
    public Sprite[] Labels;

    private bool stage1, completeStage1, stage2;
    private bool update1, update2, solveUpdate;
    private int stage1Counter;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool _isSolved = false;
    private int ruleSeed1, ruleSeed2, keypadCounter;
    private string RuleSeed;
    private MonoRandomBackdoorKeypad rnd;
    private int column;
    private int[] keyPos = new int[4];
    private int[] keyLab = new int[4];
    private char[] charList = { '©', '★', '☆', 'ټ', 'Җ', 'Ω', 'Ѭ', 'ѽ', 'ϗ', 'ϫ', 'Ϭ', 'Ϟ', 'Ѧ', 'æ', 'Ԇ', 'Ӭ', '҈', 'Ҋ', 'Ѯ', '¿', '¶', 'Ͼ', 'Ͽ', 'Ψ', 'Ѫ', 'Ҩ', '҂', 'Ϙ', 'ζ', 'ƛ', 'Ѣ' };
    private char[] charList2 = { '©', '★', '☆', 'ټ', 'Җ', 'Ω', 'Ѭ', 'ѽ', 'ϗ', 'ϫ', 'Ϭ', 'Ϟ', 'Ѧ', 'æ', 'Ԇ', 'Ӭ', '҈', 'Ҋ', 'Ѯ', '¿', '¶', 'Ͼ', 'Ͽ', 'Ψ', 'Ѫ', 'Ҩ', '҂', 'Ϙ', 'ζ', 'ƛ', 'Ѣ' };
    private int[][] resultLists = new int[][]
    {
        new int[] { 27, 12, 29, 11, 6, 8, 22},
        new int[] {15, 27, 22, 25, 2, 8, 19 },
        new int[] {0, 7, 25, 4, 14, 29, 2 },
        new int[] {10, 20, 30, 6, 4, 19, 3 },
        new int[] {23, 3, 30, 21, 20, 18, 1 },
        new int[] {10, 15, 26, 13, 23, 17, 5 }
    };
    private int[][] resultLists2 = new int[][]
    {
        new int[] { 27, 12, 29, 11, 6, 8, 22},
        new int[] {15, 27, 22, 25, 2, 8, 19 },
        new int[] {0, 7, 25, 4, 14, 29, 2 },
        new int[] {10, 20, 30, 6, 4, 19, 3 },
        new int[] {23, 3, 30, 21, 20, 18, 1 },
        new int[] {10, 15, 26, 13, 23, 17, 5 }
    };
    private int[] secondStageList = new int[16];

    // Use this for initialization
    void Start()
    {
        update1 = false;
        update2 = false;
        solveUpdate = false;
        moduleId = moduleIdCounter++;
        _isSolved = false;
        foreach (var button in TwoXTwo)
        {
            button.OnInteract += delegate ()
            {
                PressButtonStage1(button);
                return false;
            };
        }
        foreach (var button in FourXFour)
        {
            button.OnInteract += delegate ()
            {
                PressButtonStage2(button);
                return false;
            };
        }
        beginModule();
    }

    void beginModule()
    {
        foreach (var button in FourXFour)
        {
            button.gameObject.SetActive(false);
        }
        for(int i = 0; i < 4; i++)
        {
            TwoXTwoText[i].text = "";
            TwoXTwoSprite[i].gameObject.SetActive(false);
        }
        RuleSeed = "";
        generateRule1();
        for (int i = 0; i < 4; i++)
        {
            TwoXTwoText[i].text = ruleSeed1.ToString()[i].ToString();
        }
        
        
    }

    void PressButtonStage1(KMSelectable button)
    {
        if (!stage1 && !completeStage1)
        {
            if (button.GetComponentInChildren<TextMesh>().color != Color.white)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                button.AddInteractionPunch(0.5f);
                button.GetComponentInChildren<TextMesh>().color = Color.white;
                keypadCounter++;
            }
            else
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                button.AddInteractionPunch(0.5f);
            }
            if (keypadCounter == 4)
            {
                keypadCounter = 0;
                stage1 = true;
                generateStage1();
                Audio.PlaySoundAtTransform("ButtonPress", transform);

            }
        }
        else if (stage1 && !completeStage1)
        {  
            if (button.GetComponentInChildren<TextMesh>().color != Color.white)
            {
                if (button.GetComponentInChildren<TextMesh>().text == charList[resultLists[column][keyLab[Array.IndexOf(keyPos, stage1Counter)]]].ToString())
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                    button.AddInteractionPunch(0.5f);
                    button.GetComponentInChildren<TextMesh>().color = Color.white;
                    button.GetComponentInChildren<SpriteRenderer>().color = Color.white;
                    keypadCounter++;
                    stage1Counter++;
                }
                else
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                    button.AddInteractionPunch(0.5f);
                    button.GetComponentInChildren<TextMesh>().color = Color.red;
                    button.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                    BombModule.HandleStrike();

                }
                if (keypadCounter == 4)
                {
                    keypadCounter = 0;
                    completeStage1 = true;
                    Audio.PlaySoundAtTransform("ButtonPress", transform);
                    generateRule2();
                    for (int i = 0; i < 4; i++)
                    {
                        TwoXTwoText[Array.IndexOf(keyPos, i)].text = ruleSeed2.ToString()[i+4].ToString();
                        TwoXTwoText[i].gameObject.transform.localPosition = new Vector3(0f, 19.1f, 9.1f);
                        TwoXTwoSprite[i].gameObject.SetActive(false);
                    }
                    

                }
            }
            else
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                button.AddInteractionPunch(0.5f);
            }
        }
        else if (completeStage1)
        {
            if (button.GetComponentInChildren<TextMesh>().color != Color.white)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                button.AddInteractionPunch(0.5f);
                button.GetComponentInChildren<TextMesh>().color = Color.white;
                keypadCounter++;
            }
            else
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                button.AddInteractionPunch(0.5f);
            }
            if (keypadCounter == 4)
            {
                keypadCounter = 0;
                stage2 = true;
                Audio.PlaySoundAtTransform("Woosh", transform);
                generateStage2();

            }
        }

    }

    void PressButtonStage2(KMSelectable button)
    {
        if (!_isSolved)
        {
            if (button.GetComponentInChildren<TextMesh>().color != Color.white)
            {
                bool hi = false;
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                button.AddInteractionPunch(0.5f);
                for(int i = 0; i < 4; i++)
                {
                    if (button.GetComponentInChildren<TextMesh>().text == charList2[secondStageList[i]].ToString())
                    {
                        hi = true;
                    }
                }
                if (!hi)
                {
                    button.GetComponentInChildren<TextMesh>().color = Color.red;
                    button.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                    BombModule.HandleStrike();
                }
                else
                {
                    button.GetComponentInChildren<TextMesh>().color = Color.white;
                    button.GetComponentInChildren<SpriteRenderer>().color = Color.white;
                    keypadCounter++;
                }
            }
            
            if (keypadCounter == 4)
            {
                keypadCounter = 0;
                _isSolved = true;
                BombModule.HandlePass();

            }
        }
        else
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            button.AddInteractionPunch(0.5f);
        }
        
    }

    void generateRule1()
    {
        for(int i = 0; i < 4; i++)
        {
            RuleSeed += Rnd.Range(1, 10).ToString();
        }
        ruleSeed1 = (int)long.Parse(RuleSeed);
        RuleSeed = ruleSeed1.ToString();
        Debug.LogFormat("[Backdoor Keypad #{0}] Rule Seed #1: {1}", moduleId, ruleSeed1);
        rnd = new MonoRandomBackdoorKeypad(ruleSeed1);
        for (int i = 0; i < resultLists.Length; i++)
        {
            modifyRules(resultLists[i]);
        }

    }

    void generateRule2()
    {
        for (int i = 0; i < 4; i++)
        {
            TwoXTwoText[i].color = Color.black;
        }
        RuleSeed = ruleSeed1.ToString();
        for (int i = 0; i < 4; i++)
        {
            RuleSeed += Rnd.Range(1, 10).ToString();
        }
        ruleSeed2 = (int)long.Parse(RuleSeed);
        RuleSeed = ruleSeed2.ToString();
        Debug.LogFormat("[Backdoor Keypad #{0}] Rule Seed #2: {1}", moduleId, ruleSeed2);
        rnd = new MonoRandomBackdoorKeypad(ruleSeed2);
        for (int i = 0; i < resultLists.Length; i++)
        {
            modifyRules(resultLists2[i]);
        }
        
    }

    void modifyRules(int[] resultLists)
    {
        for (var i = 0; i < resultLists.Length; i++)
        {
            var value = resultLists[i];
            var index = rnd.Next(0, resultLists.Length);
            resultLists[i] = resultLists[index];
            resultLists[index] = value;
        }
    }

    void generateStage1()
    {
        for(int i = 0; i < 4; i++)
        {
            TwoXTwoText[i].color = Color.black;
        }
        column = Rnd.Range(0,6);
        int[] temp = new int[4];
        temp[0] = Rnd.Range(0, 7);
        temp[1] = Rnd.Range(0, 7);
        temp[2] = Rnd.Range(0, 7);
        temp[3] = Rnd.Range(0, 7);
        while (temp[1] == temp[0]) temp[1] = (temp[1] + 1) % 7;
        while (temp[2] == temp[0] || temp[2] == temp[1]) temp[2] = (temp[2] + 1) % 7;
        while (temp[3] == temp[0] || temp[3] == temp[1] || temp[3] == temp[2]) temp[3] = (temp[3] + 1) % 7;
        int[] things = { 0, 1, 2, 3 };
        int c = 0;
        for (int j = 0; j < 7; j++)
        {
            if (j == temp[0])
            {
                keyPos[Array.IndexOf(temp, j)] = c;
                c++;
            }
            if (j == temp[1])
            {
                keyPos[Array.IndexOf(temp, j)] = c;
                c++;
            }
            if (j == temp[2])
            {
                keyPos[Array.IndexOf(temp, j)] = c;
                c++;
            }
            if (j == temp[3])
            {
                keyPos[Array.IndexOf(temp, j)] = c;
                c++;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            Debug.LogFormat("{0}", temp[i]);
        }
        Debug.LogFormat("[Backdoor Keypad #{0}] Column in Manual: {1}", moduleId, column+1);
        string log = "";
        string pos = "";
        for (int i = 0; i < 4; i++)
        {
            keyLab[i] = temp[i];
            TwoXTwoText[i].text = charList[resultLists[column][keyLab[i]]].ToString();
            TwoXTwoSprite[i].sprite = Labels[resultLists[column][keyLab[i]]];
            TwoXTwoText[i].gameObject.transform.localPosition = new Vector3(0f, 0f, 9.1f);
            TwoXTwoSprite[i].gameObject.SetActive(true);
        }
        string sup = "";
        for(int i = 0; i < 7; i++)
        {
            sup += charList[resultLists[column][i]] + " ";
        }
        Debug.LogFormat("[Backdoor Keypad #{0}] Column {2}: {1}", moduleId, sup, column+1);
        for (int i = 0; i < 4; i++)
        {
            log += charList[resultLists[column][keyLab[Array.IndexOf(keyPos, i)]]].ToString() + " ";
            switch (Array.IndexOf(keyPos, i))
            {
                case 0:
                    pos += "TL";
                    break;
                case 1:
                    pos += "TR";
                    break;
                case 2:
                    pos += "BL";
                    break;
                case 3:
                    pos += "BR";
                    break;
            }
            if(i != 3) pos += ", ";
        }
        Debug.LogFormat("[Backdoor Keypad #{0}] Symbol Order: {1}", moduleId, log);
        Debug.LogFormat("[Backdoor Keypad #{0}] Position Order: {1}", moduleId, pos);
    }

    void generateStage2()
    {
        foreach (var button in FourXFour)
        {
            button.gameObject.SetActive(true);
        }
        foreach (var button in TwoXTwo)
        {
            button.gameObject.SetActive(false);
        }
        for (int i = 0; i < 16; i++)
        {
            FourXFourText[i].color = Color.black;
        }
        
        for(int i = 0; i < 4; i++)
        {
            secondStageList[i] = resultLists2[column][keyLab[i]];
            charList[secondStageList[i]] = ' ';
        }
        string log = "";
        for (int i = 0; i < 4; i++)
        {
            log += charList2[secondStageList[i]] + " ";
        }
        for (int i = 4; i < 16; i++)
        {
            int temp = Rnd.Range(0, charList.Length);
            while (charList[temp] == ' ') 
            {
                temp = Rnd.Range(0, charList.Length);
            }
            secondStageList[i] = temp;
            charList[temp] = ' ';
        }
        
        int r  = Rnd.Range(1, 16);
        for (int i = 0; i < r; i++)
        {
            int j = Rnd.Range(0, 16);
            int k = Rnd.Range(0, 16);
            while(k == j) k = Rnd.Range(0, 16);
            
            KMSelectable temp = FourXFour[j];
            FourXFour[j] = FourXFour[k];
            FourXFour[k] = temp;
            TextMesh temp2 = FourXFourText[j];
            FourXFourText[j] = FourXFourText[k];
            FourXFourText[k] = temp2;
            SpriteRenderer temp3 = FourXFourSprite[j];
            FourXFourSprite[j] = FourXFourSprite[k];
            FourXFourSprite[k] = temp3;

        }
        for (int i = 0; i < 16; i++)
        {
            FourXFourText[i].text = charList2[secondStageList[i]].ToString();
            FourXFourSprite[i].sprite = Labels[secondStageList[i]];
            FourXFourText[i].gameObject.transform.localPosition = new Vector3(0f, 0f, 9.1f);
        }
        
        Debug.LogFormat("[Backdoor Keypad #{0}] Labels to Press: {1}", moduleId, log);

    }

    // Update is called once per frame
    void Update()
    {
        if (stage1 && !completeStage1 && !update1)
        {
            TwitchHelpMessage += ". The current rule seed is " + ruleSeed1;
            update1 = true;
        }
        if (stage2 && !update2)
        {
            TwitchHelpMessage = "!{0} press <#> [Presses button '#' in reading order] | Presses are chainable with spaces";
            TwitchHelpMessage += ". The current rule seed is " + ruleSeed2;
            update2 = true;
        }
        if (_isSolved && !solveUpdate)
        {
            TwitchHelpMessage = "!{0} press <#> [Presses button '#' in reading order] | Presses are chainable with spaces";
            solveUpdate = true;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!completeStage1)
        {
            if (!stage1)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (TwoXTwo[i].GetComponentInChildren<TextMesh>().color != Color.white)
                    {
                        TwoXTwo[i].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                        if (stage1)
                            break;
                    }
                }
            }
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (TwoXTwo[i].GetComponentInChildren<TextMesh>().color != Color.white && TwoXTwo[i].GetComponentInChildren<TextMesh>().text == charList[resultLists[column][keyLab[Array.IndexOf(keyPos, stage1Counter)]]].ToString())
                    {
                        TwoXTwo[i].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                        if (completeStage1)
                            goto leave;
                    }
                }
            }
        }
        leave:
        if (!stage2)
        {
            for (int i = 0; i < 4; i++)
            {
                if (TwoXTwo[i].GetComponentInChildren<TextMesh>().color != Color.white)
                {
                    TwoXTwo[i].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    if (stage2)
                        break;
                }
            }
        }
        for (int i = 0; i < 16; i++)
        {
            if (FourXFour[i].GetComponentInChildren<TextMesh>().color != Color.white)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (FourXFour[i].GetComponentInChildren<TextMesh>().text == charList2[secondStageList[j]].ToString())
                    {
                        FourXFour[i].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                        break;
                    }
                }
            }
        }
    }

    private string TwitchHelpMessage = "!{0} press <#> [Presses button '#' in reading order] | Presses are chainable with spaces";

    IEnumerator ProcessTwitchCommand(string command)
    {
        
        string[] parameters = command.Split(' ');
        if (parameters[0].EqualsIgnoreCase("press"))
        {
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify at least one button to press!";
                yield break;
            }
            else
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    int temp = -1;
                    if (!int.TryParse(parameters[i], out temp))
                    {
                        yield return "sendtochaterror!f The specified button '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                    if ((!stage2 && (temp < 1 || temp > 4)) || (stage2 && (temp < 1 || temp > 16)))
                    {
                        yield return "sendtochaterror The specified button '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                }
                yield return null;
                int[] stage2Positions = { 0, 1, 4, 5, 2, 3, 6, 7, 8, 9, 12, 13, 10, 11, 14, 15 };
                KMSelectable moduleSelectable = GetComponent<KMSelectable>();
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (!stage2)
                    {
                        TwoXTwo[int.Parse(parameters[i]) - 1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        moduleSelectable.Children[stage2Positions[int.Parse(parameters[i]) - 1] + 4].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
    }

    public class MonoRandomBackdoorKeypad
    {

        /// <summary>Initializes a new instance of the <see cref="T:System.Random" /> class, using the specified seed value.</summary>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence. If a negative number is specified, the absolute value of the number is used. </param>
        /// <exception cref="T:System.OverflowException">
        ///   <paramref name="seed" /> is <see cref="F:System.Int32.MinValue" />, which causes an overflow when its absolute value is calculated. </exception>
        public MonoRandomBackdoorKeypad(int seed)
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

}
