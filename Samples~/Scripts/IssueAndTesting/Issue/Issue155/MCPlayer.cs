using System.Collections;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue41;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue62;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// fade to black
namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue155
{
	[DefaultExecutionOrder(-699999)]
	public class MCPlayer: SaintsMonoBehaviour
	{
		[GetComponent]
		public MCDamageHandler damageHandler;
		public enum MODE { LIMBO,PLAY }
		public MODE          mode;
		public Transform     head;
		// public MCRespawn     respawn;
		// public HVRScreenFade fade;
		// [GetComponentInChildren(true,typeof(HVRPlayerController))]
		public Transform body;
		public        GameObject limboUI;
		public static MCPlayer   instance;
		public static MCUnit     self;
		// public        SceneReference hubScene;
		// public UltEvent onFlatLine;

		void Awake()
		{
			instance = this;
			self     = GetComponent<MCUnit>();
		}

		void OnEnable()
		{
			// fade.UpdateFade(1);
			// fade.Fade(0,1);
			// respawn = FindObjectOfType<MCRespawn>();
		}

		// [PandaTask]
		void FadeToBlack(bool b = true)
		{
			// fade.Fade(b ? 1 : 0,1f);
			// PandaTask.Succeed();
		}

		// [PandaTask]
		// void FlatLine() => onFlatLine.Invoke();

		// [PandaTask]
		void BackToHub()
		{
			// if(PandaTask.isStarting) LoadScene(hubScene);
		}

		[Button]
		public void LoadScene(SceneReference scene) => StartCoroutine(LoadSceneC(scene));

		IEnumerator LoadSceneC(SceneReference scene)
		{
			var loadAsync = SceneManager.LoadSceneAsync(scene);
			loadAsync.allowSceneActivation = false;
			// fade.Fade(1,1);
			yield return new WaitForSeconds(2);
			loadAsync.allowSceneActivation = true;
		}

		// [PandaTask]
		void Respawn()
		{
			// var teleporter = FindObjectOfType<HVRTeleporter>(false);
			// var Grabber    = FindObjectOfType<HVRHandGrabber>();
			// Grabber.ForceRelease();
			// teleporter.Teleport(respawn.transform.position,respawn.transform.forward);
			// damageHandler._currentHealth = damageHandler.InitialHealth;
			// PandaTask.Succeed();
		}

		//todo display skill tree etc...
		IEnumerator Skill()
		{
			yield return null;
		}

		// [PandaTask]
		void ToggleMode(MODE m) => mode = m;

		// [PandaTask]
		bool IsMode(MODE m) => m==mode;

		// [PandaTask]
		void ShowLimboUI(bool b)
		{
			if(limboUI) limboUI.SetActive(b);
		}
	}
}
