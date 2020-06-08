using UnityEngine;

public sealed class DebugTransform : MonoBehaviour
{
	[SerializeField]
	private TextMesh _debugText;
	[SerializeField]
	private GameObject _debugModel;

	// Cached
	private Transform _debugTextTransform;
	private Transform _billboardCameraTransform;

	public void Init(string pName, Color pColor, Transform pBillboardCameraTransform)
	{
		name = pName;
		_debugText.text = pName;
		_debugText.color = pColor;
		// Cached
		_debugTextTransform = _debugText.transform;
		_billboardCameraTransform = pBillboardCameraTransform;
	}

	public void SetActive(bool pActive)
	{
		gameObject.SetActive(pActive);
	}

	private void Update()
	{
		if (_billboardCameraTransform != null)
		{
			_debugTextTransform.rotation = _billboardCameraTransform.rotation;
		}
	}
}