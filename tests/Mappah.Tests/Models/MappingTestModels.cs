namespace Mappah.Tests.Models
{
    public class PrimitiveSource
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    public class PrimitiveDestination
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    public class NestedSource
    {
        public int Id { get; set; }
        public InnerSource Inner { get; set; }
    }

    public class NestedDestination
    {
        public int Id { get; set; }
        public InnerDestination Inner { get; set; }
    }

    public class InnerSource
    {
        public string Value { get; set; }
    }

    public class InnerDestination
    {
        public string Value { get; set; }
    }

    public class CustomSource
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Number { get; set; }
    }

    public class CustomDestination
    {
        public string FullName { get; set; }
    }

    public class SkipSource
    {
        public string Important { get; set; }
        public string Secret { get; set; }
    }

    public class SkipDestination
    {
        public string Important { get; set; }
        public string Secret { get; set; }
    }

    public class ToReverseSource
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Secret { get; set; }
    }

    public class ReversedSource
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Secret { get; set; }
    }
}
