using Mappah.Configuration;
using Mappah.Resolution;
using Mappah.Tests.Models;

namespace Mappah.Tests
{
    public class BasicMappingTests
    {
        private readonly IMapResolver _mapper = new DefaultMapResolver();

        public BasicMappingTests()
        {
            MapperConfiguration.Create<User, UserDto>()
                .For(dest => dest.FullName, src => src.FirstName + " " + src.LastName)
                .Skip(dest => dest.Secret);
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
