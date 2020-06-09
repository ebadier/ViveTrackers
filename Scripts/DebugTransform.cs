using UnityEngine;

public sealed class DebugTransform : MonoBehaviour
{
	[SerializeField]
	private TextMesh _debugText;
	[SerializeField]
	private GameObject _debugModel;

	// Cached
	private Transform _debugTextTransform = null;
	private Transform _billboardCameraTransform = null;

	public void Init(string pName, Color pColor, Transform pBillboardCameraTransform)
	{
		name = pName;
		_debugText.text = pName;
		_debugText.color = pColor;
		// Cached
		_debugTextTransform = _debugText.transform;
		_billboardCameraTransform = pBillboardCameraTransform;
	}

	public void SetDebugActive(bool pActive)
	{
		_debugText.gameObject.SetActive(pActive);
		_debugModel.gameObject.SetActive(pActive);
		enabled = pActive;
	}

	private void Update()
	{
		if (_billboardCameraTransform != null)
		{
			_debugTextTransform.rotation = _billboardCameraTransform.rotation;
		}
	}
}