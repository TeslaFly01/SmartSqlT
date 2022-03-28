using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using System.Xml;
using SmartCode.Helper;

namespace SmartCode.UserControl
{
    [TemplatePart(Name = TextEditorTemplateName, Type = typeof(TextEditor))]
    public class TextEditorEx : Control
    {
        private static readonly Type _typeofSelf = typeof(TextEditorEx);

        private const string TextEditorTemplateName = "PART_TextEditor";

        private TextEditor _partTextEditor = null;

        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null;

        private CompletionWindow _completionWindow = null;
        private SearchPanel _searchPanel = null;

        private DispatcherTimer _timer = null;
        private bool _disabledTimer = false;

        public string Text
        {
            get
            {
                if (_partTextEditor != null)
                    return _partTextEditor.Text;

                return string.Empty;
            }
            set
            {
                if (_partTextEditor != null)
                {
                    _disabledTimer = true;
                    _partTextEditor.Text = value;
                }
            }
        }

        public bool CanRedo
        {
            get { return _partTextEditor != null && _partTextEditor.CanRedo; }
        }

        public bool CanUndo
        {
            get { return _partTextEditor != null && _partTextEditor.CanUndo; }
        }

        static TextEditorEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public TextEditorEx()
        {
            _foldingStrategy = new XmlFoldingStrategy() { ShowAttributesWhenFolded = true };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Math.Max(1, Delay)) };
            _timer.Tick += _timer_Tick;
        }

        #region RouteEvent

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventArgs), _typeofSelf);
        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        public static readonly RoutedEvent DelayArrivedEvent = EventManager.RegisterRoutedEvent("DelayArrived", RoutingStrategy.Bubble, typeof(RoutedEventArgs), _typeofSelf);
        public event RoutedEventHandler DelayArrived
        {
            add { AddHandler(DelayArrivedEvent, value); }
            remove { RemoveHandler(DelayArrivedEvent, value); }
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty SyntaxHighlightingProperty = DependencyProperty.Register("SyntaxHighlighting", typeof(IHighlightingDefinition), _typeofSelf);
        [TypeConverter(typeof(HighlightingDefinitionTypeConverter))]
        public IHighlightingDefinition SyntaxHighlighting
        {
            get { return (IHighlightingDefinition)GetValue(SyntaxHighlightingProperty); }
            set { SetValue(SyntaxHighlightingProperty, value); }
        }

        public static readonly DependencyProperty AllowSearchProperty = DependencyProperty.Register("AllowSearch", typeof(bool), _typeofSelf, new PropertyMetadata(OnAllowSearchPropertyChanged));
        public bool AllowSearch
        {
            get { return (bool)GetValue(AllowSearchProperty); }
            set { SetValue(AllowSearchProperty, value); }
        }

        private static void OnAllowSearchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var allowSearch = (bool)e.NewValue;

            if (allowSearch)
            {
                ctrl.InstallSearchPanel();
                ctrl.InitSearchPanel();
            }
            else
                ctrl.UninstallSearchPanel();
        }

        public static readonly DependencyProperty AllowFoldingProperty = DependencyProperty.Register("AllowFolding", typeof(bool), _typeofSelf, new PropertyMetadata(true, OnAllowFoldingPropertyChanged));
        public bool AllowFolding
        {
            get { return (bool)GetValue(AllowFoldingProperty); }
            set { SetValue(AllowFoldingProperty, value); }
        }

        private static void OnAllowFoldingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var allowFolding = (bool)e.NewValue;

            if (allowFolding)
                ctrl.InstallFolding();
            else
                ctrl.UninstallFolding();
        }

        public static readonly DependencyProperty LinePositionProperty = DependencyProperty.Register("LinePosition", typeof(int), _typeofSelf, new PropertyMetadata(OnLinePositionPropertyChanged));
        public int LinePosition
        {
            get { return (int)GetValue(LinePositionProperty); }
            set { SetValue(LinePositionProperty, value); }
        }

        private static void OnLinePositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var pos = (int)e.NewValue;

            ctrl._partTextEditor.TextArea.Caret.Column = pos;
        }

        public static readonly DependencyProperty LineNumberProperty = DependencyProperty.Register("LineNumber", typeof(int), _typeofSelf, new PropertyMetadata(OnLineNumberPropertyChanged));
        public int LineNumber
        {
            get { return (int)GetValue(LineNumberProperty); }
            set { SetValue(LineNumberProperty, value); }
        }

        private static void OnLineNumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var line = (int)e.NewValue;

            ctrl._partTextEditor.TextArea.Caret.Line = line;
        }

        public static readonly DependencyProperty IsMatchCaseProperty = DependencyProperty.Register("IsMatchCase", typeof(bool), _typeofSelf, new PropertyMetadata(OnIsMatchCasePropertyChanged));
        public bool IsMatchCase
        {
            get { return (bool)GetValue(IsMatchCaseProperty); }
            set { SetValue(IsMatchCaseProperty, value); }
        }

        private static void OnIsMatchCasePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var isMatchCase = (bool)e.NewValue;

            if (ctrl._searchPanel != null)
                ctrl._searchPanel.MatchCase = isMatchCase;
        }

        public static readonly DependencyProperty IsWholeWordsProperty = DependencyProperty.Register("IsWholeWords", typeof(bool), _typeofSelf, new PropertyMetadata(OnIsWholeWordsPropertyChanged));
        public bool IsWholeWords
        {
            get { return (bool)GetValue(IsWholeWordsProperty); }
            set { SetValue(IsWholeWordsProperty, value); }
        }

        private static void OnIsWholeWordsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var isWholeWords = (bool)e.NewValue;

            if (ctrl._searchPanel != null)
                ctrl._searchPanel.WholeWords = isWholeWords;
        }

        public static readonly DependencyProperty UseRegexProperty = DependencyProperty.Register("UseRegex", typeof(bool), _typeofSelf, new PropertyMetadata(OnUseRegexPropertyChanged));
        public bool UseRegex
        {
            get { return (bool)GetValue(UseRegexProperty); }
            set { SetValue(UseRegexProperty, value); }
        }

        private static void OnUseRegexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var useRegex = (bool)e.NewValue;

            if (ctrl._searchPanel != null)
                ctrl._searchPanel.UseRegex = useRegex;
        }

        public static readonly DependencyProperty IsModifiedProperty = TextEditor.IsModifiedProperty.AddOwner(_typeofSelf);
        public bool IsModified
        {
            get { return (bool)GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }

        public static readonly DependencyProperty WordWrapProperty = TextEditor.WordWrapProperty.AddOwner(_typeofSelf);
        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        public static readonly DependencyProperty ShowLineNumbersProperty = TextEditor.ShowLineNumbersProperty.AddOwner(_typeofSelf);
        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = TextEditor.IsReadOnlyProperty.AddOwner(_typeofSelf);
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsCodeCompletionProperty = DependencyProperty.Register("IsCodeCompletion", typeof(bool), _typeofSelf);
        public bool IsCodeCompletion
        {
            get { return (bool)GetValue(IsCodeCompletionProperty); }
            set { SetValue(IsCodeCompletionProperty, value); }
        }

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(double), _typeofSelf, new PropertyMetadata(1d, OnDelayPropertyChanged));
        public double Delay
        {
            get { return (double)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        private static void OnDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var delay = (double)e.NewValue;

            if (ctrl._timer != null)
                ctrl._timer.Interval = TimeSpan.FromSeconds(Math.Max(1, delay));
        }

        public static readonly DependencyProperty GenerateCompletionDataProperty =
           DependencyProperty.Register("GenerateCompletionData", typeof(Func<string, string, string, List<string>>), _typeofSelf);
        public Func<string, string, string, List<string>> GenerateCompletionData
        {
            get { return (Func<string, string, string, List<string>>)GetValue(GenerateCompletionDataProperty); }
            set { SetValue(GenerateCompletionDataProperty, value); }
        }

        #endregion

        #region Override

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (Focusable && _partTextEditor != null)
                _partTextEditor.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Left:
                    {
                        if (_partTextEditor.SelectionLength < 2 || Keyboard.Modifiers != ModifierKeys.None)
                            return;

                        _partTextEditor.TextArea.Caret.Offset = _partTextEditor.SelectionStart + 1;
                        break;
                    }
                case Key.Right:
                    {
                        if (_partTextEditor.SelectionLength < 2 || Keyboard.Modifiers != ModifierKeys.None)
                            return;

                        _partTextEditor.TextArea.Caret.Offset = _partTextEditor.SelectionStart + _partTextEditor.SelectionLength - 1;
                        break;
                    }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UnsubscribeEvents();

            _partTextEditor = GetTemplateChild(TextEditorTemplateName) as TextEditor;

            if (_partTextEditor != null)
            {
                _partTextEditor.TextChanged += _partTextEditor_TextChanged;
                _partTextEditor.TextArea.TextEntering += TextArea_TextEntering;
                _partTextEditor.TextArea.TextEntered += TextArea_TextEntered;
                _partTextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
                DataObject.AddPastingHandler(_partTextEditor, _partTextEditor_Pasting);


                _partTextEditor.Options = new TextEditorOptions { ConvertTabsToSpaces = true };
                _partTextEditor.TextArea.SelectionCornerRadius = 0;
                _partTextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
                _partTextEditor.TextArea.SelectionBorder = null;
                _partTextEditor.TextArea.SelectionForeground = null;

                if (AllowSearch)
                    InstallSearchPanel();
            }

            InitSearchPanel();
        }

        #endregion

        #region Event

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            RaiseEvent(new RoutedEventArgs(DelayArrivedEvent));
        }

        private void _searchPanel_SearchOptionsChanged(object sender, SearchOptionsChangedEventArgs e)
        {
            IsMatchCase = e.MatchCase;
            IsWholeWords = e.WholeWords;
            UseRegex = e.UseRegex;
        }

        private void _partTextEditor_TextChanged(object sender, EventArgs e)
        {
            RefreshFoldings();
            RaiseEvent(new RoutedEventArgs(TextChangedEvent));

            if (_disabledTimer)
            {
                _disabledTimer = false;
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private void _partTextEditor_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            //for invoke DelayArrivedEvent
            _disabledTimer = false;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            if (_partTextEditor == null)
                return;

            LineNumber = _partTextEditor.TextArea.Caret.Location.Line;
            LinePosition = _partTextEditor.TextArea.Caret.Location.Column;
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (IsReadOnly)
                return;

            var codeCompletion = IsCodeCompletion && GenerateCompletionData != null;
            var offset = _partTextEditor.TextArea.Caret.Offset;

            switch (e.Text)
            {
                case ".": //get attributes
                    {
                        if (!codeCompletion)
                            break;

                        var element = GetElementInFrontOfSymbol(offset - 1);
                        if (!string.IsNullOrEmpty(element))
                            ShowCompletionWindow(GenerateCompletionData(null, element, null));

                        break;
                    }

                case " ": //get attributes
                    {
                        if (!codeCompletion)
                            break;

                        if (!IsEvenQuoteInElement(offset - 2))
                            return;

                        var element = GetElement(offset - 1);
                        if (!string.IsNullOrEmpty(element))
                            ShowCompletionWindow(GenerateCompletionData(null, element, null));

                        break;
                    }

                case "<": //get child elements
                    {
                        if (!codeCompletion)
                            break;

                        var parentOffset = -1;
                        var parentElement = GetParentElement(offset - 2, ref parentOffset);

                        var elements = GenerateCompletionData(parentElement, null, null);
                        if (elements != null && elements.Count > 0)
                        {
                            elements.Insert(0, @"!--");
                            elements.Insert(1, @"![CDATA[");
                            ShowCompletionWindow(elements);
                        }

                        break;
                    }

                case "{":
                    {
                        if (!codeCompletion)
                            break;

                        InsertText("}");
                        ShowCompletionWindow(new List<string> { "Binding", "DynamicResource", "StaticResource", "x:Null", "x:Static" });

                        break;
                    }

                case "=": //get values
                    {
                        if (!IsEvenQuoteInElement(offset - 2))
                            break;

                        InsertText("\"\"");

                        if (!codeCompletion)
                            break;

                        var element = GetElementAndAttributeInFrontOfSymbol(offset - 2);

                        if (element != null && !string.IsNullOrEmpty(element.Item1) && !string.IsNullOrEmpty(element.Item2))
                            ShowCompletionWindow(GenerateCompletionData(null, element.Item1, element.Item2));

                        break;
                    }

                case "\"": //get values
                    {
                        InsertText("\"");

                        if (!codeCompletion)
                            break;

                        if (offset > 1 && FindPreviousNonSpaceChars(offset - 1, 2) == "=\"")
                        {
                            var element = GetElementAndAttributeInFrontOfSymbol(offset - 2);

                            if (element != null && !string.IsNullOrEmpty(element.Item1) && !string.IsNullOrEmpty(element.Item2))
                                ShowCompletionWindow(GenerateCompletionData(null, element.Item1, element.Item2));
                        }

                        break;
                    }

                case ">":  // auto add </XXX>
                    {
                        if (!codeCompletion)
                            break;

                        if (FindPreviousNonSpaceChars(offset - 1, 2) == "/>")
                            break;

                        var element = GetElement(offset - 2);
                        if (!string.IsNullOrEmpty(element))
                        {
                            var insertStr = string.Format("</{0}>", element);
                            InsertText(insertStr, true, insertStr.Length);
                        }

                        break;
                    }

                case "/":
                    {
                        if (!codeCompletion)
                            break;

                        if (FindPreviousNonSpaceChars(offset - 1, 2) == "</")
                        {
                            var parentOffset = -1;
                            var parentElement = GetParentElement(offset - 3, ref parentOffset);
                            if (!string.IsNullOrEmpty(parentElement))
                                ShowCompletionWindow(new List<string> { parentElement + ">" });
                        }

                        break;
                    }

                case "\n":  // auto align or insert one space line
                    DealBreak();
                    break;
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (IsReadOnly || !IsCodeCompletion || _completionWindow == null)
                return;

            if (e.Text.Length > 0)
            {
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '!')
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void CompletionList_InsertionRequested(object sender, EventArgs e)
        {
            if (!IsCodeCompletion)
                return;

            var offset = _partTextEditor.TextArea.Caret.Offset;

            if (FindPreviousNonSpaceChars(offset - 1, 3) == "!--")
            {
                InsertText("-->", true, 3);
            }
            else if (FindPreviousNonSpaceChars(offset - 1, 8) == "![CDATA[")
            {
                InsertText("]]>", true, 3);
            }
        }

        #endregion

        #region IntelliSense

        private string FindPreviousNonSpaceChars(int startOffset, int charLength = 1, int minOffset = 0)
        {
            var foundChars = string.Empty;

            while (startOffset >= minOffset)
            {
                var curChar = _partTextEditor.Text[startOffset];

                if (!char.IsWhiteSpace(curChar))
                {
                    foundChars += curChar;
                    if (foundChars.Length == charLength)
                        break;
                }

                startOffset--;
            }

            if (string.IsNullOrEmpty(foundChars) || foundChars.Length == 1)
                return foundChars;

            return new string(foundChars.Reverse().ToArray());
        }

        private string FindNextNonSpaceChars(int startOffset, int charLength = 1, int maxOffset = -1)
        {
            var foundChars = string.Empty;
            var length = maxOffset < 0 ? _partTextEditor.Text.Length : (maxOffset + 1);

            while (startOffset < length)
            {
                var curChar = _partTextEditor.Text[startOffset];

                if (!char.IsWhiteSpace(curChar))
                {
                    foundChars += curChar;
                    if (foundChars.Length == charLength)
                        break;
                }

                startOffset++;
            }

            return foundChars;
        }

        private string GetParentElement(int startOffset, ref int parentOffset)
        {
            var foundEnd = false;
            var ignoreNextEndCount = 0;

            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];

                if (curChar == '>' && i > 0 && FindPreviousNonSpaceChars(i - 1) != "/")
                {
                    foundEnd = true;
                    continue;
                }

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                {
                    foundEnd = false;
                    ignoreNextEndCount++;

                    continue;
                }

                if (curChar == '<' && foundEnd)
                {
                    if (ignoreNextEndCount > 0)
                    {
                        foundEnd = false;
                        ignoreNextEndCount--;

                        continue;
                    }

                    parentOffset = i;
                    var element = "";
                    for (int j = i + 1; j <= startOffset; j++)
                    {
                        curChar = _partTextEditor.Text[j];
                        var isLetterOrPoint = char.IsLetter(curChar) || curChar == '.';

                        if (!string.IsNullOrEmpty(element) && !isLetterOrPoint)
                            return element;

                        if (isLetterOrPoint)
                            element += curChar;
                    }

                    if (!string.IsNullOrEmpty(element))
                        return element;
                }
            }

            return null;
        }

        private string GetElementInFrontOfSymbol(int startOffset)
        {
            var element = "";

            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];
                var isLetter = char.IsLetter(curChar);

                if (!string.IsNullOrEmpty(element) && !isLetter)
                    return new string(element.Reverse().ToArray());

                if (isLetter)
                    element += curChar;
            }

            return element;
        }

        private Tuple<string, string> GetElementAndAttributeInFrontOfSymbol(int startOffset)
        {
            var finishAttribute = false;
            var attribute = "";

            var element = "";

            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];

                //attribute
                var isLetter = char.IsLetter(curChar);
                if (!string.IsNullOrEmpty(attribute) && !isLetter && !finishAttribute)
                {
                    finishAttribute = true;
                    attribute = new string(attribute.Reverse().ToArray());
                }

                if (isLetter && !finishAttribute)
                    attribute += curChar;

                //element
                if (curChar == '>')
                    break;

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                    break;

                if (curChar == '<')
                {
                    for (int j = i + 1; j <= startOffset; j++)
                    {
                        curChar = _partTextEditor.Text[j];
                        var isLetterOrPoint = char.IsLetter(curChar) || curChar == '.';

                        if (!string.IsNullOrEmpty(element) && !isLetterOrPoint)
                            break;

                        if (isLetterOrPoint)
                            element += curChar;
                    }
                }
            }

            return new Tuple<string, string>(element, attribute);
        }

        private string GetElement(int startOffset)
        {
            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];
                if (curChar == '>')
                    return null;

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                    return null;

                if (curChar == '<')
                {
                    var element = "";
                    for (int j = i + 1; j <= startOffset; j++)
                    {
                        curChar = _partTextEditor.Text[j];
                        var isLetterOrPoint = char.IsLetter(curChar) || curChar == '.';

                        if (!string.IsNullOrEmpty(element) && !isLetterOrPoint)
                            return element;

                        if (isLetterOrPoint)
                            element += curChar;
                    }

                    if (!string.IsNullOrEmpty(element))
                        return element;
                }
            }

            return null;
        }

        private void FormatIndentInElement(string lastLineText)
        {
            if (string.IsNullOrWhiteSpace(lastLineText))
                return;

            var curOffset = _partTextEditor.TextArea.Caret.Offset;

            for (int i = 0; i < lastLineText.Length; i++)
            {
                var curChar = lastLineText[i];
                if (char.IsWhiteSpace(curChar))
                    continue;

                if (curChar == '<')
                {
                    var startOffset = i;

                    var foundSpace = false;
                    for (int j = i + 1; j < lastLineText.Length; j++)
                    {
                        curChar = lastLineText[j];
                        var isWhiteSpace = char.IsWhiteSpace(curChar);

                        if (!isWhiteSpace)
                        {
                            if (!foundSpace)
                                continue;
                            else
                            {
                                startOffset = j;
                                break;
                            }
                        }
                        else
                        {
                            foundSpace = true;
                        }
                    }

                    if (startOffset == i)
                        _partTextEditor.Document.Insert(curOffset, string.Join("", Enumerable.Repeat(" ", 4)));
                    else
                        _partTextEditor.Document.Insert(curOffset, string.Join("", Enumerable.Repeat(" ", startOffset - i)));
                }

                break;
            }
        }

        private void DealBreak()
        {
            var docLine = _partTextEditor.Document.GetLineByNumber(_partTextEditor.TextArea.Caret.Line - 1);
            if (docLine == null)
                return;

            var lineText = _partTextEditor.Document.GetText(docLine.Offset, docLine.Length);
            if (string.IsNullOrWhiteSpace(lineText))
                return;

            var curOffset = _partTextEditor.TextArea.Caret.Offset;
            if (curOffset == 0)
                return;

            var element = GetElement(curOffset - 1);
            if (!string.IsNullOrEmpty(element))
            {
                FormatIndentInElement(lineText);
                return;
            }

            var parentOffset = -1;
            var parentElement = GetParentElement(curOffset - 1, ref parentOffset);
            if (!string.IsNullOrEmpty(parentElement))
            {
                var parentColumn = _partTextEditor.Document.GetLocation(parentOffset).Column;
                var curColumn = _partTextEditor.TextArea.Caret.Column;

                var targetColumn = parentColumn + 4;

                if (targetColumn > curColumn)
                    _partTextEditor.Document.Insert(curOffset, string.Join("", Enumerable.Repeat(" ", targetColumn - curColumn)));
                else if (targetColumn < curColumn)
                    _partTextEditor.TextArea.Caret.Offset -= (curColumn - targetColumn);

                curOffset = _partTextEditor.TextArea.Caret.Offset;
                var thisLine = _partTextEditor.Document.GetLineByOffset(curOffset);

                if (FindNextNonSpaceChars(curOffset, 2, thisLine.EndOffset) == "</")
                {
                    _partTextEditor.Document.Insert(curOffset, "\n" + (parentColumn > 0 ? string.Join("", Enumerable.Repeat(" ", parentColumn - 1)) : ""));
                    _partTextEditor.TextArea.Caret.Offset = curOffset;
                }
            }
        }

        private bool IsEvenQuoteInElement(int startOffset)
        {
            var quoteCount = 0;

            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];
                if (curChar == '>')
                    return false;

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                    return false;

                if (curChar == '\"')
                    quoteCount++;

                if (curChar == '<')
                    return quoteCount % 2 == 0;
            }

            return false;
        }

        #endregion

        #region Func

        public void LoadSyntaxHighlighting(string fileName)
        {
            if (!FileHelper.Exists(fileName) || _partTextEditor == null)
                return;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new XmlTextReader(fs))
                {
                    _partTextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        public void Redo()
        {
            _partTextEditor?.Redo();
        }

        public void Undo()
        {
            _partTextEditor?.Undo();
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= _timer_Tick;
                _timer = null;
            }

            _completionWindow?.Close();

            UninstallFolding();
            UninstallSearchPanel();
            UnsubscribeEvents();
        }

        private void InstallSearchPanel()
        {
            if (_partTextEditor == null || _searchPanel != null)
                return;

            _searchPanel = SearchPanel.Install(_partTextEditor.TextArea);
        }

        private void UninstallSearchPanel()
        {
            _searchPanel?.Uninstall();
            _searchPanel = null;
        }

        private void InitSearchPanel()
        {
            if (_searchPanel != null)
            {
                IsMatchCase = _searchPanel.MatchCase;
                IsWholeWords = _searchPanel.WholeWords;
                UseRegex = _searchPanel.UseRegex;

                _searchPanel.MarkerBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6B94D"));
                _searchPanel.SearchOptionsChanged += _searchPanel_SearchOptionsChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_searchPanel != null)
            {
                _searchPanel.SearchOptionsChanged -= _searchPanel_SearchOptionsChanged;
                _searchPanel.Uninstall();
            }

            if (_partTextEditor != null)
            {
                _partTextEditor.TextChanged -= _partTextEditor_TextChanged;
                _partTextEditor.TextArea.TextEntering -= TextArea_TextEntering;
                _partTextEditor.TextArea.TextEntered -= TextArea_TextEntered;
                _partTextEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
                DataObject.RemovePastingHandler(_partTextEditor, _partTextEditor_Pasting);
            }
        }

        private void AfterCloseCompletionWindow(EventHandler handler = null)
        {
            if (_completionWindow != null)
            {
                _completionWindow.CompletionList.InsertionRequested -= CompletionList_InsertionRequested;

                _completionWindow.Closed -= handler;
                _completionWindow.Resources = null;
                _completionWindow = null;
            }
        }

        private void InsertText(string text, bool caretFallBack = true, int fallbackLength = 1)
        {
            _partTextEditor.TextArea.Document.Insert(_partTextEditor.TextArea.Caret.Offset, text);

            if (caretFallBack)
                _partTextEditor.TextArea.Caret.Column -= fallbackLength;
        }

        private void ShowCompletionWindow(List<string> showDatas)
        {
            if (showDatas == null || showDatas.Count == 0)
                return;

            _completionWindow = new CompletionWindowEx(_partTextEditor.TextArea);
            _completionWindow.Resources = Resources;
            _completionWindow.MinWidth = 300;
            _completionWindow.MaxHeight = 300;
            _completionWindow.SizeToContent = SizeToContent.WidthAndHeight;

            _completionWindow.CompletionList.InsertionRequested += CompletionList_InsertionRequested;

            var datas = _completionWindow.CompletionList.CompletionData;
            showDatas.ForEach(d => datas.Add(new EditorCompletionData(d)));

            EventHandler handler = null;
            handler = (s, e) =>
            {
                AfterCloseCompletionWindow(handler);
            };

            _completionWindow.Closed -= handler;
            _completionWindow.Closed += handler;
            _completionWindow.Show();
        }

        private void RefreshFoldings()
        {
            if (_partTextEditor == null || !AllowFolding)
                return;

            InstallFolding();

            _foldingStrategy.UpdateFoldings(_foldingManager, _partTextEditor.Document);
        }

        private void InstallFolding()
        {
            if (_foldingManager != null || _partTextEditor == null)
                return;

            _foldingManager = FoldingManager.Install(_partTextEditor.TextArea);
        }

        private void UninstallFolding()
        {
            if (_foldingManager != null)
            {
                FoldingManager.Uninstall(_foldingManager);
                _foldingManager = null;
            }
        }

        #endregion
    }
}
