using System.Collections.Generic;
using CreativeSpore.SuperTilemapEditor;
using UnityEngine;

public class EloMapActor : EMono
{
	public EloMap elomap;

	public Transform transMap;

	public Transform transLight;

	public Transform moldLight;

	public Tileset tileset;

	public EloMapTileSelector selector;

	public List<EloMapLight> lights = new List<EloMapLight>();

	public bool IsActive => base.gameObject.activeSelf;

	private void Awake()
	{
		this.SetActive(enable: false);
	}

	public void Initialize(EloMap _elomap)
	{
		elomap = _elomap;
		if (!transMap)
		{
			transMap = Util.Instantiate<Transform>(elomap.idMap);
			transLight = Object.Instantiate(moldLight);
			lights.Clear();
			transMap.SetActive(enable: false);
			transLight.SetActive(enable: false);
			EMono.scene.screenElona.SetUnitSize();
		}
		if (!elomap.initialized)
		{
			elomap.Init(this);
		}
		STETilemap fogmap = elomap.fogmap;
		transMap.position = new Vector3((float)(-fogmap.MinGridX) * EMono.scene.screenElona.tileAlign.x + EMono.scene.screenElona.actorPos.x, (float)(-fogmap.MinGridY) * EMono.scene.screenElona.tileAlign.y + EMono.scene.screenElona.actorPos.y, EMono.scene.screenElona.actorPos.z);
	}

	public void OnActivate()
	{
		transLight.SetActive(enable: true);
		foreach (EloMapLight light in lights)
		{
			light.sr.transform.position = TilemapUtils.GetGridWorldPos(elomap.fogmap, light.gx, light.gy);
		}
		OnChangeHour();
		elomap.objmap.UpdateMesh();
	}

	public void OnDeactivate()
	{
		if ((bool)transMap)
		{
			transMap.SetActive(enable: false);
		}
		if ((bool)transLight)
		{
			transLight.SetActive(enable: false);
		}
	}

	public void OnChangeHour()
	{
		Color white = Color.white;
		white.a = EMono.scene.profile.light.orbitAlphaCurve.Evaluate(EMono.scene.timeRatio);
		white.a *= white.a;
		foreach (EloMapLight light in lights)
		{
			light.sr.color = white;
		}
	}

	public EloMap GetEloMap()
	{
		Initialize(EMono.world.region.elomap);
		return elomap;
	}

	public void OnKillGame()
	{
		this.SetActive(enable: false);
		if ((bool)transMap)
		{
			transLight.SetActive(enable: false);
			transMap.SetActive(enable: false);
		}
		if ((bool)transMap)
		{
			Object.DestroyImmediate(transMap.gameObject);
			Object.DestroyImmediate(transLight.gameObject);
			lights.Clear();
		}
	}
}
