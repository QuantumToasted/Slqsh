# Slqsh
Pronounced "squish". No, I will not elaborate or budge on this.

Sqlsh is a drop-in supplement¹ to your [Disqord](https://github.com/Quahu/Disqord) bot of choice. It aims to be a standalone slash command interop library, capable of doing most² things currently possible via Discord's own slash command implementation.

Versioning will be a bit wonky because I simply CBA to implement actions to automate deployment. Bear with me.

# Installation
TODO

# Usage
Simple as. Simply chain `.AddSqlsh()` to your service collection implementation, most likely inside `.ConfigureServices()` in your host of choice.
Command implementations are nearly identical to Disqord.Bot's own system. All classes and attributes are directly in the `Sqlsh` namespace. Check out the [Sqlsh.Test](../../tree/master/Slqsh.Test/) folder for some sample code with better examples.

## Notes
¹ This library is only intended to be a band-aid, holdover temporary library until Disqord's own slash command implementation is implemented.

² This library is only meant to be a simple way to convert an existing text command bot to a slash command bot and is not meant as a full-featured suite of slash command support. Anything involving modals, buttons, or selects is highly unlikely to be implemented.
