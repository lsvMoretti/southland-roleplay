namespace Server.Extensions.TextLabel
{
    public static class TextLabelStaticExtension
    {
        public static void Add(this TextLabel textLabel)
        {
            TextLabelHandler.TextLabels.Add(textLabel);
            TextLabelHandler.OnTextLabelAdded(textLabel);
        }

        public static void Remove(this TextLabel textLabel)
        {
            TextLabelHandler.TextLabels.Remove(textLabel);
            TextLabelHandler.OnTextLabelRemoved(textLabel);
        }
    }
}