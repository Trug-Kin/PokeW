using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;
    [SerializeField] float postDialogDelay = 1.0f;
    [SerializeField] Color highlightedColor;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;


    [SerializeField] List<Text> actionText;
    [SerializeField] List<Text> moveText;


    [SerializeField] Text ppText;
    [SerializeField] Text typeText;



   
    public void SetDialog(string dialog)
    {
        if (dialogText == null)
        {
            Debug.LogError($"BattleDialogBox '{name}': dialogText is not assigned in inspector.");
            return;
        }

        dialogText.text = dialog;
    }
    public IEnumerator TypeDialog(string dialog)
    {
        if (dialogText == null)
        {
            Debug.LogError($"BattleDialogBox '{name}': dialogText is not assigned in inspector.");
            yield break;
        }

        if (string.IsNullOrEmpty(dialog))
        {
            dialogText.text = "";
            yield break;
        }

        // if lettersPerSecond is invalid, show whole dialog immediately
        if (lettersPerSecond <= 0)
        {
            dialogText.text = dialog;
            // still wait a bit so callers have time to read
            if (postDialogDelay > 0f)
                yield return new WaitForSeconds(postDialogDelay);
            yield break;
        }

        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / (float)lettersPerSecond);
        }

        // small pause after the full dialog is displayed so messages don't overlap
        if (postDialogDelay > 0f)
            yield return new WaitForSeconds(postDialogDelay);

    }
    public void EnableDialogText(bool enabled)
    { 
    
        dialogText.enabled = enabled;

    }
    public void EnableActionSelector(bool enabled)
    {
    actionSelector.SetActive(enabled);
          
    }
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionText.Count; i++)
        {
            if (i == selectedAction) 
                actionText[i].color = highlightedColor;
            else
                actionText[i].color = Color.black;
        }
    }
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveText.Count; i++)
        {
            if (i == selectedMove)
                moveText[i].color = highlightedColor;
            else
                moveText[i].color = Color.black;
        }
        if (move == null)
        {
            ppText.text = "";
            typeText.text = "";
        }
        else
        {
            ppText.text = $"PP {move.PP}/{move.Base.PP}";
            typeText.text = move.Base.Type.ToString();
        }
    }

    public void SetMoveNames(List<Move>move)
    { 
        // Ensure UI list is assigned
        if (moveText == null || moveText.Count == 0)
        {
            Debug.LogWarning("BattleDialogBox: moveText is not assigned or empty in inspector.");
            return;
        }

        // defend against null input (move list may not be initialized yet)
        for (int i = 0; i < moveText.Count; i++)
        {
            var textSlot = moveText[i];
            if (textSlot == null)
                continue;

            if (move == null || i >= move.Count || move[i] == null || move[i].Base == null)
            {
                textSlot.text = "-";
            }
            else
            {
                textSlot.text = move[i].Base.Name;
            }
        }


    }

}

