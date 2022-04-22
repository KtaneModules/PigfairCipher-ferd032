using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using KModkit;

public class PigfairCipher : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;

    private int moduleId;
    private static int moduleIdCounter = 1;
    private bool moduleSolved;

    public KMSelectable[] Buttons;
    public TextMesh InputText;
    public Renderer[] ScreenImages;
    public Texture[] PigpenTextures;

    private string input = "";
    private string SerialNumber;
    private static readonly string[] wordList = new string[] { "ADVANCED", "ADVERTED", "ADVOCATE", "ADDITION", "ALLOCATE", "ALLOTYPE", "ALLOTTED", "ALTERING", "BINARIES", "BINORMAL", "BINOMIAL", "BILLIONS", "BULKHEAD", "BULLHORN", "BULLETED", "BULWARKS", "CIPHERED", "CIRCUITS", "CONNECTS", "CONQUERS", "COMMANDO", "COMPILER", "COMPUTER", "CONTINUE", "DECRYPTS", "DECEIVED", "DECIMATE", "DIVISION", "DISCOVER", "DISCRETE", "DISPATCH", "DISPOSAL", "ENCIPHER", "ENCRYPTS", "ENCODING", "ENTRANCE", "EQUALISE", "EQUATORS", "EQUATION", "EQUIPPED", "FINALISE", "FINISHED", "FINDINGS", "FINNICKY", "FORMULAE", "FORTUNES", "FORTRESS", "FORWARDS", "GARRISON", "GARNERED", "GATEPOST", "GATEWAYS", "GAUNTLET", "GAMBLING", "GATHERED", "GLOOMING", "HAZARDED", "HAZINESS", "HOTLINKS", "HOTHEADS", "HUNDREDS", "HUNKERED", "HUNTSMAN", "HUNTRESS", "INCOMING", "INDICATE", "INDIRECT", "INDIGOES", "ILLUDING", "ILLUSION", "ILLUSORY", "ILLUMINE", "JIGSAWED", "JIMMYING", "JOURNEYS", "JOUSTING", "JUNCTION", "JUNCTURE", "JUNKYARD", "JUDGMENT", "KILOWATT", "KILOVOLT", "KILOBYTE", "KINETICS", "KNOCKING", "KNOCKOUT", "KNOWABLE", "KNUCKLED", "LANGUAGE", "LANDMARK", "LIMITING", "LINEARLY", "LINGERED", "LINKAGES", "LINKWORK", "LABELING", "MONOGRAM", "MONOLITH", "MONOMIAL", "MONOTONE", "MULTITON", "MULTIPLY", "MULCTING", "MULLIGAN", "NANOBOTS", "NANOGRAM", "NANOWATT", "NANOTUBE", "NUMBERED", "NUMEROUS", "NUMERALS", "NUMERATE", "OCTANGLE", "OCTUPLES", "ORDERING", "ORDINALS", "OBSERVED", "OBSCURED", "OBSTRUCT", "OBSTACLE", "PROGRESS", "PROJECTS", "PROPHASE", "PROPHECY", "POSTSYNC", "POSSIBLE", "POSITRON", "POSITIVE", "QUADRANT", "QUADRICS", "QUARTILE", "QUARTICS", "QUICKEST", "QUIRKISH", "QUINTICS", "QUITTERS", "REVERSED", "REVOLVED", "REVEALED", "ROTATION", "ROTATORS", "RELATION", "RELATIVE", "RELAYING", "STARTING", "STANDARD", "STANDOUT", "STANZAIC", "STOCCATA", "STOCKADE", "STOPPING", "STOPWORD", "TRICKIER", "TRIGONAL", "TRIGGERS", "TRIANGLE", "TOMOGRAM", "TOMAHAWK", "TOGGLING", "TOGETHER", "UNDERRUN", "UNDERWAY", "UNDERLIE", "UNDOINGS", "ULTERIOR", "ULTIMATE", "ULTRARED", "ULTRAHOT", "VENOMOUS", "VENDETTA", "VICINITY", "VICELESS", "VOLITION", "VOLTAGES", "VOLATILE", "VOLUMING", "WEAKENED", "WEAPONED", "WINGDING", "WINNABLE", "WHATEVER", "WHATNESS", "WHATNOTS", "WHATSITS", "YELLOWED", "YEARLONG", "YEARNING", "YEASAYER", "YIELDING", "YIELDERS", "YOKOZUNA", "YOURSELF", "ZIPPERED", "ZIGGURAT", "ZIGZAGGY", "ZUGZWANG", "ZYGOMATA", "ZYGOTENE", "ZYMOLOGY", "ZYMOGRAM" };
    private int[] playfair = new int[25];
    private int[][] playfairLog = new int[5][] { new int[5], new int[5], new int[5], new int[5], new int[5] };
    private string chosenWord;
    private int[] chosenWordNumbers = new int[8];
    private int[][] playfairPairs = new int[4][] { new int[2], new int[2], new int[2], new int[2] };
    private int[][] playfairPairsAfter = new int[4][] { new int[2], new int[2], new int[2], new int[2] };
    private int[] chosenWordEncrypted = new int[8];
    private static readonly string keyOrder = "QWERTYUIOPASDFGHJKLZXCVBNM";
    private static readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private void Start()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < Buttons.Length; i++)
        {
            int j = i;
            Buttons[i].OnInteract += delegate ()
            {
                ButtonPress(j);
                return false;
            };
        }
        SerialNumber = BombInfo.GetSerialNumber();
        Generate();
    }

    private void ButtonPress(int button)
    {
        Buttons[button].AddInteractionPunch(0.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[button].transform);
        if (moduleSolved)
            return;
        if (button == 26)
        {
            input = "";
            InputText.text = input;
            return;
        }
        if (button == 27)
        {
            Submit();
            return;
        }
        if (input.Length == 8)
            return;
        input += keyOrder[button].ToString();
        InputText.text = input;
    }

    private void Generate()
    {
        List<int> list = new List<int>();
        List<int> list1 = new List<int>();
        for(int i = 0; i < SerialNumber.Length; i++)
        {
            int val = 0;
            if (SerialNumber[i] >= '0' && SerialNumber[i] <= '9')
            {
                val = SerialNumber[i] - '0' - 1;
                if (val == -1)
                    val = 9;
            }
            else
            {
                val = SerialNumber[i] - 'A';
                if (val == 23)
                    continue;
            }
            if (!list1.Contains(val))
                list1.Add(val);
        }
        List<int> list2 = new List<int>();
        for (int i = 0; i < 26; i++)
        {
            if (alphabet[i] == 'X')
                continue;
            int val = alphabet[i] - 'A';
            if (!list1.Contains(val))
                list2.Add(val);
        }
        bool evenBatteries = BombInfo.GetBatteryCount() % 2 == 0;
        if (evenBatteries)
        {
            list.AddRange(list1);
            list.AddRange(list2);
        }
        else
        {
            list.AddRange(list2);
            list.AddRange(list1);
        }
        for (int i = 0; i < 25; i++)
            playfair[i] = list[i];
        Debug.LogFormat("[Pigfair Cipher #{0}] Playfair grid:", moduleId);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
                playfairLog[i][j] = playfair[i * 5 + j];
            Debug.LogFormat("[Pigfair Cipher #{0}] {1}", moduleId, playfairLog[i].Select(s => alphabet[s]).Join(" "));
        }
        chosenWord = wordList[Rnd.Range(0, wordList.Length)];
        Debug.LogFormat("[Pigfair Cipher #{0}] Chosen word: {1}", moduleId, chosenWord);
        for (int i = 0; i < 8; i++)
        {
            chosenWordNumbers[i] = chosenWord[i] - 'A';
            playfairPairs[i / 2][i % 2] = chosenWordNumbers[i];
        }
        for (int pairNum = 0; pairNum < 4; pairNum++)
        {
            int pair1 = Array.IndexOf(playfair, playfairPairs[pairNum][0]);
            int pair2 = Array.IndexOf(playfair, playfairPairs[pairNum][1]);
            int row1 = pair1 / 5;
            int col1 = pair1 % 5;
            int row2 = pair2 / 5;
            int col2 = pair2 % 5;
            if (row1 == row2 && col1 == col2)
            {
                playfairPairsAfter[pairNum][0] = playfairPairs[pairNum][0];
                playfairPairsAfter[pairNum][1] = playfairPairs[pairNum][1];
            }
            else if (row1 == row2)
            {
                playfairPairsAfter[pairNum][0] = playfair[row1 * 5 + ((col1 + 1) % 5)];
                playfairPairsAfter[pairNum][1] = playfair[row2 * 5 + ((col2 + 1) % 5)];
            }
            else if (col1 == col2)
            {
                playfairPairsAfter[pairNum][0] = playfair[(row1 + 1) % 5 * 5 + col1];
                playfairPairsAfter[pairNum][1] = playfair[(row2 + 1) % 5 * 5 + col2];
            }
            else
            {
                playfairPairsAfter[pairNum][0] = playfair[row1 * 5 + col2];
                playfairPairsAfter[pairNum][1] = playfair[row2 * 5 + col1];
            }
            chosenWordEncrypted[pairNum * 2] = playfairPairsAfter[pairNum][0];
            chosenWordEncrypted[pairNum * 2 + 1] = playfairPairsAfter[pairNum][1];
        }
        Debug.LogFormat("[Pigfair Cipher #{0}] Encrypted word: {1}", moduleId, chosenWordEncrypted.Select(i => alphabet[i]).Join(""));
        for (int i = 0; i < chosenWordEncrypted.Length; i++)
        {
            int val = chosenWordEncrypted[i];
            ScreenImages[i].material.mainTexture = PigpenTextures[Array.IndexOf(playfair, val)];
        }
    }

    private void Submit()
    {
        if (chosenWord == input)
        {
            moduleSolved = true;
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            Debug.LogFormat("[Pigfair Cipher #{0}] Submitted {1}. Module solved.", moduleId, input);
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[Pigfair Cipher #{0}] Submitted {1}. Strike.", moduleId, input);
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Use !{0} submit <answer> to submit your answer. Answers must be eight letters long.";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        Match m = Regex.Match(command, @"^\s*submit\s+([a-z]{8})\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
        {
            yield return "sendtochaterror Invalid command! Use !{0} submit <answer> to submit your answer. Answers must be eight letters long.";
            yield break;
        }
        yield return null;
        yield return "strike";
        yield return "solve";
        if (input != "")
        {
            Buttons[26].OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
        string answer = m.Groups[1].Value.ToUpperInvariant();
        int[] order = keyOrder.Select(i => i - 'A').ToArray();
        for (int i = 0; i < answer.Length; i++)
        {
            Buttons[Array.IndexOf(order, answer[i] - 'A')].OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
        Buttons[27].OnInteract();
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (input != "")
        {
            Buttons[26].OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
        int[] order = keyOrder.Select(i => i - 'A').ToArray();
        for (int i = 0; i < chosenWord.Length; i++)
        {
            Buttons[Array.IndexOf(order, chosenWord[i] - 'A')].OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
        Buttons[27].OnInteract();
    }
}
