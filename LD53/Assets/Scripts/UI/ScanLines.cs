// Modified from https://github.com/aaaleee/UnityScanlinesEffect

using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Camera/Scan Lines")]
public class ScanLines : MonoBehaviour
{
	public Shader shader;

	[Range(0, 10)]
	public float lineWidth = 2f;

	[Range(0, 1)]
	public float hardness = 0.9f;

	[Range(0, 1)]
	public float displacementSpeed = 0.1f;

	private Material _material;
	protected Material material
	{
		get
		{
			if (_material == null) _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
			return _material;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (shader == null) return;
		material.SetFloat("_LineWidth", lineWidth);
		material.SetFloat("_Hardness", hardness);
		material.SetFloat("_Speed", displacementSpeed);
		Graphics.Blit(source, destination, material, 0);
	}

	void OnDisable()
	{
		if (_material != null) DestroyImmediate(_material);
	}
}