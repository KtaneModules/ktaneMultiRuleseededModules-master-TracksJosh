using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;
using System.Linq;
using System;
using System.Runtime.InteropServices;

public class newWorldScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public TextMesh RuleSeed1;
    public GameObject[] MazeItself;
    public KMSelectable Ready;
    public KMSelectable UpButton;
    public KMSelectable DownButton;
    public KMSelectable LeftButton;
    public KMSelectable RightButton;
    public GameObject[] Squares;
    public GameObject[] Triangles;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private int dBatt, aaBatt, battHold, poPl, ports, unlit, lit, digits;
    private bool ready, cycle = false;
    private bool _isSolved = false;
    private string[,] Maze;
    public static MonoRandomNewWorld rnd;
    private int ruleSeed1;
    private int ruleSeed2;
    private int yourX, saveX;
    private int yourY, saveY;
    private int goalX;
    private int goalY;
    private int counter = 0;
    private string ruleSeed;
    string[] theRules = { "D Batteries", "AA Batteries", "Battery Holders", "Port Plates", "Ports", "Unlit Indicators", "Lit Indicators", "Digits of Serial Number (Excluding Leading Zeros)" };
    
    // Use this for initialization
    void Start() {
        _isSolved = false;
        moduleId = moduleIdCounter++;
        UpButton.OnInteract += delegate { if (ready) Up(); return false; };
        DownButton.OnInteract += delegate { if (ready) Down(); return false; };
        LeftButton.OnInteract += delegate { if (ready) Left(); return false; };
        RightButton.OnInteract += delegate { if (ready) Right(); return false; };
        Ready.OnInteract += delegate { if (!ready) ReleaseMaze(); return false; };
        beginModule();

    }

    private void beginModule()
    {
        for (int i = 0; i < MazeItself.Length; i++)
        {
            MazeItself[i].SetActive(false);
        }
        RuleSeed1.text = "";
        generateRule1();

        ruleSeed += ' ';
        battHold = Bomb.GetBatteryHolderCount();
        dBatt = 2 * battHold - Bomb.GetBatteryCount();
        aaBatt = 2 * (Bomb.GetBatteryCount() - battHold);
        poPl = Bomb.GetPortPlateCount();
        ports = Bomb.GetPortCount();
        unlit = Bomb.GetOffIndicators().Count();
        lit = Bomb.GetOnIndicators().Count();
        var h = "";
        foreach (var digit in Bomb.GetSerialNumberNumbers())
        {
            h += digit;
        }
        int[] rules = new int[8];
        int[] rules2 = new int[8];
        digits = (int)long.Parse(h);
        rnd = new MonoRandomNewWorld(ruleSeed1);
        modifyRules(rules);
        for (int i = 0; i < rules.Length; i++)
        {
            switch (rules[i])
            {
                case 0:
                    rules[i] = dBatt; break;
                case 1:
                    rules[i] = aaBatt; break;
                case 2:
                    rules[i] = battHold; break;
                case 3:
                    rules[i] = poPl; break;
                case 4:
                    rules[i] = ports; break;
                case 5:
                    rules[i] = unlit; break;
                case 6:
                    rules[i] = lit; break;
                case 7:
                    rules[i] = digits; break;
            }
        }
        for (int i = 0; i < rules.Length; i++) Debug.LogFormat("[New World #{0}] Rule #{1}: {3} = {2}", moduleId, i + 1, rules[i], theRules[i]);
        generateRule2(rules);
        yourX = Rnd.Range(0, 6);
        yourY = Rnd.Range(0, 6);
        goalX = Rnd.Range(0, 6);
        goalY = Rnd.Range(0, 6);
        saveX = yourX;
        saveY = yourY;
        if (goalX == yourX && goalY == yourY)
        {
            goalX = (goalX + Rnd.Range(1, 6)) % 6;
            goalY = (goalY + Rnd.Range(1, 6)) % 6;
        }

    }

    // Update is called once per frame
    void Update() {
        if (!ready)
        {
            if(!cycle) StartCoroutine(CycleDigits());
            cycle = true;
        }
        
    }

    private void Up()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        UpButton.AddInteractionPunch(0.5f);
        Squares[yourY * 6 + yourX].SetActive(false);
        Triangles[goalY * 6 + goalX].SetActive(false);
        if (Maze[yourY, yourX].Contains('U'))
        {
            BombModule.HandleStrike();
            Squares[yourY * 6 + yourX].SetActive(true);
            Triangles[goalY * 6 + goalX].SetActive(true);
        }
        else
        {
            yourY--;
        }
        if (yourX == goalX && yourY == goalY)
        {
            Audio.PlaySoundAtTransform("pew", transform);
            _isSolved = true;
            BombModule.HandlePass();
        }
    }
    private void Down()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        DownButton.AddInteractionPunch(0.5f);
        Squares[yourY * 6 + yourX].SetActive(false);
        Triangles[goalY * 6 + goalX].SetActive(false);
        if (Maze[yourY, yourX].Contains('D'))
        {
            BombModule.HandleStrike();
            Squares[yourY * 6 + yourX].SetActive(true);
            Triangles[goalY * 6 + goalX].SetActive(true);
        }
        else
        {
            yourY++;
        }
        if (yourX == goalX && yourY == goalY)
        {
            Audio.PlaySoundAtTransform("pew", transform);
            _isSolved = true;
            BombModule.HandlePass();
        }
    }
    private void Left()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        LeftButton.AddInteractionPunch(0.5f);
        Squares[yourY * 6 + yourX].SetActive(false);
        Triangles[goalY * 6 + goalX].SetActive(false);
        if (Maze[yourY, yourX].Contains('L'))
        {
            BombModule.HandleStrike();
            Squares[yourY * 6 + yourX].SetActive(true);
            Triangles[goalY * 6 + goalX].SetActive(true);
        }
        else
        {
            yourX--;
        }
        if (yourX == goalX && yourY == goalY)
        {
            Audio.PlaySoundAtTransform("pew", transform);
            _isSolved = true;
            BombModule.HandlePass();
        }
    }
    private void Right()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        RightButton.AddInteractionPunch(0.5f);
        Squares[yourY * 6 + yourX].SetActive(false);
        Triangles[goalY * 6 + goalX].SetActive(false);
        if (Maze[yourY, yourX].Contains('R'))
        {
            BombModule.HandleStrike();
            Squares[yourY * 6 + yourX].SetActive(true);
            Triangles[goalY * 6 + goalX].SetActive(true);
            
        }
        else
        {
            yourX++;
        }
        if(yourX == goalX && yourY == goalY)
        {
            Audio.PlaySoundAtTransform("pew", transform);
            _isSolved = true;
            BombModule.HandlePass();
        }
    }
    private void ReleaseMaze()
    {
        Audio.PlaySoundAtTransform("pow", transform);
        Ready.AddInteractionPunch(0.5f);
        ready = true;
        Ready.gameObject.SetActive(false);
        for (int i = 0; i < MazeItself.Length; i++)
        {
            MazeItself[i].SetActive(true);
        }
        rnd = new MonoRandomNewWorld(ruleSeed2);
        if (ruleSeed2 == 1)
        {
            Maze = new string[6,6]
                {
                    {"U L D", "U", "U R D", "U L", "U", "U R D" },
                    {"U L", "D R","U L","D R","L D","U R" },
                    {"L R","L U","D R","U L","U D","R" },
                    {"L","D R","U L","D R","U L R","L R" },
                    {"L R","L U R","L R","U L","D R","L R" },
                    {"L D R", "L D", "D R", "L D", "U D", "D R"}
                };
        }
        else
        {
            
            var cardinal = new[] { "N", "E", "W", "S" };
            var direction = new[] { "U", "R", "L", "D" };
            
            MazeGeneration.InitializeGeneration();
            Maze = Enumerable.Range(0, 6).SelectMany(r => Enumerable.Range(0, 6).Select(c => Enumerable.Range(0, 4).Select(d => (!MazeGeneration.Cells[0,r,c][cardinal[d]]) ? direction[d] : "").Join(" "))).ToArray().ToArray2D(6, 6);
            Debug.LogFormat("[New World #{0}] {1}", moduleId, generateSVG());
        }
        showLocations();
    }

    private string generateSVG()
    {
        string svg = @"=svg[Maze:]";
        svg += "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"-9 1.5 725 300\"><g><rect x=\"240\" y=\"21\" width=\"270\" height=\"270\" fill=\"transparent\" stroke=\"black\" stroke-width=\"2\"></rect>";
        for(int j = 0; j < 6; j++)
        {
            for(int k = 0; k < 6; k++)
            {
                var x = 2 + 45 * k + 237.5;
                var y = 21 + 45 * j;
                var small = "<rect x=\""+(x+19)+"\" y=\""+(y+19)+"\" width=\"12\" height=\"12\" fill=\"black\"></rect>";
                svg += small;
                if(k < 5 && Maze[j, k].Contains("R"))
                {
                    var e = "<line x1=\""+(x+45)+"\" y1=\""+y+"\" x2=\""+(x+45)+"\" y2=\""+(y+45)+"\" stroke=\"black\" stroke-width=\"2\"></line>";
                    svg += e;
                }
                if (j < 5 && Maze[j, k].Contains("D"))
                {
                    var e = "<line x1=\"" + x + "\" y1=\"" + (y+45) + "\" x2=\"" + (x + 45) + "\" y2=\"" + (y + 45) + "\" stroke=\"black\" stroke-width=\"2\"></line>";
                    svg += e;
                }
            }
        }
        svg += "</g></svg>";
        return svg;
    }

    private void showLocations()
    {
        for(int i = 0; i < 6; i++)
        {
            MazeItself[i].SetActive(true);
        }
        for(int i = 0; i < Squares.Length; i++)
        {
            Squares[i].SetActive(false);
            Triangles[i].SetActive(false);
        }
        Squares[yourY*6+yourX].SetActive(true);
        Triangles[goalY*6+goalX].SetActive(true);
        char[] alpha = { 'A', 'B', 'C', 'D', 'E', 'F' };
        char[] num = { '1', '2', '3', '4', '5', '6' };
        Debug.LogFormat("[New World #{0}] Starting Location: {1}{2}", moduleId, alpha[yourX], num[yourY]);
        Debug.LogFormat("[New World #{0}] Goal Location: {1}{2}", moduleId, alpha[goalX], num[goalY]);
    }

    private void generateRule1()
    {
        for (int i = 0; i < 10; i++)
        {
            ruleSeed += Rnd.Range(0, 10);
        }
        ruleSeed1 = (int)(long.Parse(ruleSeed)%2147483647);
        ruleSeed = ruleSeed1.ToString();
        Debug.LogFormat("[New World #{0}] Rule Seed #1: {1}", moduleId, ruleSeed1);
    }

    private void generateRule2(int[] rules)
    {
        string ruleSeed3 = "";
        for(int i = 0; i < rules.Length; i++)
        {
            ruleSeed3 += rules[i].ToString();
        }
        ruleSeed2 = (int)(long.Parse(ruleSeed3) % 2147483647);
        Debug.LogFormat("[New World #{0}] Rule Seed #2: {1}", moduleId, ruleSeed2);
    }

    private void modifyRules(int[] rules)
    {
        if (ruleSeed1 == 1)
        {
            for (int i = 0; i < rules.Length; i++) rules[i] = i;
            for (int i = 0; i < rules.Length; i++) Debug.LogFormat("[New World #{0}] Rule #{2}: {1}", moduleId, theRules[rules[i]], i + 1);
        }
        else
        {
            //rnd = new MonoRandom(ruleSeed1);
            int random = rnd.Next(8);
            for (int i = 0; i < rules.Length; i++) rules[i] = i;
            for (int i = 0; i < random; i++)
            {
                int r1 = rnd.Next(8);
                int r2 = rnd.Next(8);
                int temp = rules[r1];
                rules[r1] = rules[r2];
                rules[r2] = temp;
                string temp2 = theRules[r1];
                theRules[r1] = theRules[r2];
                theRules[r2] = temp2;
            }
        }
    }

    IEnumerator CycleDigits()
    {
        if (counter == ruleSeed.Length)
        {
            counter = 0;
        }
        RuleSeed1.text = ruleSeed[counter].ToString();
        yield return new WaitForSecondsRealtime(0.5f);
        RuleSeed1.text = "";
        counter++;
        yield return new WaitForSecondsRealtime(0.5f);
        cycle = false;
    }
    IEnumerator TwitchHandleForcedSolves()
    {
        if (!ready)
        {
            ReleaseMaze();
            yield return new WaitForSeconds(0.1f);
        }
    }
    private string TwitchHelpMessage = "Use !{0} ready to activate the maze. Use !{0} move/press/submit uldr to move up, left, down, and right. Use !{0} reset or !{0} press reset to reset back to the start.";

    IEnumerator ProcessTwitchCommand(string input)
    {
        List<KMSelectable> Buttons = new List<KMSelectable>();

        string twitchInput = input.ToLowerInvariant();

        if (twitchInput.Equals("ready"))
        {
            if (ready)
            {
                yield return "sendtochaterror You have activated the maze!";
            }
            ReleaseMaze();
            yield return null;
        }
        if (!ready && (twitchInput.StartsWith("move ") || twitchInput.StartsWith("press ") || twitchInput.StartsWith("submit ")))
        {
            yield return "sendtochaterror You have not activated the maze yet!";
        }
        if (twitchInput.Equals("reset") || twitchInput.Equals("press reset"))
        {
            yourX = saveX;
            yourY = saveY;
            Squares[yourY * 6 + yourX].SetActive(true);
            Triangles[goalY * 6 + goalX].SetActive(true);
            yield return null;
        }

        
        
        if (twitchInput.StartsWith("move ") || twitchInput.StartsWith("press ") || twitchInput.StartsWith("submit "))
        {
            twitchInput = twitchInput.Substring(twitchInput.IndexOf(" ", System.StringComparison.Ordinal) + 1);
            foreach (char character in twitchInput)
            {
                switch (character)
                {
                    case 'u':
                        Up();
                        yield return new WaitForSeconds(0.1f);
                        break;
                    case 'r':
                        Right();
                        yield return new WaitForSeconds(0.1f);
                        break;
                    case 'd':
                        Down();
                        yield return new WaitForSeconds(0.1f);
                        break;
                    case 'l':
                        Left();
                        yield return new WaitForSeconds(0.1f);
                        break;
                    case ' ':
                        break;
                    default:
                        break;
                }
            }
        }
        yield return null;

    }

    

    public class MonoRandomNewWorld
    {
        
        /// <summary>Initializes a new instance of the <see cref="T:System.Random" /> class, using the specified seed value.</summary>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence. If a negative number is specified, the absolute value of the number is used. </param>
        /// <exception cref="T:System.OverflowException">
        ///   <paramref name="seed" /> is <see cref="F:System.Int32.MinValue" />, which causes an overflow when its absolute value is calculated. </exception>
        public MonoRandomNewWorld(int seed)
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
