namespace MVC.Models
{
    public class Summernote
    {
        public Summernote(string iDEditor, bool loadLibrary = true)
        {
            IDEditor = iDEditor;
            LoadLibrary = loadLibrary;
        }

        public string IDEditor { get; set; }
        public bool LoadLibrary { get; set; } = true;

        public int Height { get; set; } = 200;
        public string Toolbar { get; set; } = @"
            [
                ['style', ['style']],
                ['font', ['bold', 'underline', 'clear']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link', 'picture', 'video', 'elfinder']],
                ['view', ['fullscreen', 'codeview', 'help']]
            ]
        ";
    }
}