using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Helper
{
    public class TruncateLongLines : VisualLineElementGenerator
    {
        const int maxLength = 2000;
        const string ellipsis = "...";
        const int charactersAfterEllipsis = 100;

        public override int GetFirstInterestedOffset(int startOffset)
        {
            DocumentLine line = CurrentContext.VisualLine.LastDocumentLine;
            if (line.Length > maxLength)
            {
                int ellipsisOffset = line.Offset + maxLength - charactersAfterEllipsis - ellipsis.Length;
                if (startOffset <= ellipsisOffset)
                    return ellipsisOffset;
            }
            return -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            return new FormattedTextElement(ellipsis, CurrentContext.VisualLine.LastDocumentLine.EndOffset - offset - charactersAfterEllipsis);
        }
    }
}
