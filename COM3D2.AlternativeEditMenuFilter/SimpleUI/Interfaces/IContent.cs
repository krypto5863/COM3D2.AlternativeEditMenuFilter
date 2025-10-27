namespace COM3D2.SimpleUI
{
    public interface IContent
    {
        string text { get; set; }
        UITexture texture { get; set; }
        string tooltip { get; set; }
    }
}