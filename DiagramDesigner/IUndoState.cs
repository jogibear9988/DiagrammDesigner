namespace DiagramDesigner
{
    internal interface IUndoState<T>
    {
        T State { get; }
    }
}
