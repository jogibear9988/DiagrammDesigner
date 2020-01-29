using System;
using System.Collections.Generic;

namespace DiagramDesigner
{
    public class Undoable<T> : IUndoable<T>
    {
        Stack<IUndoState<T>> _redoStack;
        Stack<IUndoState<T>> _undoStack;
        T _value;

        public Undoable(T value)
        {
            _value = value;
            _redoStack = new Stack<IUndoState<T>>();
            _undoStack = new Stack<IUndoState<T>>();
        }

        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool CanRedo
        {
            get { return _redoStack.Count != 0; }
        }

        public bool CanUndo
        {
            get { return _undoStack.Count != 0; }
        }

        public void SaveState()
        {
            _redoStack.Clear();
            _undoStack.Push(GenerateUndoState());
        }

        public void Undo()
        {
            if (_undoStack.Count == 0) throw new InvalidOperationException("Undo history exhausted");

            _redoStack.Push(GenerateUndoState());
            _value = _undoStack.Pop().State;
        }

        private UndoState<T> GenerateUndoState()
        {
            return new UndoState<T>(Value);
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) throw new InvalidOperationException("Redo history exhausted");

            _undoStack.Push(GenerateUndoState());
            _value = _redoStack.Pop().State;
        }
    }
}
