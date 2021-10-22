# BirthdayBot
## A self-hosted Discord Bot Birthday Reminder

BirthdayBot is a Discord bot that will send a birthday greeting to registered users birthdays when their birthday occurs. 

## Features
- Celebrates birthdays so no one will be forgotten
- Uses Giphy to send a special gif on someones birthday
- Can use text based birthday wish if you choose to not use Giphy
- Randomized picker for indecisiveness

### Get Started
Use the command prompt, navigate to the project and compile it for publishing. For my personal use, I used a Linux host, so to compile was:
```
dotnet publish -c release -r linux-arm --self-contained 
```
Some of these command parameters may change depending on the host OS you decide to use. See [this doc](https://docs.microsoft.com/en-us/dotnet/core/deploying/) for help with deploying the app. Be sure to have dotnet runtime installed on the host you choose.

### Configuration
The project contains a file named '_config.yml', which contains some settings for the bots. This is where you will enter your Discord bot token and Giphy API key.

### Running the project on the host
Once you have the project compiled, dotnet installed, and the config file completed, move the Publish folder to your host. Navigate the command terminal to the folder of the host and run the command
```
dotnet BirthdayBot.dll
```

See [Microsoft's detnet doc](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run) to learn more.

## Commands
The default prefix to execute the command is ~, but this can be changed in the _config.yml file. These examples will use the default prefix.

**Add someone's birthday**
~add <@mention> <birthday mm/dd>

**Add your own birthday**
~addme <birthday mm/dd>

**See upcoming birthdays**
~upcoming [or ~up] 

**Delete your birthday**
~delete [or ~del]

**List all birthdays**
~list

**To see a command dialog**
~help

**See Admin commands**
~adminhelp

## Note
Birthdays will be celebrated at the first minute of the chosen starting hour (so by default, the starting hour is 12, so the birthday message will occur at 12:00). Bare in mind, this time is based on the system time of the host.
