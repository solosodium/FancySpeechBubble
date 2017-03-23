using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[RequireComponent(typeof(ContentSizeFitter))]
public class FancySpeechBubble : MonoBehaviour {

	/// <summary>
	/// Character start font size.
	/// </summary>
	public int characterStartSize = 1;

	/// <summary>
	/// Character size animate speed.
	/// Unit: delta font size / second
	/// </summary>
	public float characterAnimateSpeed = 1000f;

	/// <summary>
	/// The bubble background (OPTIONAL).
	/// </summary>
	public Image bubbleBackground;

	/// <summary>
	/// Minimum height of background.
	/// </summary>
	public float backgroundMinimumHeight;

	/// <summary>
	/// Vertical margin (top + bottom) between label and background (OPTIONAL).
	/// </summary>
	public float backgroundVerticalMargin;

	/// <summary>
	/// A copy of raw text.
	/// </summary>
	private string _rawText;
	public string rawText {
		get { return _rawText; }
	}

	/// <summary>
	/// Processed version of raw text.
	/// </summary>
	private string _processedText;
	public string processedText {
		get { return _processedText; }
	}

	/// <summary>
	/// Set the label text.
	/// </summary>
	/// <param name="text">Text.</param>
	public void Set (string text) {
		StopAllCoroutines();
		StartCoroutine(SetRoutine(text));
	}	

	/// <summary>
	/// Set the label text.
	/// </summary>
	/// <param name="text">Text.</param>
	public IEnumerator SetRoutine (string text) 
	{
		_rawText = text;
		yield return StartCoroutine(TestFit());
		yield return StartCoroutine(CharacterAnimation());
	}

	/// <summary>
	/// Test fit candidate text,
	/// set intended label height,
	/// generate processed version of the text.
	/// </summary>
	private IEnumerator TestFit () 
	{
		// prepare targets
		Text label = GetComponent<Text>();
		ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();

		// change label alpha to zero to hide test fit
		float alpha = label.color.a;
		label.color = new Color(label.color.r, label.color.g, label.color.b, 0f);

		// configure fitter and set label text so label can auto resize height
		fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		label.text = _rawText;

		// need to wait for a frame before label's height is updated
		yield return new WaitForEndOfFrame();
		// make sure label is anchored to center to measure the correct height
		float totalHeight = label.rectTransform.sizeDelta.y;

		// (OPTIONAL) set bubble background
		if (bubbleBackground != null) {
			bubbleBackground.rectTransform.sizeDelta = new Vector2(
				bubbleBackground.rectTransform.sizeDelta.x, 
				Mathf.Max(totalHeight + backgroundVerticalMargin, backgroundMinimumHeight));
		}

		// now it's time to test word by word
		_processedText = "";
		string buffer = "";
		string line = "";
		float currentHeight = -1f;
		// yes, sorry multiple spaces
		foreach (string word in _rawText.Split(' ')) {
			buffer += word + " ";
			label.text = buffer;
			yield return new WaitForEndOfFrame();
			if (currentHeight < 0f) {
				currentHeight = label.rectTransform.sizeDelta.y;
			}
			if (currentHeight != label.rectTransform.sizeDelta.y) {
				currentHeight = label.rectTransform.sizeDelta.y;
				_processedText += line.TrimEnd(' ') + "\n";
				line = "";
			}
			line += word + " ";
		}
		_processedText += line;

		// prepare fitter and label for character animation
		fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
		label.text = "";
		label.rectTransform.sizeDelta = new Vector2(label.rectTransform.sizeDelta.x, totalHeight);
		label.color = new Color(label.color.r, label.color.g, label.color.b, alpha);
	}

	private IEnumerator CharacterAnimation () 
	{
		// prepare target
		Text label = GetComponent<Text>();

		// go through character in processed text
		string prefix = "";
		foreach (char c in _processedText.ToCharArray()) {
			// animate character size
			int size = characterStartSize;
			while (size < label.fontSize) {
				size += (int)(Time.deltaTime * characterAnimateSpeed);
				size = Mathf.Min(size, label.fontSize);
				label.text = prefix + "<size=" + size + ">" + c + "</size>";
				yield return new WaitForEndOfFrame();
			}
			prefix += c;
		}

		// set processed text
		label.text = _processedText;
	}

}
