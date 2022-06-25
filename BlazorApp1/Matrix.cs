namespace BlazorApp1
{
    public record Matrix
    {
        public List<Row> Rows { get; set; } = new();

        public record Row(double[] Data)
        {
        }
    }
}
