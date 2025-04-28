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

            MapperConfigurationBuilder.Build();
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
