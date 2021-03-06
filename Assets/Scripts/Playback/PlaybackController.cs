﻿using System;
using UnityEngine;
using UnityEngine.Video;

public class PlaybackController : MonoBehaviour
{
	[SerializeField] VideoPlayer player;
	[SerializeField] Material material;
	[SerializeField] BranchCreator branchCreator;
	[SerializeField] Credits credits;
	[SerializeField] End endPanel;
	[SerializeField] ControllerVisiblity controllerVisible;
	[SerializeField] Pointer[] pointers;

	[SerializeField, Header("最初に再生するビデオ")] Video firstVideo;
	Video currentVideo;

	[HideInInspector] public NextIs currentNextIs;

	void Start()
	{
		material.SetColor("_Tint", Color.black);

		player.prepareCompleted += source => material.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f, 0.5f));

		player.loopPointReached += OnVideoEnd;
	}

	void OnVideoEnd(VideoPlayer source)
	{
		switch (currentNextIs)
		{
			case NextIs.Video:
				branchCreator.Create(currentVideo.sentence, currentVideo.branches, currentVideo.currentBranchSize);
				if (currentVideo.overrideSoundOnBranch)
				{
					BackgroundSound.Play(currentVideo.overrideSoundOnBranch);
				}
				else
				{
					BackgroundSound.Play(Situation.Intermission);
				}

				break;

			case NextIs.Credits:
				StartCoroutine(credits.Play());
				BackgroundSound.Play(Situation.End);
				break;

			case NextIs.End:
				endPanel.Show();
				BackgroundSound.Play(Situation.End);
				break;
		}

		foreach (var pointer in pointers) pointer.SetRunningState(true);
		controllerVisible.ChangeAlpha(1f);
	}

	public void PlayFirstVideo()
	{
		Play(firstVideo.clip, firstVideo.nextIs, firstVideo);
		BackgroundSound.Stop();
	}

	public void Play(VideoClip clip, NextIs nextIs, Video video)
	{
		material.SetColor("_Tint", Color.black);

		// ステレオ判定
		var layout = clip.name.EndsWith("_TB", StringComparison.OrdinalIgnoreCase) ? 2 : 0;
		material.SetFloat("_Layout", layout);

		material.SetFloat("_Rotation", Mathf.Repeat(video.rotationOffset + 90f, 360f));

		player.clip = clip;
		player.Play();
		BackgroundSound.Stop();
		Debug.Log("Playing " + clip.name);

		foreach (var pointer in pointers) pointer.SetRunningState(false);
		controllerVisible.ChangeAlpha(0f);
		currentNextIs = nextIs;
		currentVideo = video;
	}

	public void SetRotation(float rotation)
	{
		// Panoramic シェーダーのデフォルトオフセット90度を勘案
		material.SetFloat("_Rotation", Mathf.Repeat(rotation + 90f, 360f));
		Debug.Log("Set Rotation to " + Mathf.Repeat(rotation + 90f, 360f));
	}

#if UNITY_EDITOR
	Color initialColor;

	void Awake()
	{
		initialColor = material.GetColor("_Tint");
	}

	void OnApplicationQuit()
	{
		material.SetColor("_Tint", initialColor);
	}

	void Update()
	{
		if (player.isPlaying && Input.GetKeyDown(KeyCode.Return))
		{
			player.time = player.clip.length;
		}
	}
#endif
}