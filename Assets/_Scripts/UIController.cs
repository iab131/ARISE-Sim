using System.Collections;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private NavBarController navBar;
    [SerializeField] private BlockCodeExecutor blockCodeExecutor;

    public void OnRunSimClicked()
    {
        StartCoroutine(SwitchAndRun());
    }

    private IEnumerator SwitchAndRun()
    {
        // 1. Switch to Sim view (this disables the block coding panel)
        navBar.ShowSimulation();
        // 2. Wait a frame so Unity completes enabling/disabling
        yield return new WaitForSeconds(1f);

        // 3. Run the code (BlockCodeExecutor should now reference the sim robot)
        blockCodeExecutor.OnPlay();
    }
}
