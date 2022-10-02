namespace GlobalInputHook.IPC
{
    public enum EExitCode
    {
        Normal = 0,
        InvalidParentProcessID,
        InvalidMapArgument,
        InvalidMaxUpdateRateArgument,
        ParentProcessExited
    }
}
