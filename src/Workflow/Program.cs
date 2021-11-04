using Workflow.Bibliography;
using Workflow.Formats;

namespace Workflow
{
    static class Program
    {
        static void Main()
        {
            // ValidationBib.Validate();
            // ValidationRef.Validate();

            // Transformations.Doc2Md();
            // Markdown.PostProcessing();

            // Transformations.Md2Tex();
            Latex.PostProcessing();

            // Transformations.Md2Docx();
        }
    }
}