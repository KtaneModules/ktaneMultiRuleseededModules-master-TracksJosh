using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class simonStepsScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public Light[] Lights;
    public Material Back;
    public Material[] Colors;
    public KMSelectable[] Sections;
    public TextMesh[] Digits;
    public TextMesh StageIndicator;
    public TextMesh ColorblindText;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool _isSolved = false;
    private bool ready, submit;
    private KMAudio.KMAudioRef audioReference;
    private int stageCounter;
    private int pressCounter;
    private string[,] directions = 
    {
        {"U","R","D","L","U","L","D","R" },
        {"R","D","L","U","R","U","L","D" },
        {"D","L","U","R","D","R","U","L" },
        {"L","U","R","D","L","D","R","U" }
    };
    private string[] direction = { "U", "R", "D", "L" };
    private int stage = 0;
    private string[] RuleSeeds = new string[4];
    private int[][] stages = new int[][] // Stages for Submission
    {
        new int[1],
        new int[2],
        new int[3],
        new int[4]
    };
    private string[][] stageInputs = new string[][] // Submitting per stage
    {
        new string[1],
        new string[2],
        new string[3],
        new string[4]
    };
    private int[][] stageColors =
    {
        new int[4],
        new int[4],
        new int[4],
        new int[4],
    };
    private int[][] sectionFlash = new int[4][];
    private string RuleSeed;
    private int ruleSeed = 0;
    private int cont;
    private MonoRandomSimonSteps rnd;


    // Use this for initialization
    void Start () {
        moduleId = moduleIdCounter++;
        _isSolved = false;
        ready = false;
        submit = false;
        Sections[0].OnInteract += delegate { UpButton(); return false; };
        Sections[1].OnInteract += delegate { RightButton(); return false; };
        Sections[2].OnInteract += delegate { DownButton(); return false; };
        Sections[3].OnInteract += delegate { LeftButton(); return false; };

        beginModule();

    }

    void beginModule()
    {
        int c = 0;
        ready = false;
        submit = false;
        StageIndicator.gameObject.SetActive(false);
        foreach(KMSelectable button in Sections)
        {
            int i = Rnd.Range(0, 8);
            button.GetComponent<Renderer>().material = Colors[i];
            stageColors[stage][c] = i;
            c++;
        }
        float scalar = transform.lossyScale.x;
        foreach(Light light in Lights)
        {
            light.gameObject.SetActive(false);
            light.range *= scalar * 0.5f;
        }
        foreach (TextMesh text in Digits)
        {
            text.gameObject.SetActive(false);
        }
        createStage();
        StartCoroutine(Flash());
    }
	
    private void createStage()
    {
        int flashCounter = Rnd.Range(1, 5);
        stageCounter = 0;
        pressCounter = 0;
        generateRule(flashCounter);
        generateOrder(flashCounter);
        generateDirections();
    }

    private void UpButton()
    {
        Audio.PlaySoundAtTransform("PressButton", transform);
        Sections[0].AddInteractionPunch(0.5f);
        if (!ready) ready = true;
        else
        {
            if (submit)
            {
                if (stageInputs[stage][stages[stage][stageCounter]][pressCounter].ToString() != "U")
                {
                    BombModule.HandleStrike();
                    StartCoroutine(Flashback());
                }
                else
                {
                    pressCounter++;
                    checkNext();
                }
            }
        }
    }

    private void DownButton()
    {
        Audio.PlaySoundAtTransform("PressButton", transform);
        Sections[2].AddInteractionPunch(0.5f);
        if (!ready) ready = true;
        else
        {
            if (submit)
            {
                if (stageInputs[stage][stages[stage][stageCounter]][pressCounter].ToString() != "D")
                {
                    BombModule.HandleStrike();
                    StartCoroutine(Flashback());
                }
                else
                {
                    pressCounter++;
                    checkNext();
                }
            }
        }
    }

    private void LeftButton()
    {
        Audio.PlaySoundAtTransform("PressButton", transform);
        Sections[3].AddInteractionPunch(0.5f);
        if (!ready) ready = true;
        else
        {
            if (submit)
            {
                if (stageInputs[stage][stages[stage][stageCounter]][pressCounter].ToString() != "L")
                {
                    BombModule.HandleStrike();
                    StartCoroutine(Flashback());
                }
                else
                {
                    pressCounter++;
                    checkNext();
                }
            }
        }
    }

    private void RightButton()
    {
        Audio.PlaySoundAtTransform("PressButton", transform);
        Sections[1].AddInteractionPunch(0.5f);
        if (!ready) ready = true;
        else
        {
            if (submit)
            {
                if (stageInputs[stage][stages[stage][stageCounter]][pressCounter].ToString() != "R")
                {
                    BombModule.HandleStrike();
                    StartCoroutine(Flashback());
                }
                else
                {
                    pressCounter++;
                    checkNext();
                }
            }
        }
    }

    private void checkNext()
    {
        if(pressCounter == stageInputs[stage][stages[stage][stageCounter]].Length)
        {
            stageCounter++;
            if(stageCounter == 4)
            {
                BombModule.HandlePass();
                _isSolved = true;
                Audio.PlaySoundAtTransform("PowerDown", transform);
                StageIndicator.gameObject.SetActive(false);

            }
            else if(stageCounter == stage+1)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TitleMenuPressed, transform);
                if (stage != 0) RuleSeed = ruleSeed.ToString();
                cont = RuleSeed.Length;
                stage++;
                stageCounter = 0;
                pressCounter = 0;
                beginModule();

            }
            else
            {
                pressCounter = 0;
                StageIndicator.text = (stages[stage][stageCounter] + 1).ToString();
                Debug.LogFormat("[Simon Steps #{1}] Stage: {0}", stageInputs[stage][stages[stage][stageCounter]],moduleId);
            }
        }
    }

    private void StartSubmit()
    {
        submit = true;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TitleMenuPressed, transform);
        Debug.LogFormat("[Simon Steps #{1}] Stage: {0}", stageInputs[stage][stages[stage][stageCounter]], moduleId);
        StopCoroutine(Flash());
        foreach (Light light in Lights)
        {
            light.gameObject.SetActive(false);
        }
        foreach (TextMesh text in Digits)
        {
            text.gameObject.SetActive(false);
        }
        foreach (KMSelectable k in Sections)
        {
            k.gameObject.GetComponent<Renderer>().material = Back;
        }
        StageIndicator.text = (stages[stage][stageCounter]+1).ToString();
        StageIndicator.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        if (audioReference != null)
            audioReference.StopSound();
    }

    private void generateRule(int flashCount)
    {
        for (int i = 0; i < flashCount; i++)
        {
            RuleSeeds[stage] += Rnd.Range(0, 10).ToString();
            RuleSeed += RuleSeeds[stage][i];
        }
        ruleSeed = (int)(long.Parse(RuleSeed) % 2147483647);
        
    }

    private void generateOrder(int flashCount)
    {
        sectionFlash[stage] = new int[flashCount];
        for (int i = 0; i < flashCount; i++)
        {
            sectionFlash[stage][i] = Rnd.Range(0, 4);
        }
        string logFlash = "";
        string logDir = "";
        int c = 1;
        foreach (int i in sectionFlash[stage])
        {
            logFlash += Sections[i].GetComponent<Renderer>().material.name;
            logDir += direction[i];
            if(c != flashCount)
            {
                logFlash = logFlash.Replace(" (Instance)", ", ");
                logDir += ", ";
                c++;
            }
            else
            {
                logFlash = logFlash.Replace(" (Instance)", "");
            }
        }

        Debug.LogFormat("[Simon Steps #{0}] Stage {1}: Flashes: {2}. Directions: {3}. Digits: {4}", moduleId, stage + 1, logFlash, logDir, RuleSeeds[stage]);
        Debug.LogFormat("[Simon Steps #{0}] Rule Seed {1}: {2}", moduleId, stage + 1, ruleSeed);
    }

    private void generateDirections()
    {
        rnd = new MonoRandomSimonSteps(ruleSeed);
        if(ruleSeed != 1)
        {
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    directions[i, j] = "";
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    directions[i, j] = direction[rnd.Next(4)];
                }
            }

        }
        generateSubmissionString();
        
    }

    private void generateSubmissionString()
    {
       for(int i = 0; i < stage+1; i++)
       {
            for(int j = 0; j < sectionFlash[i].Length; j++)
            {
                switch (stageColors[i][sectionFlash[i][j]])
                {
                    case 0: stageInputs[stage][i] += directions[stage,0]; break;
                    case 1: stageInputs[stage][i] += directions[stage,1]; break;
                    case 2: stageInputs[stage][i] += directions[stage,2]; break;
                    case 3: stageInputs[stage][i] += directions[stage,3]; break;
                    case 4: stageInputs[stage][i] += directions[stage,4]; break;
                    case 5: stageInputs[stage][i] += directions[stage,5]; break;
                    case 6: stageInputs[stage][i] += directions[stage,6]; break;
                    case 7: stageInputs[stage][i] += directions[stage,7]; break;
                }
            }
       }
        for(int i =0; i < stage+1; i++)
        {
            stages[stage][i] = Rnd.Range(0, stage + 1);
            if(stage >= 1) while (stages[stage][1] == stages[stage][0]) stages[stage][1] = (stages[stage][1] + 1) % 2;
            if(stage >= 2) while (stages[stage][2] == stages[stage][0] || stages[stage][2] == stages[stage][1]) stages[stage][2] = (stages[stage][2] + 1) % 3;
            if(stage >= 3) while (stages[stage][3] == stages[stage][0] || stages[stage][3] == stages[stage][1] || stages[stage][3] == stages[stage][2]) stages[stage][3] = (stages[stage][3] + 1) % 4;
        }

        string log = "";
        for(int i = 0; i < stage+1; i++)
        {
            for(int j = 0; j < sectionFlash[i].Length; j++)
            {
                log += direction[sectionFlash[i][j]];
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    IEnumerator Flash()
    {
        while (!ready)
        {
            for (int i = 0; i < sectionFlash[stage].Length; i++)
            {
                Lights[sectionFlash[stage][i]].gameObject.SetActive(true);
                Digits[sectionFlash[stage][i]].gameObject.SetActive(true);
                Digits[sectionFlash[stage][i]].text = RuleSeed[i+cont].ToString();
                ColorblindText.text = Sections[sectionFlash[stage][i]].GetComponent<Renderer>().material.name.Replace(" (Instance)", "").ToUpper()[0].ToString();
                yield return null;
                yield return new WaitForSeconds(0.5f);
                Lights[sectionFlash[stage][i]].gameObject.SetActive(false);
                Digits[sectionFlash[stage][i]].gameObject.SetActive(false);
                ColorblindText.text = "";
                yield return new WaitForSeconds(0.5f);
                yield return null;
            }
            if (!ready) yield return new WaitForSeconds(2f);
        }
        if (!ready) StartCoroutine(Flash());
        if (ready)
        {
            StartSubmit();
            yield return null;
        }
    }

    IEnumerator Flashback()
    {
        submit = false;
        StageIndicator.gameObject.SetActive(false);
        stageCounter = 0;
        pressCounter = 0;
        for (int j = 0; j < stage+1; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                Sections[i].GetComponent<MeshRenderer>().material = Colors[stageColors[j][i]];
            }
            for (int i = 0; i < sectionFlash[j].Length; i++)
            {
                Lights[sectionFlash[j][i]].gameObject.SetActive(true);
                Digits[sectionFlash[j][i]].gameObject.SetActive(true);
                Digits[sectionFlash[j][i]].text = RuleSeeds[j][i].ToString();
                ColorblindText.text = Sections[sectionFlash[j][i]].GetComponent<Renderer>().material.name.Replace(" (Instance)", "").ToUpper()[0].ToString();
                yield return null;
                yield return new WaitForSeconds(0.4f);
                Lights[sectionFlash[j][i]].gameObject.SetActive(false);
                Digits[sectionFlash[j][i]].gameObject.SetActive(false);
                ColorblindText.text = "";
                yield return new WaitForSeconds(0.4f);
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
        }
        StartSubmit();
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        int start = stage;
        for (int i = start; i < 4; i++)
        {
            if (!ready)
                Sections[0].OnInteract();
            while (!submit) yield return true;
            int start2 = stageCounter;
            int end = stage + 1;
            for (int k = start2; k < end; k++)
            {
                int start3 = pressCounter;
                for (int j = start3; j < stageInputs[stage][stages[stage][k]].Length; j++)
                {
                    Sections[Array.IndexOf(direction, stageInputs[stage][stages[stage][k]][j].ToString())].OnInteract();
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }

    private string TwitchHelpMessage = "!{0} press <u/l/d/r> [Presses the up, left, down, or right button] | Presses are chainable with or without spaces";

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
                    for (int j = 0; j < parameters[i].Length; j++)
                    {
                        if (!"URDL".Contains(parameters[i][j].ToString().ToUpper()))
                        {
                            yield return "sendtochaterror!f The specified button '" + parameters[i][j] + "' is invalid!";
                            yield break;
                        }
                    }
                }
                yield return null;
                for (int i = 1; i < parameters.Length; i++)
                {
                    for (int j = 0; j < parameters[i].Length; j++)
                    {
                        Sections["URDL".IndexOf(parameters[i][j].ToString().ToUpper())].OnInteract();
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
        }
    }

    public class MonoRandomSimonSteps
    {

        /// <summary>Initializes a new instance of the <see cref="T:System.Random" /> class, using the specified seed value.</summary>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence. If a negative number is specified, the absolute value of the number is used. </param>
        /// <exception cref="T:System.OverflowException">
        ///   <paramref name="seed" /> is <see cref="F:System.Int32.MinValue" />, which causes an overflow when its absolute value is calculated. </exception>
        public MonoRandomSimonSteps(int seed)
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
