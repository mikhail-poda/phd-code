using Workflow.Bibliography;
using Workflow.Formats;

namespace Workflow
{
    static class Program
    {
        static void Main()
        {
            //ValidationBib.Validate();

            //Transformations.Doc2Md();
            Markdown.PostProcessing();
            //ValidationMd.PostProcessing();
            //ValidationRef.Validate();
            
            //Transformations.Md2Tex();
            // Latex.PostProcessing();
            // Transformations.Md2Docx();
            
        }
    }
}