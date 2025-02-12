using System.Collections.Generic;
using UnityEngine;
using NoAlloq;
using System;
using System.Diagnostics;
using _Project.Scripts.GlobalHandlers;

/**
 * Environment Query Instance
 */
public class EnvQuery : MonoBehaviour
{
	public enum EnvQueryGeneratorType
	{
		OnCircle,
		SimpleGrid
	}

	public EnvQueryItem BestResult { get; private set; }

	public EnvQueryGeneratorType GeneratorType = EnvQueryGeneratorType.OnCircle;
	public GameObject CenterOfItems;
	public float Radius = 4.0f;
	public float SpaceBetween = 1.0f;
	public List<EnvQueryTest> EnvQueryTests = new List<EnvQueryTest>();

	private GameObject querier;
	private EnvQueryGenerator generator;
	private List<EnvQueryItem> envQueryItems;
	private List<EnvQueryItem> envQueryItemsBacking;

    [SerializeField]
    private int maxWorkMiliseconds = 2;

    [SerializeField]
    private bool startNextStepImmediately = false;

    [HideInInspector]
    public EQSStatus QueryStatus;

    public Action ProgressQuery;
	private int index;
	private Stopwatch stopwatch = new Stopwatch();
    private float minScore;
	private float maxScore;

	void Start()
	{
		if(querier == null)
		{
			querier = gameObject;
		}
		if(CenterOfItems == null)
		{
			CenterOfItems = querier;
		}

		if(GeneratorType == EnvQueryGeneratorType.OnCircle)
		{
			generator = new EnvQueryGeneratorOnCircle(Radius, SpaceBetween);
		}
		else if(GeneratorType == EnvQueryGeneratorType.SimpleGrid)
		{
			generator = new EnvQueryGeneratorSimpleGrid(Radius, SpaceBetween);
		}

		if(CenterOfItems != null && generator != null)
		{
			envQueryItems = generator.GenerateItems(EnvQueryTests.Count, CenterOfItems.transform);
		}
		else
		{
			envQueryItems = new List<EnvQueryItem>();
		}

		envQueryItemsBacking = envQueryItems.GetRange(0, envQueryItems.Count);
	}

    //void Update()
    //{
		//RunEQSQuery();
    //}

	public void PrepareQuery()
	{
		QueryStatus = EQSStatus.Running;
        index = 0;
		ProgressQuery = new Action(ResetScoreWithTimeGuard);
    }

	public void ResetScoreWithTimeGuard()
	{
		stopwatch.Restart();

        while (index < envQueryItems.Count && stopwatch.ElapsedMilliseconds < maxWorkMiliseconds)
		{
			envQueryItems[index++].Score = 0f;
        }

		stopwatch.Stop();

		if(index >= envQueryItems.Count)
		{
			index = 0;
			ProgressQuery = new Action(UpdateNavMeshProjectionWithTimeGuard);

			if (startNextStepImmediately) UpdateNavMeshProjectionWithTimeGuard();
		}
	}

	private void UpdateNavMeshProjectionWithTimeGuard()
	{
        stopwatch.Restart();

        while (index < envQueryItems.Count && stopwatch.ElapsedMilliseconds < maxWorkMiliseconds)
        {
            envQueryItems[index++].UpdateNavMeshProjection();
        }

		stopwatch.Stop();

        if (index >= envQueryItems.Count)
        {
            index = 0;
            ProgressQuery = new Action(RunTestsWithTimeGuard);

            if (startNextStepImmediately) RunTestsWithTimeGuard();
        }
    }

	private void RunTestsWithTimeGuard()
	{
        stopwatch.Restart();

        while (index < EnvQueryTests.Count && stopwatch.ElapsedMilliseconds < maxWorkMiliseconds)
        {
            EnvQueryTests[index].RunTest(index, envQueryItems);
            EnvQueryTests[index].NormalizeItemScores(index, envQueryItems);
			index += 1;
        }

		stopwatch.Stop();

        if (index >= EnvQueryTests.Count)
        {
            if (envQueryItems == null || envQueryItems.Count < 1)
            {
                QueryStatus = EQSStatus.Finished;
				return;
            }

            index = 0;
            ProgressQuery = new Action(FindMinMaxWithTimeGuard);

            maxScore = envQueryItems[0].Score;
            minScore = envQueryItems[0].Score;
            BestResult = envQueryItems[0];

            if (startNextStepImmediately) FindMinMaxWithTimeGuard();
        }
    }

    private void FindMinMaxWithTimeGuard()
	{
        stopwatch.Restart();

        while (index < envQueryItems.Count && stopwatch.ElapsedMilliseconds < maxWorkMiliseconds)
        {
            if (envQueryItems[index].Score > maxScore)
            {
                maxScore = envQueryItems[index].Score;
            }
            if (envQueryItems[index].Score < minScore)
            {
                minScore = envQueryItems[index].Score;
            }
            index += 1;
        }

		stopwatch.Stop();

        if (index >= envQueryItems.Count)
        {
            index = 0;

            if (maxScore != minScore)
			{
                ProgressQuery = new Action(NormalizeScoreWithTimeGuard);
            }
			else
			{
                ProgressQuery = new Action(FindBestResult);
            }

            if (startNextStepImmediately) ProgressQuery();
        }
    }

    private void NormalizeScoreWithTimeGuard()
	{
        stopwatch.Restart();

        while (index < envQueryItems.Count && stopwatch.ElapsedMilliseconds < maxWorkMiliseconds)
        {
			envQueryItems[index].Score = (envQueryItems[index].Score - minScore) / (maxScore - minScore);
            index += 1;
        }

		stopwatch.Stop();

		if (index >= envQueryItems.Count)
		{
			index = 0;
            ProgressQuery = new Action(FindBestResult);

            if (startNextStepImmediately) FindBestResult();
        }
    }

	private void FindBestResult()
	{
        //BestResult = envQueryItems
        //                .AsSpan()
        //                .Where(x => x.IsValid)
        //                .OrderByDescending(envQueryItemsBacking.AsSpan(), x => x.Score)
        //                .FirstOrDefault();

        stopwatch.Restart();

        while (index < envQueryItems.Count && stopwatch.ElapsedMilliseconds < maxWorkMiliseconds)
        {
			if(envQueryItems[index].IsValid)
			{
				if(envQueryItems[index].Score > BestResult.Score)
				{
					BestResult = envQueryItems[index];
                }
			}
            index += 1;
        }

        stopwatch.Stop();

		if (index >= envQueryItems.Count)
		{
			index = 0;
			QueryStatus = EQSStatus.Finished;

			if (!BestResult.IsValid) BestResult = null;
        }
    }

    public void RunEQSQuery()
	{
		ResetScore();

        foreach (EnvQueryItem item in envQueryItems)
        {
            item.UpdateNavMeshProjection();
        }

        for (int currentTest = 0; currentTest < EnvQueryTests.Count; currentTest++)
        {
            EnvQueryTests[currentTest].RunTest(currentTest, envQueryItems);
            EnvQueryTests[currentTest].NormalizeItemScores(currentTest, envQueryItems);
        }

        NormalizeScore();
        FindBestResult();
    }

	private void ResetScore()
	{
		foreach(EnvQueryItem item in envQueryItems)
		{
			item.Score = 0.0f;
		}
	}

	private void NormalizeScore()
	{
        if(envQueryItems == null || envQueryItems.Count < 1)
        {
            return;
        }

		maxScore = envQueryItems[0].Score;
		minScore = envQueryItems[0].Score;

		foreach(EnvQueryItem item in envQueryItems)
		{
			if(item.Score > maxScore)
			{
				maxScore = item.Score;
			}
			if(item.Score < minScore)
			{
				minScore = item.Score;
			}
		}

		if(maxScore != minScore)
		{
			foreach(EnvQueryItem item in envQueryItems)
			{
				item.Score = (item.Score - minScore) / (maxScore - minScore);
			}
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (!GlobalStartSettings.Instance.EnableEnemiesAIGizmos)
		{
			return;
		}
		
		if (isActiveAndEnabled && envQueryItems != null)
		{
			foreach (EnvQueryItem item in envQueryItems)
			{
				if (item.IsValid)
				{
					Gizmos.color = Color.HSVToRGB((item.Score / 2.0f), 1.0f, 1.0f);
					Gizmos.DrawWireSphere(item.GetWorldPosition(), 0.25f);
					UnityEditor.Handles.Label(item.GetWorldPosition(), item.Score.ToString());
				}
			}
		}
		if (isActiveAndEnabled && BestResult != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(BestResult.GetWorldPosition(), 0.25f);
		}
	}
#endif
}

public enum EQSStatus
{
	Running,
	Finished
}