[![Paypal](https://img.shields.io/badge/Paypal-Donate-success?style=for-the-badge&logo=paypal)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=ELFGQ65FJFPVQ&currency_code=CAD&source=url) 
[![Discord](https://img.shields.io/discord/674782527139086350?color=7289DA&label=Discord&style=for-the-badge&logo=discord)](https://discord.gg/ATCM64M)
[![DockerHub](https://img.shields.io/badge/Docker-Hub-%23099cec?style=for-the-badge&logo=docker)](https://hub.docker.com/r/darkalfx/requestrr)
[![DockerHub](https://img.shields.io/badge/GitHub-Repo-lightgrey?style=for-the-badge&logo=github)](https://github.com/darkalfx/requestrr/)
[![DevBoard](https://img.shields.io/badge/Dev-Board-%23233240?style=for-the-badge&logo=gitkraken)](https://app.gitkraken.com/glo/board/Xmfwg65zLQARKZdL)
[![Timeline](https://img.shields.io/badge/Project-Timeline-%23233240?style=for-the-badge&logo=gitkraken)](https://timelines.gitkraken.com/timeline/0656f1edc0dd4a2191406c62343c22c1)


Requestrr 
=================

![logo](https://i.imgur.com/0UzLYvw.png)

Requestrr is a chatbot used to simplify using services like Sonarr/Radarr/Ombi via the use of chat!  

### Features

- Ability to request content via Discord
- Apple's Siri integration
- Users can get notified when their requests complete
- Sonarr/Radarr V2/V3 integration
- Ombi V3 integration with support for per user roles/quotas
- Fully configurable via a web portal


Installation & Configuration
==================

Refer to the Wiki for detailed steps:
https://github.com/darkalfx/requestrr/wiki

Docker Setup & Start
==================

Open a command prompt/terminal and then use the following command create and start the container:

    docker run --name requestrr \
      -p 4545:4545 \
      -v path to config:/root/config \
      --restart=unless-stopped \
      darkalfx/requestrr

You can also choose to run the container as a different user. See [docker run](https://docs.docker.com/engine/reference/run/#user) reference for how to set the user for your container.

Then simply access the web portal at http://youraddress:4545/ to create your admin account, then you can configure everything through the web portal.

Once you have configured the bot and invited it to your Discord server, simply type **!help** to see all available commands.

### docker-compose

```
---
version: "2.1"
services:
  requestrr:
    image: darkalfx/requestrr
    container_name: requestrr
    volumes:
      - path to config:/config
    ports:
      - 4545:4545
    restart: unless-stopped
```

Build Instructions
==================

### Setup
* npm. You can install npm via brew on mac. On mac you might need to re install your xcode command line tools. See https://medium.com/flawless-app-stories/gyp-no-xcode-or-clt-version-detected-macos-catalina-anansewaa-38b536389e8d.
* .netcore sdk. Download [SDK 3.1.201](https://dotnet.microsoft.com/download/dotnet-core/3.1) installer for your environment.

### Building
* In directory [Requestrr.WebApi/ClientApp](Requestrr.WebApi/ClientApp) run `npm run install:clean`. You can safefly exit it once the build is done running. For example
```
./src/components/Inputs/MultiDropdown.jsx
  Line 29:  Expected '!==' and instead saw '!='  eqeqeq
  Line 53:  No duplicate props allowed           react/jsx-no-duplicate-props

./src/views/TvShows.jsx
  Line 38:  'Input' is defined but never used  no-unused-vars

./src/views/Movies.jsx
  Line 38:  'Input' is defined but never used  no-unused-vars

Search for the keywords to learn more about each warning.
To ignore, add // eslint-disable-next-line to the line before.
```

* In directory [Requestrr.WebApi](Requestrr.WebApi) run `dotnet publish -c release -o publish -r linux-x64`.
