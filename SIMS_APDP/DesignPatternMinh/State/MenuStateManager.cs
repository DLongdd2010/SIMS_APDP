// DesignPatternMinh/State/MenuStateManager.cs
namespace SIMS_APDP.DesignPatternMinh.State
{
    public class MenuStateManager
    {
        public string CurrentPage { get; private set; } = "Dashboard";

        public void SetActive(string page)
        {
            CurrentPage = page;
        }
    }
}