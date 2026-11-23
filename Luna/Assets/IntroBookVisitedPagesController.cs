using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroBookVisitedPagesController : MonoBehaviour
{
    [Header("Current Scene's Spread Controller")]
    [SerializeField] private IntroBookSpreadSequenceController spreadController;

    [Header("All Book Page Prefab Assets (Across All Scenes)")]
    [SerializeField] private List<BookPagePrefab> pagePrefabs = new List<BookPagePrefab>();

    [Header("Parent for Instantiated Visited Pages")]
    [SerializeField] private Transform pageParent;

    [Header("Navigation")]
    [SerializeField] private KeyCode nextKey1 = KeyCode.RightArrow;
    [SerializeField] private KeyCode nextKey2 = KeyCode.D;
    [SerializeField] private KeyCode previousKey1 = KeyCode.LeftArrow;
    [SerializeField] private KeyCode previousKey2 = KeyCode.A;

    private readonly List<BookPagePrefab> history = new List<BookPagePrefab>();
    private BookPagePrefab currentInstance;
    private int historyIndex = -1;
    private bool transitioning;
    private float nextAllowedTurn;

    private void Update()
    {
        if (spreadController == null || !spreadController.ViewingHistory ||
            transitioning || Time.unscaledTime < nextAllowedTurn)
            return;

        if (Input.GetKeyDown(nextKey1) || Input.GetKeyDown(nextKey2))
            Next();
        else if (Input.GetKeyDown(previousKey1) || Input.GetKeyDown(previousKey2))
            Previous();
    }

    public void EnterHistory()
    {
        if (transitioning || spreadController == null ||
            spreadController.ViewingHistory || spreadController.IsBusy)
            return;

        RebuildHistory();
        if (history.Count == 0)
            return;

        StartCoroutine(EnterRoutine());
    }

    private void RebuildHistory()
    {
        history.Clear();
        Dictionary<string, BookPagePrefab> lookup =
            new Dictionary<string, BookPagePrefab>();

        foreach (BookPagePrefab prefab in pagePrefabs)
        {
            if (prefab == null) continue;
            if (!lookup.ContainsKey(prefab.name))
                lookup.Add(prefab.name, prefab);
            else
                Debug.LogWarning("[VisitedPages] Duplicate prefab name: " + prefab.name, this);
        }

        foreach (string id in BookPageHistory.VisitedPages)
        {
            // Current-scene pages are navigated by the regular spread controller.
            if (spreadController.IsCurrentScenePage(id)) continue;

            if (lookup.TryGetValue(id, out BookPagePrefab prefab))
                history.Add(prefab);
            else
                Debug.LogWarning("[VisitedPages] No prefab registered for saved page: " + id, this);
        }
    }

    private IEnumerator EnterRoutine()
    {
        transitioning = true;
        spreadController.EnterHistoryView();
        while (!spreadController.ViewingHistory)
            yield return null;

        historyIndex = history.Count - 1;
        ShowHistoryPage(historyIndex);
        FinishTurn();
    }

    private void Next()
    {
        if (historyIndex >= history.Count - 1)
            StartCoroutine(ReturnToCurrentScene());
        else
            StartCoroutine(TurnHistoryPage(historyIndex + 1));
    }

    private void Previous()
    {
        if (historyIndex > 0)
            StartCoroutine(TurnHistoryPage(historyIndex - 1));
    }

    private IEnumerator TurnHistoryPage(int index)
    {
        transitioning = true;
        spreadController.PlayHistoryPageTurnSound();
        yield return FadeOutCurrentInstance();
        ClearCurrentInstance();
        historyIndex = index;
        ShowHistoryPage(historyIndex);
        FinishTurn();
    }

    private IEnumerator ReturnToCurrentScene()
    {
        transitioning = true;
        spreadController.PlayHistoryPageTurnSound();
        yield return FadeOutCurrentInstance();
        ClearCurrentInstance();
        historyIndex = -1;
        spreadController.ReturnFromHistory();
        FinishTurn();
    }

    private void ShowHistoryPage(int index)
    {
        if (index < 0 || index >= history.Count) return;
        BookPagePrefab prefab = history[index];
        currentInstance = Instantiate(prefab, pageParent != null ? pageParent : transform);
        currentInstance.gameObject.SetActive(true);
        spreadController.PlayPrefabArtwork(currentInstance);
    }

    private IEnumerator FadeOutCurrentInstance()
    {
        if (currentInstance == null) yield break;

        spreadController.StopPrefabArtwork();
        Graphic[] graphics = currentInstance.GetComponentsInChildren<Graphic>(true);
        Color[] starting = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
            starting[i] = graphics[i].color;

        float duration = Mathf.Max(0f, spreadController.PageTurnFadeDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed = Mathf.Min(duration, elapsed + Time.unscaledDeltaTime);
            float t = duration <= 0f ? 1f : elapsed / duration;
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] == null) continue;
                Color color = starting[i];
                color.a = Mathf.Lerp(starting[i].a, 0f, t);
                graphics[i].color = color;
            }
            yield return null;
        }
    }

    private void ClearCurrentInstance()
    {
        if (currentInstance != null)
            Destroy(currentInstance.gameObject);
        currentInstance = null;
    }

    private void FinishTurn()
    {
        nextAllowedTurn = Time.unscaledTime + spreadController.PageTurnCooldown;
        transitioning = false;
    }
}