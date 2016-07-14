﻿using UnityEngine;
using System.Collections;
using IngameEditors;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IngameEditors {

	public class AssemblyEditorScene : MonoBehaviour {

		public PurchaseMenu purchaseMenu;
		public float zoomSpeed;

        public Module rootModule;
        public bool continueSave;

        public GameObject saveMenu;
        public Text assemblyName;
        public Text assemblyDesc;

        public Text statsText;

        public static AssemblyEditorScene cur;

        // Use this for initialization
        void Awake () {
            cur = this;
			ResearchMenu.InitializeAllStatics ();
			Game.currentScene = Scene.AssemblyBuilder;
			Game.credits = int.MaxValue;
			purchaseMenu.standard = Resources.LoadAll<GameObject> ("Modules").ToList ();
			purchaseMenu.Initialize ();
		}

        void Start () {
            Game.UpdateDarkOverlay();
        }

        public void QuitScene () {
			Game.currentScene = Scene.Play;
            SceneManager.LoadScene ("pv_menu");
		}

		void Update () {
			Camera.main.orthographicSize = Mathf.Lerp (Camera.main.orthographicSize, 
			                                           Camera.main.orthographicSize + zoomSpeed * Input.GetAxis ("Scroll Wheel") * Time.deltaTime,
			                                           -zoomSpeed * Time.deltaTime);

            if (Input.GetMouseButtonDown (1)) {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
                ray.direction *= -PlayerInput.cur.camDepth * 2f;

                if (Physics.Raycast (ray, out hit, -PlayerInput.cur.camDepth, Game.game.moduleLayer)) {
                    Module hitModule = hit.collider.gameObject.GetComponent<Module> ();
                    if (hitModule) {
                        hitModule.DestroyModule ();
                    }
                }
            }
        }

        void FixedUpdate () {
            if (rootModule) {
                statsText.text =
                    "Cost: " + rootModule.GetAssemblyCost () + "\n\n" +
                    "DPS: " + rootModule.GetAssemblyDPS () + "\n\n" +
                    "Range: " + rootModule.GetRange () + "\n\n" +
                    "Turnspeed: " + rootModule.GetAssemblyAVGTurnSpeed ();
            }else {
                statsText.text = "Please place a module to start.";
            }
        }

        public void SaveAssembly () {
            Game.UpdateDarkOverlay();
            saveMenu.SetActive(true);
            continueSave = false;
            StartCoroutine(SAVE());
        }

        public void LoadAssembly () {
            Game.UpdateDarkOverlay ();
            FileBrowser.OpenFileBrowser (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY, gameObject, "OnLoadAssembly");
        }

        void OnFileBrowserClosed () {
            Game.UpdateDarkOverlay ();
        }

        public void OnLoadAssembly (string file) {
            if (rootModule)
                rootModule.DestroyModule ();

            for (int i = 0; i < Game.currentModules.Count; i++) {
                if (Game.currentModules[i].isRoot) {
                    Game.currentModules[i].DestroyModule ();
                }
            }

            Assembly ass = Assembly.LoadFromFile (file, true);

            ModuleAssemblyLoader loader = (Instantiate (purchaseMenu.assemblyLoader)).GetComponent<ModuleAssemblyLoader> ();
            GameObject root = loader.LoadAssembly (ass, true);
            root.transform.position = new Vector3 (0, 0);
            root.transform.rotation = Quaternion.Euler (0, 0, 90);
            root.transform.parent = null;

            rootModule = root.GetComponent<Module> ();
            for (int i = 0; i < rootModule.modules.Count; i++) {
                rootModule.modules[i].isOnBattlefield = true;
            }

        }

        public void ContinueSave () {
            Game.UpdateDarkOverlay();
            saveMenu.SetActive(false);
            continueSave = true;
        }

        IEnumerator SAVE () {
            while (!continueSave) {
                yield return new WaitForEndOfFrame();
            }

            rootModule.assemblyName = assemblyName.text;
            rootModule.assemblyDesc = assemblyDesc.text;
            rootModule.SaveModuleAssembly(rootModule.assemblyName);
        }
    }
}