namespace pdbget
{
    internal interface IAction<TOptions>
    {
        int Setup(TOptions options);
        int Run();
    }
}
