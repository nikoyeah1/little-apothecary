using UnityEngine;

public interface IInteractable
{
    string GetInteractLabel();

    string GetDescription();

    void NotifyLookedAt();

    void NotifyLookedAway();

    void Interact(GameObject player);
}
