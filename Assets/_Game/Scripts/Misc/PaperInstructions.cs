using UnityEngine;
using Ouiki.FPS;
using Ouiki.Interfaces;
using Ouiki.UI;
using Zenject;

namespace Ouiki.Items
{
    public class PaperInstructions : MonoBehaviour, ILabel, IInteractable
    {
        [Header("Paper Content")]
        [SerializeField] private string documentTitle = "Manager's Note";
        [Inject] private ReadableCanvasManager readableCanvasManager; 

        public string Label => "Instructions Paper";
        public bool IsInteractable => true;
        public string ActionName => "[E] Read Instructions";

        [TextArea(3, 12)]
        [SerializeField] string instructionsText =
@"Welcome to your first shift at Café Eclipse.

I had to step out tonight, so you'll handle things alone.
Don’t worry—just serve the customers coffee.
Some prefer it... stronger. You'll know when you see them.

Keep the café clean, stay polite, and no matter what happens,
do not look outside when the lights flicker.

— Controls —
W / A / S / D  — Move
Mouse          — Look
E              — Interact
Space          — Jump
Left Shift     — Sprint
C      — Crouch
Right Mouse    — Zoom / Focus

If something feels wrong, Run.
You’ll understand soon enough.";

        public void OnInteract(PlayerInteractionController controller)
        {
            if (readableCanvasManager != null)
                readableCanvasManager.ShowReadable(documentTitle, instructionsText, OnPaperClosed);
        }

        public void SetInteractable(bool interactable) { }

        private void OnPaperClosed()
        {
            gameObject.SetActive(false);
        }
    }
}