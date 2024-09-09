public interface IState
{
    public void EnterState()
    {
        //Code to run when entering state
    }

    public void UpdateState()
    {
        //Code to run while in current state
    }

    public void ExitState()
    {
        //Code to run when exiting state
    }
}
