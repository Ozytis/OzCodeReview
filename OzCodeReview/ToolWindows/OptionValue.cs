namespace OzCodeReview.ToolWindows
{
    public record OptionValue<T>
    {
        public T Value { get; set; }

        public string Label { get; set; }

        public override string ToString()
        {
            return this.Label;
        }
    }
}