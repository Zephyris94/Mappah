# Mappah

[![NuGet version](https://img.shields.io/nuget/v/Mappah.svg?label=Mappah&logo=nuget&style=flat-square)](https://www.nuget.org/packages/Mappah)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![Build](https://img.shields.io/github/actions/workflow/status/Zephyris94/Mappah/minor.yml?branch=minor&label=Build&logo=github&style=flat-square)](https://github.com/Zephyris94/Mappah/actions/workflows/minor.yml)
[![Repo stars](https://img.shields.io/github/stars/Zephyris94/Mappah?style=flat-square)](https://github.com/Zephyris94/Mappah/stargazers)

Minimalist object-to-object mapper for .NET.

---

## 📦 Installation

Install via NuGet:

```bash
dotnet add package Mappah
```

If you want to integrate with ASP.NET Core Dependency Injection:

```bash
dotnet add package Mappah.Extensions
```

---

## 🚀 Quick Start

Define a mapping:

```csharp
MapperConfiguration.Create<User, UserDto>()
    .For(dest => dest.FullName, src => src.FirstName + " " + src.LastName)
    .Skip(dest => dest.Password);
```

Use the mapper:

```csharp
var mapper = new DefaultMapperResolver();

var user = new User { FirstName = "John", LastName = "Doe", Password = "123456" };
var userDto = mapper.Map<UserDto, User>(user);

// userDto.FullName == "John Doe"
// userDto.Password == null
```

---

## 💡 Features

- Auto-mapping properties by name
- Custom property mapping with expressions
- Ignoring properties
- Reverse mapping support (`WithReverse()`)
- ASP.NET Core integration via `AddMappah()`

---

## 🔧 ASP.NET Core Integration

```csharp
builder.Services.AddMappah();
```

Inject `IMapResolver` anywhere:

```csharp
public class MyService
{
    private readonly IMapResolver _mapper;

    public MyService(IMapResolver mapper)
    {
        _mapper = mapper;
    }
}
```

---

## 📝 License

Licensed under the [MIT License](https://opensource.org/licenses/MIT).
