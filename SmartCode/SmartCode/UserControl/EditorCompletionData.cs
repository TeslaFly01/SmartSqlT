using System;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace SmartCode.UserControl
{
    public class EditorCompletionData : ICompletionData
    {
        public string Text { get; private set; }

        public EditorCompletionData(string text)
        {
            Text = text;
        }

        public object Content
        {
            get { return Text; }
        }

        public object Description
        {
            get { return null; }
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public double Priority
        {
            get { return 1; }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
