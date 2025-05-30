﻿/******************************************************************************************************************************************************
* MIT License																																		  *
*																																					  *
* Copyright (c) 2024																																  *
* Emmanuel Badier <emmanuel.badier@gmail.com>																										  *
* 																																					  *
* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),  *
* to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,  *
* and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:		  *
* 																																					  *
* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.					  *
* 																																					  *
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, *
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 																							  *
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 		  *
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.							  *
******************************************************************************************************************************************************/

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

	public void Init(string pName, string pText, float pTextSize, Color pTextColor, Vector3 pTextOffset, Transform pBillboardCameraTransform)
	{
		name = pName;
		_debugText.text = pText;
		_debugText.characterSize = pTextSize;
		_debugText.color = pTextColor;
		// Cached
		_debugTextTransform = _debugText.transform;
		_debugTextTransform.localPosition = pTextOffset;
		_billboardCameraTransform = pBillboardCameraTransform;
	}

	public void SetDebugText(string pText)
    {
		_debugText.text = pText;
	}

	public void SetDebugTextSize(float pSize)
    {
		_debugText.characterSize = pSize;
	}

	public void SetDebugTextActive(bool pActive)
	{
		_debugText.gameObject.SetActive(pActive);
		enabled = pActive;
	}

	public void SetDebugModelActive(bool pActive)
    {
		_debugModel.gameObject.SetActive(pActive);
	}

	private void Update()
	{
		if (_billboardCameraTransform != null)
		{
			_debugTextTransform.rotation = _billboardCameraTransform.rotation;
		}
	}
}