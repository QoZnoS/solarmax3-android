using System;
using UnityEngine;

public class ParticleSystemReset : MonoBehaviour
{
	private void Start()
	{
		this.effect = base.transform.GetComponent<ParticleSystem>();
		if (this.effect != null)
		{
			this.orgDelay = this.effect.main.startDelay.constant;
		}
	}

	private void OnEnable()
	{
		if (this.effect != null)
		{
			this.effect.main.startDelay = this.orgDelay;
		}
	}

	private ParticleSystem effect;

	private float orgDelay;
}
