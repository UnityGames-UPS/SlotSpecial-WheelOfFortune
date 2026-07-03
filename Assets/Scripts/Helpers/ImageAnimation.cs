using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
	public enum ImageState
	{
		NONE,
		PLAYING,
		PAUSED
	}

	public static ImageAnimation Instance;

	public List<Sprite> textureArray;

	public Image rendererDelegate;

	public bool useSharedMaterial = true;

	public bool doLoopAnimation = true;

	[HideInInspector]
	public ImageState currentAnimationState;

	private int indexOfTexture;

	private float idealFrameRate = 0.0416666679f;

	private float delayBetweenAnimation;

	public float AnimationSpeed = 5f;

	public float delayBetweenLoop;

	public bool StartOnAwake = false;

	public bool DestroyOnCompletion = false;

	[SerializeField] internal bool isplaying;

	[SerializeField]
	private Sprite OriginalSprite;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	private void Start()
	{
		OriginalSprite = rendererDelegate.sprite;
	}

	private void OnEnable()
	{
		if (StartOnAwake)
		{
			StartAnimation();
		}
	}

	private void OnDisable()
	{
		//rendererDelegate.sprite = textureArray[0];
		StopAnimation();
	}

	private void AnimationProcess()
	{
		isplaying = true;
		SetTextureOfIndex();
		indexOfTexture++;
		if (indexOfTexture == textureArray.Count)
		{
			indexOfTexture = 0;
			if (doLoopAnimation)
			{
				Invoke("AnimationProcess", delayBetweenAnimation + delayBetweenLoop);
				isplaying = true;
			}
			else
			{
				if (DestroyOnCompletion)
				{
					this.gameObject.SetActive(false);
				}
				// A non-looping animation has completed a full pass. Reset the state so
				// StartAnimation() can replay it on the next spin, and clear isplaying so
				// external listeners can detect completion.
				currentAnimationState = ImageState.NONE;
				isplaying = false;
			}
		}
		else
		{
			Invoke("AnimationProcess", delayBetweenAnimation);
			isplaying = true;

		}
	}

	internal void StartAnimation()
	{
		if (textureArray == null || textureArray.Count == 0)
		{
			Debug.LogWarning($"{name} StartAnimation skipped: textureArray is empty");
			return;
		}

		indexOfTexture = 0;
		if (currentAnimationState == ImageState.NONE)
		{
			RevertToInitialState();
			delayBetweenAnimation = idealFrameRate * textureArray.Count / AnimationSpeed;
			currentAnimationState = ImageState.PLAYING;
			// Mark as playing immediately so a caller polling isplaying right after
			// starting doesn't mistake the pre-first-frame gap for completion.
			isplaying = true;
			Invoke("AnimationProcess", delayBetweenAnimation);
		}
	}


	internal void PauseAnimation()
	{
		if (currentAnimationState == ImageState.PLAYING)
		{
			CancelInvoke("AnimationProcess");
			currentAnimationState = ImageState.PAUSED;
		}
	}

	internal void ResumeAnimation()
	{
		if (currentAnimationState == ImageState.PAUSED && !IsInvoking("AnimationProcess"))
		{
			Invoke("AnimationProcess", delayBetweenAnimation);
			currentAnimationState = ImageState.PLAYING;
		}
	}

	internal void StopAnimation()
	{
		if (currentAnimationState != 0)
		{
			rendererDelegate.sprite = textureArray[0];
			CancelInvoke("AnimationProcess");
			currentAnimationState = ImageState.NONE;
			isplaying = false;
		}
	}

	internal void RevertToInitialState()
	{
		indexOfTexture = 0;
		SetTextureOfIndex();
	}

	// Restores the sprite that was present when this object started (captured in Start),
	// e.g. a default background frame, rather than the animation's first frame.
	internal void RestoreOriginalSprite()
	{
		if (rendererDelegate != null && OriginalSprite != null)
			rendererDelegate.sprite = OriginalSprite;
	}

	// Approximate seconds left until a currently playing (non-looping) animation finishes.
	// Returns 0 when not playing. Lets callers time other actions to end alongside it.
	internal float RemainingSeconds
	{
		get
		{
			if (currentAnimationState != ImageState.PLAYING || textureArray == null || textureArray.Count == 0)
				return 0f;
			int remainingFrames = Mathf.Max(0, textureArray.Count - indexOfTexture);
			return remainingFrames * delayBetweenAnimation;
		}
	}

	private void SetTextureOfIndex()
	{
		if (textureArray == null || textureArray.Count == 0)
			return;

		if (indexOfTexture < 0 || indexOfTexture >= textureArray.Count)
		{
			indexOfTexture = 0;
		}

		rendererDelegate.sprite = textureArray[indexOfTexture];
	}

}
