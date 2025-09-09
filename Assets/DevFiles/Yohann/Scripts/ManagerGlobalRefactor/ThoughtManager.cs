using UnityEngine;
using TMPro;
using System.Collections;

// THOUGHT MANAGER
// a singleton MonoBehaviour that manages the display of "thought" messages in the UI.
// It shows a thought bubble with animated appearance, keeps it visible for a set duration, then fades it out.
// The ShowThought method triggers the display, and ClearCurrentThought hides and resets the thought UI.
// The script uses a coroutine to handle the animation and timing, and tracks the sender of the current thought.
public class ThoughtManager : MonoBehaviour
{
    public static ThoughtManager Instance;

    [SerializeField] private GameObject goThought;
    [SerializeField] private CanvasGroup cgThought;
    [SerializeField] private TextMeshProUGUI txtThought;

    private const float THOUGHT_TIMER_MAX = 3f;
    private Coroutine corThoughtTimer;
    private GameObject thoughtSender;

    private void Awake()
    {
        Instance = this;
        goThought.SetActive(false);
    }

    public void ShowThought(GameObject sender, string str)
    {
        thoughtSender = sender;
        txtThought.text = str;

        if (corThoughtTimer != null)
            StopCoroutine(corThoughtTimer);

        corThoughtTimer = StartCoroutine(IE_ShowThought());
    }

    private IEnumerator IE_ShowThought()
    {
        goThought.SetActive(true);
        cgThought.alpha = 1;

        // animate in
        float duration = 0f;
        while (duration < 0.5f)
        {
            goThought.transform.localScale = Vector3.Lerp(Vector3.forward, new Vector3(0.01f, 0.01f, 1), duration / 0.5f);
            goThought.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0, 0.25f, 0.67f), duration / 0.5f);

            duration += Time.deltaTime;
            yield return null;
        }

        // wait
        yield return new WaitForSeconds(THOUGHT_TIMER_MAX);

        float timer = THOUGHT_TIMER_MAX;
        while (timer > 0)
        {
            cgThought.alpha = Mathf.Lerp(0, 1, timer / THOUGHT_TIMER_MAX);
            timer -= Time.deltaTime;
            yield return null;
        }

        ClearCurrentThought();
    }

    public void ClearCurrentThought()
    {
        if (corThoughtTimer != null)
        {
            StopCoroutine(corThoughtTimer);
            corThoughtTimer = null;
        }

        goThought.SetActive(false);
        txtThought.text = "Hmmm...";
        thoughtSender = null;
    }

    public GameObject CurrentSender => thoughtSender;
}
