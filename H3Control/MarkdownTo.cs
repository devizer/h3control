namespace H3Control
{

    class MarkdownTo
    {
        public static string Html(string md)
        {
/*
            Markdown mark = new Markdown();
            string text = mark.Transform(md);
*/
            string text = CommonMark.CommonMarkConverter.Convert(md);
            return text;
        }
    }
}