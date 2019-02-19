# VkNet.AudioBypass
Расширение для [VkNet](https://github.com/vknet/vk) для обхода ограничения к методам **Audio**.

[![NuGet](https://img.shields.io/nuget/v/VkNet.AudioBypassService.svg)](https://www.nuget.org/packages/VkNet.AudioBypassService/)
[![NuGet](https://img.shields.io/nuget/dt/VkNet.AudioBypassService.svg)](https://www.nuget.org/packages/VkNet.AudioBypassService/)

## Как использовать?

``` C#
using VkNet.AudioBypassService.Extensions;

//...

var services = new ServiceCollection();
services.AddAudioBypass(); 

var api = new VkApi(services);

// Авторизируемся для получения токена валидного для вызова методов Audio
api.Authorize(new ApiAuthParams
{
    Login = "login",
    Password = "password"
});
```
