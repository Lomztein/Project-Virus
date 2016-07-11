﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Highscore : MonoBehaviour {

    public static Highscore instance;
    public Text names;
    public Text scores;

    public InputField inputName;
    public Button submitButton;

    public static void Add ( string playerName, int score) {
        List<Entry> entries = Utility.LoadObjectFromFile<List<Entry>> (Game.GAME_DATA_DIRECTORY + Game.difficulty.name + "-highscores.dat");
        if (entries == null)
            entries = new List<Entry> ();

        int result = 0;
        for (int i = 0; i < entries.Count; i++) {
            if (entries[i].scoreValue < score) {
                result = i;
                break;
            }
        }

        if (result < 10) {
            entries.Insert (result, new Entry (playerName, score));
            Utility.SaveObjectToFile (Game.GAME_DATA_DIRECTORY + Game.difficulty.name + "-highscores.dat", entries);
        }
    }

    public void AddPlayer () {
        Add (inputName.text, EnemyManager.externalWaveNumber);
        inputName.interactable = false;
        submitButton.interactable = false;
        Display (names, scores);
    }

    void Awake () {
        instance = this;
    }

    public void InstanceDisplay () {
        Display (names, scores);
    }

    public static void Display (Text names, Text scores) {
        List<Entry> entries = Utility.LoadObjectFromFile<List<Entry>> (Game.GAME_DATA_DIRECTORY + Game.difficulty.name + "-highscores.dat");
        if (entries == null)
            entries = new List<Entry> ();

        string nameText = "";
        string scoreText = "";

        for (int i = 0; i < Mathf.Min (entries.Count, 10); i++) {
            nameText += (i+1) + " - " + entries[i].playerName + "\n\n";
            scoreText += entries[i].scoreValue + "\n\n";
        }
        for (int i = entries.Count; i < 10; i++) {
            nameText += (i + 1) + " - N/A\n\n";
        }

        names.text = nameText;
        scores.text = scoreText;
    }

    [System.Serializable]
    public class Entry {

        public string playerName;
        public int scoreValue;

        public Entry (string n, int s) {
            playerName = n;
            scoreValue = s;
        }
    }
}
