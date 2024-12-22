Steam Workshop Description:

[h1][b]Adds BepInEx To Content Warning Through The Base Game Mod Loader[/b][/h1]

[h2]Why Should I Care?[/h2]
[list]
    [*]Better Pre-processor
    [*]Dependency Support with Correct Load Orders
[/list] 
[h2]That's Just What BepInEx Does, Why Make an Entire Mod to Install It?[/h2]
[list]
    [*]Automatically Installs The Mod Loader on First Boot after Subscribing
    [*]Comes Packaged with BepInEx Fix, to Resolve a Compatibility Issue Between It and the Game.
    [*]Allows for the Easy Upload of BepInEx Plugins (enable the setting within the mod settings menu)
    [*]Automatically Installs Subscribed BepInEx Plugins from the workshop
[/list] 

[h2]For Developers![/h2]
[h3]Uploading A Normal BepInEx Plugin:[/h3]
[list]
    [*]Make a directory within the BepInEx plugins folder
    [*]Put your mod with a preview(.png/.jpeg) image into the folder
    [*]Enable uploading BepInEx mods within the Mod settings
    [*]Select your plugin within the mod manger and upload
[/list]

[h2]Uploading More Advanced Plugins[/h2]
[h3]This Will Require the Steam Command Line[/h3]
[h3]Prepare Mod:[/h3]
[list]
    [*]Create a folder for your plugin, it can be anywhere
    [*]Create a sub folder named: 
[code]
Root
[/code] 
    [*]Any files put into this folder will be placed into the game relative to the root of the game.
[/list]
[h3]Upload Mod:[/h3]
    [*]Make a descriptor
[code]
"workshopitem"
{
    "appid"             "2881650"
    "publishedfileid"   "NUMBER"
    "contentfolder"     "PATH TO YOUR MOD"
    "previewfile"       "PATH TO YOUR PREVIEW FILE"
    "visibility"        "2"
    "title"             "TITLE"
    "description"       "DISCRIPTION"
    "changenote"        "Initial Release."
}
[/code]
    [*]Download SteamCMD: https://developer.valvesoftware.com/wiki/SteamCMD
    [*]Login To SteamCMD:
[code]
login ACCOUNT_NAME
[/code]
    [*]Upload the mod:
[code]
workshop_build_item PATH_TO_DESCRIPTOR
[/code]

[h3]Source Code:[/h3]
https://github.com/C0mputery/ContentLoader