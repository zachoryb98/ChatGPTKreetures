using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class BattleUnit : MonoBehaviour
{	
	[SerializeField] bool isPlayerUnit;
	[SerializeField] BattleHud hud;
	public VisualEffect levelUpVFX;

	public bool IsPlayerUnit
	{
		get { return isPlayerUnit; }
	}

	public BattleHud Hud
	{
		get { return hud;  }
	}

	[SerializeField] GameObject KreetureGameObject;

	[Header("Where to place the Kreetures")]
	public Transform playerSpawnPosition;
	public Transform enemySpawnPosition;

	public Kreeture Kreeture { get; set; }
	GameObject kreetureModel;

	

	public void Setup(Kreeture kreeture)
	{
		Kreeture = kreeture;

		kreetureModel = Kreeture.Base.Model;
		
		hud.gameObject.SetActive(true);

		// Ensure the model is not null
		if (kreetureModel != null)
		{
			if (isPlayerUnit)
			{
				KreetureGameObject = Instantiate(Kreeture.Base.Model, playerSpawnPosition.position, Quaternion.identity); ;

				//BattleManager.Instance.SetKreetureGameObject(KreetureGameObject);				
			}
			else
			{				
				KreetureGameObject = Instantiate(Kreeture.Base.Model, enemySpawnPosition.position, Quaternion.Euler(0f, 180f, 0f));

				//BattleManager.Instance.SetEnemyKreetureGameObject(EnemyKreetureGameObject);
			}

			levelUpVFX = KreetureGameObject.transform.Find("vfxLevelUp").GetComponent<VisualEffect>();
			levelUpVFX.gameObject.SetActive(false);

			hud.SetData(kreeture);

			PlayEnterAnimation();
		}
	}

	public void Clear()
	{
		hud.gameObject.SetActive(false);
	}

	public void PlayEnterAnimation()
	{
		//Throw Out Kreeture
	}

	public void PlayAttackAnimation()
	{
		Animator animator = KreetureGameObject.GetComponent<Animator>();
		animator.SetTrigger("SetAttackTrigger");
	}

	public void PlayLevelUpAnimation()
	{
		Animator animator = KreetureGameObject.GetComponent<Animator>();
		levelUpVFX.gameObject.SetActive(true);
		levelUpVFX.Play();
		animator.SetTrigger("SetLevelUpTrigger");
	}

	public void PlayHitAnimation()
	{
		Animator animator = KreetureGameObject.GetComponent<Animator>();
		animator.SetTrigger("SetHitTrigger");
	}

	public void PlayFaintAnimation()
	{
		Animator animator = KreetureGameObject.GetComponent<Animator>();
		animator.SetTrigger("SetFaintTrigger");
	}

	public void DestroyFaintedModel()
	{
		DestroyImmediate(KreetureGameObject, true);
	}

	public void PlayCaptureAnimation()
	{
		//For now do nothing, player throws ball
	}

	public void PlayBreakOutAnimation()
	{
		//For now do nothing, Kreeture Breaks out
	}
}
