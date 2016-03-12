﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ResearchEditor : EditorWindow {

	private ResearchMenu research;
	public Vector2 offset;

	private enum Action { Default, ChoosingPrerequisite, MovingResearch, CloningResearch };
	private enum WindowState { All, Research };

	private Action action = Action.Default;
	private WindowState state = WindowState.All;

	private Research focusResearch;

	[MenuItem ("Project Virus/Research")]
	public static void ShowWindow () {
		EditorWindow.GetWindow (typeof (ResearchEditor));
	}

	void DrawResearchButton (Vector3 pos, Research u) {
		Rect rect = new Rect (pos.x * 30 - offset.x + position.width/2, pos.y * 30 - offset.y + 10f, 20, 20);

		if (u != null) {
			if (u.sprite) {
				if (GUI.Button (rect, u.sprite.texture)) {
					SelectResearch (u);
				}
			}else{
				if (GUI.Button (rect, "")) {
					SelectResearch (u);
				}
			}

			if (u.prerequisite != -1) {
				Vector2 o = new Vector2 (position.width/2 + 10 + (u.x - u.GetPrerequisite ().x) * 15, 20);
				Drawing.DrawLine (
					new Vector2 (u.x, u.y) * 30 - offset + o
				  , new Vector2 (u.GetPrerequisite ().x, u.GetPrerequisite().y) * 30 - offset + o
				  , Color.black, 2, false);
			}
		}else{
			if (GUI.Button (rect, "")) {
				SelectEmptyButton (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y));
			}
		}
	}

	void SelectEmptyButton (int x, int y) {
		if (action == Action.MovingResearch) {
			focusResearch.x = x;
			focusResearch.y = y;
		}

		if (action == Action.CloningResearch) {
            Research r = new Research ();
			r.x = x;
			r.y = y;
			r.colour = focusResearch.colour;
			r.desc = focusResearch.desc;
			r.name = focusResearch.name;
			r.func = focusResearch.func;
			r.value = focusResearch.value;
			r.sprite = focusResearch.sprite;
            r.index = research.research.Count;
            r.prerequisite = -1;
			research.research.Add (r);
		}

		action = Action.Default;
	}
	
	void OnGUI () {
		if (!research) {
			GameObject r = GameObject.Find ("ResearchMenu");
			if (r) research = r.GetComponent<ResearchMenu>();
		}else{

			if (state == WindowState.All) {
				if (action != Action.MovingResearch) {

					offset.y = GUI.VerticalSlider (new Rect (position.width - 15, 5, 10, position.height - 10), offset.y, position.height, 0);
					if (GUI.Button (new Rect (position.width / 3, position.height - 25, position.width / 3, 20), "Add Research")) {
						research.research.Add (new Research ());
                        SelectResearch (research.research[research.research.Count-1]);
                        research.research[research.research.Count - 1].index = research.research.Count - 1;
                        research.research[research.research.Count - 1].prerequisite = -1;
                        state = WindowState.All;
                        action = Action.MovingResearch;
					}
					
					for (int i = 0; i < research.research.Count; i++) {
						
						Research u = research.research[i];
						Vector3 pos = research.GetPos (u);

						DrawResearchButton (pos, u);
					}

					for (int i = (int)(offset.y / 30); i < (int)(position.height / 30) + (int)(offset.y / 30); i++) {
						GUI.Label (new Rect (10, i * 30 + 10 - offset.y, 20, 20), i.ToString ()); 
					}
				}

				if (action == Action.MovingResearch || action == Action.CloningResearch) {

					// Dis part gunna be heavy and hacky.
					// I'm truely sorry you have to see this.

					// Create and populate unfound research.
					List<Research> unfound = new List<Research>();
					foreach (Research r in research.research) {
						unfound.Add (r);
					}

					// Foreach loops are major simple to work with, yay! :D

					for (int y = (int)(offset.y / 30); y < (int)(position.height / 30) + (int)(offset.y / 30); y++) {
						for (int x = (int)((offset.x - position.width/2) / 30); x < (int)((position.width/2) / 30); x++) {

							// Figure out if there is research at current position, search in unfound.
							Research atPos = null;
							foreach (Research r in unfound) {
								if (r.x == x && r.y == y) {
									atPos = r;
								}
							}

							if (atPos != null) {
								DrawResearchButton (new Vector3 (x,y), atPos);
								unfound.Remove (atPos);
							}else{
								DrawResearchButton (new Vector3 (x,y), null);
							}
						}
					}
				}
					
			}

			if (state == WindowState.Research) {
				if (focusResearch == null) {
					state = WindowState.All;
					action = Action.Default;
				}
				focusResearch.name = EditorGUILayout.TextField ("Name: ",focusResearch.name);
				focusResearch.desc = EditorGUILayout.TextArea (focusResearch.desc);
				focusResearch.func = EditorGUILayout.TextField ("Function: ",focusResearch.func);
				focusResearch.value = EditorGUILayout.IntField ("Value: ",focusResearch.value);
				focusResearch.sprite = (Sprite)EditorGUILayout.ObjectField ("Sprite: ",focusResearch.sprite, typeof (Sprite), false);
				focusResearch.colour = (Colour)EditorGUILayout.EnumPopup ("Colour: ", focusResearch.colour);
				if (focusResearch.prerequisite != -1) {
					if (GUILayout.Button ("Remove Prerequisite: " + focusResearch.GetPrerequisite ().name)) {
						Debug.Log ("Removing Prerequisitite");
						focusResearch.prerequisite = -1;
					}
				}else{
					if (GUILayout.Button ("Add Prerequisite")) {
						state = WindowState.All;
						action = Action.ChoosingPrerequisite;
					}
				}

				if (GUILayout.Button ("Move")) {
					state = WindowState.All;
					action = Action.MovingResearch;
				}

				if (GUILayout.Button ("Clone")) {
					state = WindowState.All;
					action = Action.CloningResearch;
				}

				if (GUILayout.Button ("Delete")) {
					research.research.Remove (focusResearch);
					state = WindowState.All;
					action = Action.Default;
					focusResearch = null;
				}

                if (GUILayout.Button ("Suggest Name")) {
                    focusResearch.name = SuggestTitle (focusResearch);
                }

				if (GUILayout.Button ("Return")) {
					state = WindowState.All;
					action = Action.Default;
					focusResearch = null;
				}
			}
		}
	}

    private string SuggestTitle (Research r) {
        switch (r.func) {
            case "UnlockModule":
                return "Research the " + research.unlockableModules[r.value].GetComponent<Module> ().moduleName + " module";

            case "UnlockSpecialModule":
                return "Research the specialized " + research.unlockableModules[r.value].GetComponent<Module> ().moduleName + " module";

            case "IncreaseFirerate":
                return "Increase " + r.colour.ToString ().ToLower () + " firerate by " + r.value.ToString () + "%";
            case "IncreaseDamage":
                return "Increase " + r.colour.ToString ().ToLower () + " damage by " + r.value.ToString () + "%";
            case "DecreaseCost":
                return "Decrease " + r.colour.ToString ().ToLower () + " cost by " + r.value.ToString () + "%";
            case "IncreaseRange":
                return "Increase base range by " + r.value.ToString () + "%";
            case "IncreaseTurnrate":
                return "Increase rotator turnrate by " + r.value.ToString () + "%";
            default:
                return r.name;
        }
    }

	void SelectResearch (Research u) {
		if (action == Action.Default)
			focusResearch = u;

		if (action == Action.ChoosingPrerequisite) {
			focusResearch.prerequisite = u.index;
			action = Action.Default;
		}

		state = WindowState.Research;
	}
}
