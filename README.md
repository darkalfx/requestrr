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

First pull the image from docker hub using docker push darkalfx/requestrr

Then use the following command create and start the container:

    docker run --name requestrr \
      -p 4545:4545 \
      -v path to config:/root/config \
      --restart=unless-stopped \
      darkalfx/requestrr

Then simply access the web portal at http://youraddress:4545/ to create your admin account, then you can configure everything through the web portal.

Once you have configured the bot and invited it to your Discord server, simply type **!help** to see all available commands.
