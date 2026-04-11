Library for easy config management

Currently only works with objects, but can potentially work with dictionaries and lua tables

#### What it adds:
- it adds functions for config serialization, saving and networking
- config managers that save and network config automatically
- console command to inspect and edit config
- Methods for traversing config, getting and setting deep values
- Deep reactivity system to track config changes

Any object that inmplements IConfig instantly has all that functionality

#### useless details:
```
Functionality is located in ConfigCore, you can create it manually from any object
IConfig has static ConditionalWeakTable<IConfig, ConfigCore> with associated cores which are created on demand
Method calls are mapped to the core via extentions to IConfig
```

IConfig has all these methods like, save, load, sync, toXMl  
https://github.com/SomeRandomNoobKekeke/Baro-Junk/blob/main/CSharp/Shared/Libs/Config/IConfig/IConfigExtensions.cs

IConfig is IDirectlyLocatable, IDirectlyLocatable provides methods for traversing config  
https://github.com/SomeRandomNoobKekeke/Baro-Junk/blob/main/CSharp/Shared/Libs/Config/Locators/IDirectlyLocatable.cs

IDirectlyLocatable works with ConfigEntry, ConfigEntry is a wrapper around config and prop name that contains enough info to get and set value of a prop from anywhere

ConfigEntry is also IDirectlyLocatable so you can chain GetEntry calls

IConfig have some settings  
https://github.com/SomeRandomNoobKekeke/Baro-Junk/blob/main/CSharp/Shared/Libs/Config/Settings/ConfigSettings.cs

IConfig has managers that are listening for game hooks and call save, load and sync methods automatically  
They are activated when you choose config strategy

IConfig has ReactiveCore, it's just an object that hosts OnPropChanged and OnUpdate events  
IConfig is also IReactiveLocatable, it's similar to IDirectlyLocatable but returns ReactiveEntry  
https://github.com/SomeRandomNoobKekeke/Baro-Junk/blob/main/CSharp/Shared/Libs/Config/Locators/IReactiveLocatable.cs

ReactiveEntry has a link to original ReactiveCore and calls its OnPropChanged when its value is set  
Also Managers call OnUpdated when config is loaded or recieved over network


Check BaroJunk/Test/Config or jovian radiation rework for some examples





