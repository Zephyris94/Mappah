namespace Mappah.Tests.Models
{
    public class NestedManualDifferentCollectionSource
    {
        public PrimitiveSource[] CollectionOne { get; set; }
        public List<PrimitiveSource> CollectionTwo { get; set; }
    }

    public class NestedManualDifferentCollectionTarget
    {
        public List<PrimitiveDestination> CollectionOne { get; set; }

        public PrimitiveDestination[] CollectionTwo { get; set; }
    }

    public class NestedManualCollectionSource
    {
        public List<PrimitiveSource> Collection { get; set; }
    }

    public class NestedManualCollectionTarget
    {
        public List<PrimitiveDestination> Collection { get; set; }
    }

    public class NestedCollectionSource
    {
        public List<PrimitiveSource> Collection { get; set; }
    }

    public class NestedCollectionTarget
    {
        public List<PrimitiveDestination> Collection { get; set; }
    }

    public class PrimitiveModel
    {
        public Guid GuidProperty { get; set; }

        public float FloatProperty { get; set; }

        public int IntProperty { get; set; }

        public byte ByteProperty { get; set; }

        public short ShortProperty { get; set; }

        public double DoubleProperty { get; set; }

        public decimal DecimalProperty { get; set; }

        public CustomStruct StructProp { get; set; }
    }

    public struct CustomStruct
    {
        public int StructProp { get; set; }
    }

    public class TargetPrimitiveModel
    {
        public Guid GuidProperty { get; set; }

        public float FloatProperty { get; set; }

        public int IntProperty { get; set; }

        public byte ByteProperty { get; set; }

        public short ShortProperty { get; set; }

        public double DoubleProperty { get; set; }

        public decimal DecimalProperty { get; set; }

        public CustomStruct StructProp { get; set; }
    }

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
