namespace DiagramDesigner
{
    public interface IUndoable<T>
    {
        bool CanRedo { get; }
        bool CanUndo { get; }
        T Value { get; set; }
        void SaveState();
        void Undo();
        void Redo();
    }
}
