using UnityEngine;

// Lets the level-select panel use spare vertical space on taller screens.
// The panel's height and position are driven by menu animations that only write while
// they play, so this adds a constant extra on top: when an animation has just written the
// size, the full extra is applied once; otherwise only the change in extra (window resized)
// is applied. The panel grows evenly up and down; the cap keeps the header clear of the logo.
[RequireComponent(typeof(RectTransform))]
public class LevelSelectHeight : MonoBehaviour
{
	[Tooltip("Fraction of the canvas height the panel should occupy when there is room")]
	public float targetFraction = 0.62f;

	[Tooltip("Never grow beyond this many canvas units in total")]
	public float maxHeight = 820.0f;

	private RectTransform rect;
	private RectTransform canvasRect;
	private float applied = 0.0f;
	private float lastHeight = float.NaN;

	void Awake()
	{
		rect = GetComponent<RectTransform>();
		var canvas = GetComponentInParent<Canvas>();
		canvasRect = canvas != null ? canvas.rootCanvas.GetComponent<RectTransform>() : null;
	}

	void LateUpdate()
	{
		if (canvasRect == null)
			return;

		float height = rect.sizeDelta.y;
		bool animatorWrote = float.IsNaN(lastHeight) || !Mathf.Approximately(height, lastHeight);

		// Extra relative to the base (animated) height, which is at most ~700
		float baseHeight = animatorWrote ? height : height - applied;
		// The panel's units are not the canvas's units, so convert through world space
		float availableHeight = canvasRect.rect.height * canvasRect.lossyScale.y / rect.lossyScale.y;
		float desired = Mathf.Clamp(availableHeight * targetFraction, baseHeight, maxHeight);
		float extra = Mathf.Max(0.0f, desired - baseHeight);

		float delta = animatorWrote ? extra : extra - applied;
		if (!Mathf.Approximately(delta, 0.0f))
		{
			var size = rect.sizeDelta;
			size.y += delta;
			rect.sizeDelta = size;
		}

		applied = extra;
		lastHeight = rect.sizeDelta.y;
	}
}
