﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class EnemyManager : MonoBehaviour {

	[Header ("References")]
	public static string WAVESET_FILE_EXTENSION = ".dat";
	
	public float spawnTime = 1f;

	public GameObject[] enemyTypes;

	public static bool waveStarted;
	public static bool wavePrebbing;

	public Image waveStartedIndicator;
	public Text waveCounterIndicator;
	public GameObject gameOverIndicator;

	[Header ("Wave Stuffs")]
	public List<Wave> waves = new List<Wave>();
	public Wave.Subwave currentSubwave;

	public int waveNumber;
	private int subwaveNumber;
	private int[] spawnIndex;
	private int endedIndex;
	public int waveMastery = 1;
    public int amountModifier = 1;

	public static float readyWaitTime = 2f;
	
	public static float gameProgress = 1f;
	public float gameProgressSpeed = 1f;

	[Header ("Enemies")]
	public int currentEnemies;
	public GameObject endBoss;
	private Dictionary<Wave.Enemy, List<GameObject>> pooledEnemies = new Dictionary<Wave.Enemy, List<GameObject>> ();
	public Transform enemyPool;
    private List<Enemy> spawnedEnemies = new List<Enemy> ();

	public static EnemyManager cur;

	[Header ("Upcoming Wave")]
	public RectTransform upcomingCanvas;
	public RectTransform upcomingWindow;
	public GameObject upcomingEnemyPrefab;
	public GameObject upcomingSeperatorPrefab;

	private List<GameObject> upcomingContent = new List<GameObject>();
	private List<UpcomingElement> upcomingElements = new List<UpcomingElement>();

	public float buttonSize;
	public float seperatorSize;
	public float windowPosY;

    public static int researchPerWave = 1;
    public static int spawnedResearch = 0;
    public static int chanceToSpawnResearch;

	void Start () {
		cur = this;
        UpdateAmountModifier ();
		EndWave ();
		AddFinalBoss ();
	}

    public static void AddEnemy (Enemy enemy) {
        cur.spawnedEnemies.Add (enemy);
    }

    void FixedUpdate () {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
        mousePos.z = 0;


        // Enemy movement code:
        for (int i = 0; i < spawnedEnemies.Count; i++) {
            if (spawnedEnemies[i] && spawnedEnemies[i].gameObject.activeSelf) {

                Enemy enemy = spawnedEnemies[i];
                // Movement code.
                if (enemy.pathIndex == enemy.path.Length - 1 || enemy.isFlying) {
                    enemy.transform.position += Vector3.down * Time.fixedDeltaTime * enemy.speed;
                    if (enemy.rotateSprite)
                        enemy.transform.rotation = Quaternion.Euler (0, 0, 270);
                    continue;
                }
                Vector3 loc = new Vector3 (enemy.path[enemy.pathIndex].x, enemy.path[enemy.pathIndex].y) + enemy.offset;
                float dist = Vector3.Distance (enemy.transform.position, loc);

                if (dist < enemy.speed * Time.fixedDeltaTime * 2f) {
                    enemy.pathIndex++;
                }

                enemy.transform.position = Vector3.MoveTowards (enemy.transform.position, loc, enemy.speed * Time.fixedDeltaTime * enemy.freezeMultiplier);

                if (enemy.rotateSprite)
                    enemy.transform.rotation = Quaternion.Lerp (enemy.transform.rotation, Quaternion.Euler (0, 0, Angle.CalculateAngle (enemy.transform.position, loc)), 0.2f);

                if (enemy.freezeMultiplier < 1f) {
                    enemy.freezeMultiplier += 0.5f * Time.fixedDeltaTime;
                } else {
                    enemy.freezeMultiplier = 1f;
                }

                //Healthslider Code
                if (enemy.healthSlider) {
                    if (enemy.healthSlider.transform.parent != enemy.transform) {  
                        enemy.healthSlider.value = enemy.health;
                        enemy.healthSlider.transform.position = enemy.transform.position + Vector3.up;

                        if (Vector3.Distance (enemy.transform.position, mousePos) > 5f) {
                            if (enemy.healthSlider.transform.parent != enemy.transform) {
                                enemy.healthSlider.transform.SetParent (enemy.transform);
                                ///enemy.healthSlider.gameObject.SetActive (false);
                            }
                        }
                    } else if (Vector3.Distance (enemy.transform.position, mousePos) < 5f) {
                        enemy.healthSlider.transform.SetParent (Game.game.worldCanvas.transform);
                        enemy.healthSlider.transform.rotation = Quaternion.identity;
                        //enemy.healthSlider.gameObject.SetActive (true);
                    }
                }
            }
        }
    }

    void UpdateAmountModifier () {
        amountModifier = waveMastery;
        if (Game.game.gamemode == Gamemode.GlassEnemies) {
            amountModifier = (int)((float)waveMastery * 10f);
        } else if (Game.game.gamemode == Gamemode.TitaniumEnemies) {
            amountModifier = (int)((float)waveMastery * 0.1f);
        }
    }

    private IEnumerator CleanEnemyArray () {
        int destroyPerTick = Mathf.CeilToInt ((float)spawnedEnemies.Count / readyWaitTime * Time.fixedDeltaTime);

        for (int i = 0; i < spawnedEnemies.Count; i++) {
            Destroy (spawnedEnemies[i].gameObject);
            if (i % destroyPerTick == 0)
                yield return new WaitForFixedUpdate ();
        }

        spawnedEnemies.Clear ();
    }

	void AddFinalBoss () {
		// What the fuck is this shit?
		Wave.Enemy e = new Wave.Enemy ();
		e.enemy = endBoss;
		e.spawnAmount = 1;
		Wave.Subwave s = new Wave.Subwave ();
		s.enemies.Add (e);
		s.spawnTime = 1f;
		Wave w = new Wave ();
		w.subwaves.Add (s);
		waves.Add (w);
	}

	void Poop () {
		// Hø hø hø hø..
		waveNumber++;
		UpdateUpcomingWaveScreen (waves [waveNumber]);
		Invoke ("Poop", 1f);
	}

	// TODO: Replace wavePrebbing and waveStarted with enums

	public IEnumerator PoolBaddies () {
		Wave cur = waves [waveNumber - 1];
		Queue<Wave.Enemy> spawnQueue = new Queue<Wave.Enemy>();
		float startTime = Time.time;

		currentEnemies = 0;
		int index = -1;
		foreach (Wave.Subwave sub in cur.subwaves) {
			foreach (Wave.Enemy ene in sub.enemies) {
				index++;
				ene.index = index;

				spawnQueue.Enqueue (ene);

				SplitterEnemySplit split = ene.enemy.GetComponent<SplitterEnemySplit>();
				if (split) currentEnemies += ene.spawnAmount * split.spawnPos.Length * amountModifier;
				currentEnemies += ene.spawnAmount * amountModifier;
			}
		}

		int spawnPerTick = Mathf.CeilToInt ((float)currentEnemies / readyWaitTime * Time.fixedDeltaTime);
        List<Enemy> toArray = new List<Enemy> ();

		index = 0;
		while (spawnQueue.Count > 0) {
			for (int i = 0; i < spawnQueue.Peek ().spawnAmount * amountModifier; i++) {
				GameObject newEne = (GameObject)Instantiate (spawnQueue.Peek ().enemy, enemyPool.position, Quaternion.identity);
                toArray.Add (newEne.GetComponent<Enemy> ());

				newEne.SetActive (false);
				newEne.transform.parent = enemyPool;

				Enemy e = newEne.GetComponent<Enemy>();
				e.upcomingElement = upcomingElements[spawnQueue.Peek ().index];

				if (!pooledEnemies.ContainsKey (spawnQueue.Peek ())) {
					pooledEnemies.Add (spawnQueue.Peek (), new List<GameObject>());
				}
				pooledEnemies[spawnQueue.Peek ()].Add (newEne);
				index++;


				if (index >= spawnPerTick) {
					yield return new WaitForFixedUpdate ();
					index = 0;
				}
			}
			spawnQueue.Dequeue ();
		}

        spawnedEnemies = toArray;
        chanceToSpawnResearch = currentEnemies;
		Invoke ("StartWave", readyWaitTime - (Time.time - startTime));
		yield return null;
	}

	public void ReadyWave () {
		if (!waveStarted && !wavePrebbing) {
            waveStartedIndicator.GetComponentInParent<HoverContextElement> ().text = "Initializing...";
            HoverContextElement.activeElement = null;

            waveNumber++;
			wavePrebbing = true;
			waveStartedIndicator.color = Color.yellow;
			waveCounterIndicator.text = "Wave: Initialzing..";
            spawnedResearch = 0;
			Pathfinding.BakePaths ();
		}else if (waveStarted) {
            Game.ToggleFastGameSpeed ();
        }
	}

	public void OnEnemyDeath () {
		currentEnemies--;
		if (currentEnemies < 1) {
			EndWave ();

			if (waveNumber >= waves.Count) {
				if (waveMastery == 1) {
					gameOverIndicator.SetActive (true);
				}else{
					ContinueMastery ();
				}
			}
		}
	}

	public void ContinueMastery () {
		waveNumber = 0;
		waveMastery *= 2;
        UpdateAmountModifier ();
		gameOverIndicator.SetActive (false);
        UpdateUpcomingWaveScreen (waves[waveNumber]);
	}

	void UpdateUpcomingWaveScreen (Wave upcoming) {

		for (int i = 0; i < upcomingContent.Count; i++) {
			Destroy (upcomingContent [i]);
		}

		for (int i = 0; i < upcomingElements.Count; i++) {
			Destroy (upcomingElements [i]);
		}

		upcomingElements.Clear ();

		int sIndex = 0;
		int eIndex = 0;

		foreach (Wave.Subwave sub in upcoming.subwaves) {

			Vector3 sepPos = Vector3.down * (4 + eIndex * buttonSize) + Vector3.down * sIndex * seperatorSize;
			GameObject newSep = (GameObject)Instantiate (upcomingSeperatorPrefab, sepPos, Quaternion.identity);
			newSep.transform.SetParent (upcomingCanvas, false);
			upcomingContent.Add (newSep);
			sIndex++;
			foreach (Wave.Enemy ene in sub.enemies) {

				RectTransform rt = upcomingEnemyPrefab.GetComponent<RectTransform>();
				Vector3 enePos = new Vector3 (-rt.sizeDelta.x ,-rt.sizeDelta.y, 0) / 2 + Vector3.down * sIndex * seperatorSize + Vector3.down * eIndex * buttonSize + Vector3.right * 45;
				GameObject newEne = (GameObject)Instantiate (upcomingEnemyPrefab, enePos, Quaternion.identity);
				newEne.transform.SetParent (upcomingCanvas, false);
				upcomingContent.Add (newEne);

				newEne.transform.FindChild ("Image").GetComponent<Image>().sprite = ene.enemy.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
				Text text = newEne.transform.FindChild ("Amount").GetComponent<Text>();

				upcomingElements.Add (UpcomingElement.CreateInstance <UpcomingElement>());
				upcomingElements[upcomingElements.Count - 1].upcomingText = text;
				upcomingElements[upcomingElements.Count - 1].remaining = ene.spawnAmount * amountModifier + 1;
				upcomingElements[upcomingElements.Count - 1].Decrease ();

				eIndex++;
			}
		}

		upcomingWindow.sizeDelta = new Vector2 (upcomingWindow.sizeDelta.x, sIndex * seperatorSize + eIndex * buttonSize + buttonSize);
		upcomingWindow.position = new Vector3 (upcomingWindow.position.x, Screen.height - windowPosY - upcomingWindow.sizeDelta.y / 2);
	}

	public void StartWave () {
		if (waveNumber <= waves.Count) {

            waveStartedIndicator.GetComponentInParent<HoverContextElement> ().text = "Speed up the game";
            HoverContextElement.activeElement = null;

            wavePrebbing = false;
			waveStarted = true;
			waveStartedIndicator.color = Color.red;
			waveCounterIndicator.text = "Wave: " + waveNumber.ToString ();
			gameProgress *= gameProgressSpeed;
			ContinueWave (true);
		}
	}

	void ContinueWave (bool first) {

		endedIndex = 0;
		if (!first)
			subwaveNumber++;

		if (waves [waveNumber - 1].subwaves.Count > subwaveNumber) {
			currentSubwave = waves [waveNumber - 1].subwaves [subwaveNumber];
			spawnIndex = new int[currentSubwave.enemies.Count];

			for (int i = 0; i < currentSubwave.enemies.Count; i++) {
				Invoke ("Spawn" + i.ToString (), 0f);
			}
			//Invoke ("ContinueFalseWave", currentSubwave.spawnTime + 2f);
		}
	}

	void ContinueFalseWave () {
		ContinueWave (false);
	}

	public void EndWave () {
        StartCoroutine (CleanEnemyArray ());

		waveStarted = false;
		currentSubwave = null;
		subwaveNumber = 0;
        if (Game.fastGame)
            Game.ToggleFastGameSpeed ();
        waveStartedIndicator.color = Color.green;
		Game.credits += 25 * waveNumber;
		if (waves.Count >= waveNumber + 1) {
			UpdateUpcomingWaveScreen (waves [waveNumber]);
		}
        waveStartedIndicator.GetComponentInParent<HoverContextElement>().text = "Start wave " + (waveNumber + 1).ToString ();
        HoverContextElement.activeElement = null;
	}

	EnemySpawnPoint GetSpawnPosition () {
		return Game.game.enemySpawnPoints[Random.Range (0, Game.game.enemySpawnPoints.Count)];
	}

	void CreateEnemy (int index) {
		Wave.Enemy enemy = currentSubwave.enemies[index];
		GameObject e = pooledEnemies [enemy] [0];
		e.SetActive (true);

		Enemy ene = e.GetComponent<Enemy>();
		ene.spawnPoint = GetSpawnPosition ();
		ene.transform.position = ene.spawnPoint.worldPosition;
		ene.path = ene.spawnPoint.path;


		pooledEnemies [enemy].RemoveAt (0);
		spawnIndex[index]++;

		if (spawnIndex[index] < currentSubwave.enemies[index].spawnAmount * amountModifier) {
			Invoke ("Spawn" + index.ToString (), currentSubwave.spawnTime / ((float)currentSubwave.enemies[index].spawnAmount * amountModifier));
		}else{
			endedIndex++;
			if (endedIndex == spawnIndex.Length) {
				ContinueWave (false);
			}
		}
	}

	public void SaveWaveset (Wave[] waves, string name) {
		string path = Game.WAVESET_SAVE_DIRECTORY + name + WAVESET_FILE_EXTENSION;
        StreamWriter write = new StreamWriter (path, false);

		write.WriteLine ("PROJECT VIRUS WAVE SET FILE, EDIT WITH CAUTION");
		write.WriteLine (name);

		foreach (Wave wave in waves) {
			write.WriteLine ("\twave:");
			foreach (Wave.Subwave subwave in wave.subwaves) {
				write.WriteLine ("\t\tsptm:" + subwave.spawnTime.ToString ());
				write.WriteLine ("\t\tenms:");
				foreach (Wave.Enemy enemy in subwave.enemies) {
					write.WriteLine ("\t\t\tenmy:" + enemy.enemy.name);
					write.WriteLine ("\t\t\tamnt:" + enemy.spawnAmount.ToString ());
				}
			}
		}

		write.WriteLine ("END OF FILE");
		write.Close ();
	}

	public static List<Wave> LoadWaveset (string name) {
		string path = Game.WAVESET_SAVE_DIRECTORY + name + WAVESET_FILE_EXTENSION;
		string[] content = ModuleAssemblyLoader.GetContents (path);

		List<Wave> locWaves = new List<Wave> ();

		Wave cw = null;
		Wave.Subwave cs = null;
		Wave.Enemy ce = null;

		for (int i = 0; i < content.Length; i++) {
			string c = content [i];

			// Find wave
			if (c.Length > 4) {
				if (c.Substring (0,5) == "\twave") {
					cw = new Wave ();
					locWaves.Add (cw);
				}
			}

			// Find and read subwave
			if (c.Length > 5) {
				if (c.Substring (0,6) == "\t\tsptm") {
					cs = new Wave.Subwave ();
					cs.spawnTime = float.Parse (c.Substring (7));
					cw.subwaves.Add (cs);
				}
			}

			// Find and read enemy
			if (c.Length > 6) {
				if (c.Substring (0,7) == "\t\t\tenmy") {
					ce =  new Wave.Enemy ();
					ce.enemy = EnemyManager.cur.GetEnemyFromName (c.Substring (8));
				}

				if (c.Substring (0,7) == "\t\t\tamnt") {
					ce.spawnAmount = int.Parse (c.Substring (8));
					cs.enemies.Add (ce);
				}
			}
		}

		return locWaves;
	}

	GameObject GetEnemyFromName (string n) {
		foreach (GameObject obj in enemyTypes) {
			if (obj.name == n) {
				return obj;
			}
		}

		return null;
	}

    void OnDrawGizmos () {
        if (waveStarted) {
            for (int i = 0; i < Game.game.enemySpawnPoints.Count; i++) {
                EnemySpawnPoint point = Game.game.enemySpawnPoints[i];
                for (int j = 0; j < point.path.Length - 1; j++) {
                    Debug.DrawLine (point.path[j], point.path[j + 1], Color.red);
                }
            }
        }
    }

	void Spawn0 () {
		int index = 0;
		CreateEnemy (index);
	}
	void Spawn1 () {
		int index = 1;
		CreateEnemy (index);
	}
	void Spawn2 () {
		int index = 2;
		CreateEnemy (index);
	}
	void Spawn3 () {
		int index = 3;
		CreateEnemy (index);
	}
	void Spawn4 () {
		int index = 4;
		CreateEnemy (index);
	}
	void Spawn5 () {
		int index = 5;
		CreateEnemy (index);
	}
	void Spawn6 () {
		int index = 6;
		CreateEnemy (index);
	}
	void Spawn7 () {
		int index = 7;
		CreateEnemy (index);
	}
}