using Mappah.Configuration;
using Mappah.Resolution;
using Mappah.Tests.Models;
using Mappah.Tests.Models.Mappah.Tests.Models;

namespace Mappah.Tests
{
    public class BasicMappingTests
    {
        private readonly IMapResolver _mapper = new DefaultMapResolver();

        public BasicMappingTests()
        {
            MapperConfiguration.Create<User, UserDto>()
                .For(dest => dest.FullName, src => src.FirstName + " " + src.LastName)
                .Skip(dest => dest.Secret)
                .Build();

            MapperConfiguration.Create<InnerObject, InnerObjectDto>()
                .Build();
            MapperConfiguration.Create<DifferentInner, DifferentInnerDto>()
                .For(dest => dest.AnotherValue, src => src.SomeValue)
                .Build();
            MapperConfiguration.Create<OuterObject, OuterObjectDto>()
                .For(dest => dest.ManualInnerDto, src => src.DifferentInner)
                .Build();
        }

        [Fact]
        public void Should_Map_Nested_Automatic_And_Manual()
        {
            // Arrange
            var outer = new OuterObject
            {
                OuterField = 100,
                Inner = new InnerObject
                {
                    TestField = 999
                },
                DifferentInner = new DifferentInner
                {
                    SomeValue = "Mapped manually"
                }
            };

            // Act
            var dto = _mapper.Map<OuterObjectDto>(outer);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(100, dto.OuterField);
            Assert.NotNull(dto.Inner);
            Assert.Equal(999, dto.Inner.TestField);
            Assert.NotNull(dto.ManualInnerDto);
            Assert.Equal("Mapped manually", dto.ManualInnerDto.AnotherValue);
        }

        [Fact]
        public void Should_Map_Single_Object()
        {
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Secret = "topsecret"
            };

            var result = _mapper.Map<UserDto, User>(user);

            Assert.Equal("John Doe", result.FullName);
            Assert.Null(result.Secret);
        }

        [Fact]
        public void Should_Map_Collection_Of_Objects()
        {
            var users = new List<User>
            {
                new User { FirstName = "John", LastName = "Doe", Secret = "topsecret" },
                new User { FirstName = "Jane", LastName = "Smith", Secret = "classified" }
            };

            var results = _mapper.Map<UserDto, User>(users);

            Assert.Collection(results,
                item => Assert.Equal("John Doe", item.FullName),
                item => Assert.Equal("Jane Smith", item.FullName));
        }
    }
}
