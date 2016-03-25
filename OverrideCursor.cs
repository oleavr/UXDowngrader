using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace UXDowngrader
{
    public class OverrideCursor : IDisposable
    {
        static Stack<Cursor> cursorStack = new Stack<Cursor>();

        public OverrideCursor(Cursor changeToCursor)
        {
            cursorStack.Push(changeToCursor);

            if (Mouse.OverrideCursor != changeToCursor)
                Mouse.OverrideCursor = changeToCursor;
        }

        public void Dispose()
        {
            cursorStack.Pop();

            Cursor cursor = cursorStack.Count > 0 ? cursorStack.Peek() : null;

            if (cursor != Mouse.OverrideCursor)
                Mouse.OverrideCursor = cursor;
        }
    }
}
