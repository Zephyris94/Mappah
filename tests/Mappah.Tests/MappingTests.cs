using Mappah.Configuration;
using Mappah.Resolution;
using Mappah.Tests.Models;

namespace Mappah.Tests
{
    public class MappingTests
    {
        private readonly IMapResolver _mapper = DefaultMapResolver.Instance;

        public MappingTests()
        {
            MapperConfigurationBuilder.Create<PrimitiveSource, PrimitiveDestination>();

            MapperConfigurationBuilder.Create<NestedSource, NestedDestination>();

            MapperConfigurationBuilder.Create<InnerSource, InnerDestination>();

            MapperConfigurationBuilder.Create<CustomSource, CustomDestination>()
                .For(dest => dest.FullName, src => src.FirstName + " " + src.LastName + " #" + src.Number);

            MapperConfigurationBuilder.Create<SkipSource, SkipDestination>()
                .Skip(dest => dest.Secret);

            MapperConfigurationBuilder.Create<ToReverseSource, ReversedSource>()
                .Skip(dest => dest.Secret)
                .WithReverse()
                .For(dest => dest.Name, src => src.Name + " 123")
                .Skip(dest => dest.Age);

            MapperConfigurationBuilder.Create<PrimitiveModel, TargetPrimitiveModel>();

            MapperConfigurationBuilder.Create<NestedCollectionSource, NestedCollectionTarget>();

            MapperConfigurationBuilder.Create<NestedManualCollectionSource, NestedManualCollectionTarget>()
                .WithCollection(dest => dest.Collection, src => src.Collection);

            MapperConfigurationBuilder.Build();
        }

        [Fact]
        public void Manual_Nested_Collection_Mapping()
        {
            var src = new NestedManualCollectionSource();
            src.Collection = new List<PrimitiveSource>();
            for (int i = 0; i < 10; i++)
            {
                src.Collection.Add(new PrimitiveSource
                {
                    Age = i,
                    Name = (i + 10).ToString()
                });
            }

            var result = _mapper.Map<NestedManualCollectionTarget>(src);

            Assert.Equal(src.Collection.Count, result.Collection.Count);
            for (int i = 0; i < src.Collection.Count; i++)
            {
                var srcElem = src.Collection[i];
                var tarElem = result.Collection[i];

                Assert.Equal(tarElem.Name, srcElem.Name);
                Assert.Equal(tarElem.Age, srcElem.Age);
            }
        }

        [Fact]
        public void Collection_Mapping()
        {
            var col = new List<PrimitiveSource>();
            for (int i = 0; i < 10; i++)
            {
                col.Add(new PrimitiveSource
                {
                    Age = i,
                    Name = (i + 10).ToString()
                });
            };

            var result = _mapper.Map<List<PrimitiveDestination>>(col);

            Assert.Equal(col.Count, result.Count);
            for (int i = 0; i < col.Count; i++)
            {
                var srcElem = col[i];
                var tarElem = result[i];

                Assert.Equal(tarElem.Name, srcElem.Name);
                Assert.Equal(tarElem.Age, srcElem.Age);
            }
        }

        [Fact]
        public void Nested_Collection_Mapping()
        {
            var src = new NestedCollectionSource();
            src.Collection = new List<PrimitiveSource>();
            for(int i = 0; i < 10; i++)
            {
                src.Collection.Add(new PrimitiveSource
                {
                    Age = i,
                    Name = (i + 10).ToString()
                });
            }

            var result = _mapper.Map<NestedCollectionTarget>(src);

            Assert.Equal(src.Collection.Count, result.Collection.Count);
            for(int i = 0; i < src.Collection.Count; i++)
            {
                var srcElem = src.Collection[i];
                var tarElem = result.Collection[i];

                Assert.Equal(tarElem.Name, srcElem.Name);
                Assert.Equal(tarElem.Age, srcElem.Age);
            }
        }

        [Fact]
        public void Guid_Mapping()
        {
            var guidModel = new PrimitiveModel
            {
                GuidProperty = Guid.NewGuid(),
                ByteProperty = 1,
                DecimalProperty = 2.0M,
                DoubleProperty = 3.0,
                FloatProperty = 4.0f,
                IntProperty = 5,
                ShortProperty = 6,
                StructProp = new CustomStruct
                {
                    StructProp = 7,
                }
            };

            var target = _mapper.Map<TargetPrimitiveModel>(guidModel);

            Assert.NotNull(target);
            Assert.Equal(guidModel.GuidProperty, target.GuidProperty);
            Assert.Equal(guidModel.ByteProperty, target.ByteProperty);
            Assert.Equal(guidModel.DecimalProperty, target.DecimalProperty);
            Assert.Equal(guidModel.DoubleProperty, target.DoubleProperty);
            Assert.Equal(guidModel.FloatProperty, target.FloatProperty);
            Assert.Equal(guidModel.IntProperty, target.IntProperty);
            Assert.Equal(guidModel.ShortProperty, target.ShortProperty);
            Assert.Equal(guidModel.StructProp.StructProp, target.StructProp.StructProp);
        }

        [Fact]
        public void Reverse_Mapping()
        {
            var source = new ToReverseSource
            {
                Name = "ATest",
                Age = 18,
                Secret = "qwerty"
            };

            var dest = _mapper.Map<ReversedSource>(source);

            Assert.Null(dest.Secret);
            Assert.Equal(source.Name, dest.Name);
            Assert.Equal(source.Age, dest.Age);

            dest.Secret = "reversed";
            var reversedSource = _mapper.Map<ToReverseSource>(dest);
            
            Assert.Equal(0, reversedSource.Age);
            Assert.Equal(dest.Secret, reversedSource.Secret);
            Assert.Equal(dest.Name + " 123", reversedSource.Name );
        }

        [Fact]
        public void Manual_PrimitiveMapping()
        {
            var src = new CustomSource { FirstName = "John", LastName = "Doe", Number = 7 };
            var dest = _mapper.Map<CustomDestination, CustomSource>(src);

            Assert.Equal("John Doe #7", dest.FullName);
        }

        [Fact]
        public void Auto_PrimitiveMapping()
        {
            var src = new PrimitiveSource { Age = 30, Name = "Alice" };
            var dest = _mapper.Map<PrimitiveDestination, PrimitiveSource>(src);

            Assert.Equal(30, dest.Age);
            Assert.Equal("Alice", dest.Name);
        }

        [Fact]
        public void Manual_ComplexMapping()
        {
            var src = new NestedSource { Id = 1, Inner = new InnerSource { Value = "Nested" } };
            var dest = _mapper.Map<NestedDestination, NestedSource>(src);

            Assert.Equal(1, dest.Id);
            Assert.NotNull(dest.Inner);
            Assert.Equal("Nested", dest.Inner.Value);
        }

        [Fact]
        public void Auto_ComplexMapping()
        {
            var src = new InnerSource { Value = "InnerValue" };
            var dest = _mapper.Map<InnerDestination, InnerSource>(src);

            Assert.Equal("InnerValue", dest.Value);
        }

        [Fact]
        public void Skip_Property()
        {
            var src = new SkipSource { Important = "Data", Secret = "Don't Map Me" };
            var dest = _mapper.Map<SkipDestination, SkipSource>(src);

            Assert.Equal("Data", dest.Important);
            Assert.Null(dest.Secret);
        }
    }
}
