using Workflow.Bibliography;
using Workflow.Formats;

namespace Workflow
{
    static class Program
    {
        static void Main()
        {
            //ValidationBib.Validate();
            //ValidationRef.Validate();
            
            //Transformations.Doc2Md();
            //Markdown.PostProcessing();
            //ValidationMd.PostProcessing();
            
            //Transformations.Md2Tex();
            //Latex.PostProcessing();
            Transformations.Tex2Docx();
        }
    }
}