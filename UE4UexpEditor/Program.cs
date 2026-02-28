using NDesk.Options;
using UE4UexpEditor.Psychonauts;

namespace UE4UexpEditor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string input = null;
            string output = null;
            string rebuild = null;
            string version = "1.0";
            bool import = false;

            var options = new OptionSet()
                .Add("in=", value => input = value)
                .Add("out=", value => output = value)
                .Add("rebuild=", value => rebuild = value)
                .Add("version=", value => version = value)
                .Add("import", value => import = true);

            options.Parse(args);
            
            var editor = new PsychonautsEditor(input, output, rebuild, version);

            if (!import)
            {
                editor.ExtractText();
            }
            else
            {
                editor.CreateUexp();
            }
        }
    }
}
